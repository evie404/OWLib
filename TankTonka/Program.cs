using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataTool;
using DataTool.Helper;
using DataTool.JSON;
using TankLib;
using TankLib.STU;
using TankTonka.Formatters;
using TankTonka.Models;
using Utf8Json;
using Utf8Json.Resolvers;

namespace TankTonka {
    internal static class Program {
        private static string _outputDirectory;

        public static void Main(string[] args) {
            const string locale = "enUS";

            DataTool.Program.Flags = new ToolFlags {
                                                       OverwatchDirectory = args[0],
                                                       Language           = locale,
                                                       SpeechLanguage     = locale,
                                                       UseCache           = true,
                                                       CacheCDNData       = true
                                                   };

            _outputDirectory = args[1];
            var types = args.Skip(2)
                            .Select(x => ushort.Parse(x, NumberStyles.HexNumber))
                            .ToArray();
            if (types.Length == 0) types = null;

            DataTool.Program.InitStorage(false);

            CompositeResolver.RegisterAndSetAsDefault(new IJsonFormatter[] { new AssetRepoTypeFormatter(), new ResourceGUIDFormatter() }, new[] { StandardResolver.Default });

            if (types == null)
                foreach (var type in DataTool.Program.TrackedFiles)
                    ProcessType(type.Key);
            else
                foreach (var type in types)
                    ProcessType(type);

            using (Stream outputFile = File.OpenWrite(Path.Combine(_outputDirectory, "missing.json"))) {
                outputFile.SetLength(0);
                var buf = JsonSerializer.PrettyPrintByteArray(JsonSerializer.Serialize(IO.MissingKeyLog.ToList()));
                outputFile.Write(buf, 0, buf.Length);
            }
        }

        private static void ProcessType(ushort type) {
            if (!DataTool.Program.TrackedFiles.ContainsKey(type)) return;

            var manifest = new TypeManifest { Type = (Common.AssetRepoType) type };

            if (TypeClassifications.STUv2.Contains(type))
                ProcessTypeSTUv2(type, manifest);
            else
                ProcessTypeBlob(type, manifest);
        }

        private static void ProcessTypeBlob(ushort type, TypeManifest manifest) {
            if (TypeClassifications.KnownPayload.Contains(type)) return;

            manifest.StructuredDataInfo = new Common.StructuredDataInfo();
            var referenceTypes = new HashSet<Common.AssetRepoType>();

            var typeDirectory  = Path.Combine(_outputDirectory, type.ToString("X3"));
            var assetDirectory = Path.Combine(typeDirectory,    "assets");
            IO.CreateDirectorySafe(assetDirectory);

            Parallel.ForEach(DataTool.Program.TrackedFiles[type], x => ProcessAssetBlob(x, assetDirectory));

            manifest.GUIDReferenceTypes = referenceTypes;
        }

        private static void ProcessAssetBlob(ulong guid, string typeDir) {
            using (var s = IO.OpenFile(guid)) {
                if (s == null) return;

                var data = new byte[s.Length];
                s.Read(data, 0, data.Length);


                var record = new AssetRecord { GUID = (teResourceGUID) guid, References = new HashSet<teResourceGUID>() };

                unsafe {
                    fixed (byte* ptr = data) {
                        var i = 0;
                        while (i + 8 <= data.Length) {
                            var sig = *(ulong*) (ptr + i);
                            if (DataTool.Program.ValidKey(sig)) record.References.Add((teResourceGUID) sig);
                            i += 1;
                        }
                    }
                }

                using (Stream outputFile = File.OpenWrite(Path.Combine(typeDir, $"{teResourceGUID.AsString(guid)}.json"))) {
                    outputFile.SetLength(0);
                    var buf = JsonSerializer.PrettyPrintByteArray(JsonSerializer.Serialize(record));
                    outputFile.Write(buf, 0, buf.Length);
                }
            }
        }

