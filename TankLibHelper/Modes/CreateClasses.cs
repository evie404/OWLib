using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TankLibHelper.Modes {
    public class CreateClasses : IMode {
        private StructuredDataInfo _info;
        public  string             Mode => "createclasses";

        public ModeResult Run(string[] args) {
            if (args.Length < 2) {
                Console.Out.WriteLine("Missing required arg: \"output\"");
                return ModeResult.Fail;
            }

            var    outDirectory = args[1];
            string dataDirectory;

            if (args.Length >= 3)
                dataDirectory = args[2];
            else
                dataDirectory = StructuredDataInfo.GetDefaultDirectory();

            var extraData = args.Skip(3)
                                .ToArray();

            _info = new StructuredDataInfo(dataDirectory);
            foreach (var extra in extraData) _info.LoadExtra(extra);

            var instanceBuilderConfig = new BuilderConfig { Namespace = "TankLib.STU.Types" };
            var enumBuilderConfig     = new BuilderConfig { Namespace = "TankLib.STU.Types.Enums" };

            var generatedDirectory      = Path.Combine(outDirectory, "Generated");
            var generatedEnumsDirectory = Path.Combine(outDirectory, "Generated", "Enums");
            Directory.CreateDirectory(generatedDirectory);
            Directory.CreateDirectory(generatedEnumsDirectory);

            var enumFields = new Dictionary<uint, STUFieldJSON>();

            foreach (var instance in _info.Instances) {
                if (_info.BrokenInstances.Contains(instance.Key)) continue;
                //if (instance.Key == 0x440233A5) {  // for generating the mirror types with oldhash
                //    continue;
                //}

                var instanceBuilder = new InstanceBuilder(instanceBuilderConfig, _info, instance.Value);

                BuildAndWriteCSharp(instanceBuilder, generatedDirectory);

                foreach (var field in instance.Value.Fields) {
                    if (field.SerializationType != 8 && field.SerializationType != 9) continue;

                    var enumType = uint.Parse(field.Type, NumberStyles.HexNumber);
                    if (!enumFields.ContainsKey(enumType)) {
                        enumFields[enumType] = field;

                        var enumBuilder = new EnumBuilder(enumBuilderConfig, _info, field);
                        BuildAndWriteCSharp(enumBuilder, generatedEnumsDirectory);
                    }
                }
            }

            foreach (var enumData in _info.Enums) {
                if (enumFields.ContainsKey(enumData.Key)) continue;
                var enumBuilder = new EnumBuilder(enumBuilderConfig, _info, new STUFieldJSON { Type = enumData.Key.ToString("X8"), Size = 4 });
                BuildAndWriteCSharp(enumBuilder, generatedEnumsDirectory);
            }


            return ModeResult.Success;
        }

        public void BuildAndWriteCSharp(ClassBuilder builder, string directory) {
            var instanceCode = builder.BuildCSharp();

            using (var file = new StreamWriter(Path.Combine(directory, builder.GetName() + ".cs"))) {
                file.Write(instanceCode);
            }
        }
    }
}