        private static void ProcessTypeSTUv2(ushort type, TypeManifest manifest) {
            manifest.StructuredDataInfo = new Common.StructuredDataInfo();
            var referenceTypes = new HashSet<Common.AssetRepoType>();

            var typeDirectory  = Path.Combine(_outputDirectory, type.ToString("X3"));
            var assetDirectory = Path.Combine(typeDirectory,    "assets");
            IO.CreateDirectorySafe(assetDirectory);

            Parallel.ForEach(DataTool.Program.TrackedFiles[type], x => ProcessAssetSTUv2(x, assetDirectory));

            manifest.GUIDReferenceTypes = referenceTypes;
        }

        private static void ProcessAssetSTUv2(ulong guid, string typeDir) {
            var structuredData = STUHelper.OpenSTUSafe(guid);
            if (structuredData == null) return;

            var record = new AssetRecord {
                                             GUID               = (teResourceGUID) guid,
                                             StructuredDataInfo = new Common.StructuredDataInfo(),
                                             References         = new HashSet<teResourceGUID>()
                                         };

            foreach (var instance in structuredData.Instances) {
                if (instance == null) continue;
                STUv2ProcessInstance(record, instance);
            }

            using (Stream outputFile = File.OpenWrite(Path.Combine(typeDir, $"{teResourceGUID.AsString(guid)}.json"))) {
                outputFile.SetLength(0);
                var buf = JsonSerializer.PrettyPrintByteArray(JsonSerializer.Serialize(record));
                outputFile.Write(buf, 0, buf.Length);
            }
        }

        private static void STUv2ProcessInstance(AssetRecord record, STUInstance instance) {
            var fields = GetFields(instance.GetType(), true);

            foreach (var field in fields) {
                var fieldValue = field.GetValue(instance);
                var fieldType  = field.FieldType;
                if (fieldValue == null) continue;

                var fieldAttribute = field.GetCustomAttribute<STUFieldAttribute>();
                if (fieldAttribute != null)
                    if (fieldAttribute.ReaderType == typeof(InlineInstanceFieldReader)) {
                        if (!fieldType.IsArray) {
                            STUv2ProcessInstance(record, (STUInstance) fieldValue);
                        } else {
                            var enumerable = (IEnumerable) fieldValue;
                            foreach (var val in enumerable) STUv2ProcessInstance(record, (STUInstance) val);
                        }

                        return;
                    }

                if (fieldType.IsArray) {
                    var elementType = fieldType.GetElementType();
                    var enumerable  = (IEnumerable) fieldValue;
                    foreach (var val in enumerable) STUv2ProcessField(record, val, elementType);
                } else {
                    STUv2ProcessField(record, fieldValue, fieldType);
                }
            }
        }

        private static void STUv2ProcessField(AssetRecord record, object value, Type fieldType) {
            if (value is teResourceGUID resGuid) {
                record.References.Add(resGuid);
                return;
            }

            if (fieldType.IsGenericType) {
                var genericBase = fieldType.GetGenericTypeDefinition();

                var genericParams = fieldType.GetGenericArguments();
                if (genericBase != typeof(teStructuredDataAssetRef<>)) return;

                var method = typeof(Program).GetMethod(nameof(GetAssetRefGUID));
                if (method == null) return;

                method = method.MakeGenericMethod(genericParams);
                record.References.Add((teResourceGUID) method.Invoke(null, new[] { value }));
                return;
            }

            if (!(value is ulong @ulong)) return;
            if (@ulong == 0) return;
            var type = teResourceGUID.Type(@ulong);
            if (type > 1 && type < 0xFF) record.References.Add((teResourceGUID) @ulong);
        }

        internal static FieldInfo[] GetFields(Type type, bool doParent = false) {
            var parent                                                                                                                         = new FieldInfo[0];
            if (type.BaseType != null && type.BaseType.Namespace != null && !type.BaseType.Namespace.StartsWith("System.") && doParent) parent = GetFields(type.BaseType);
            return parent.Concat(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                         .ToArray();
        }

        public static teResourceGUID GetAssetRefGUID<T>(teStructuredDataAssetRef<T> val) { return val.GUID; }
    }
}
