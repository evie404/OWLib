using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TankLibHelper.Modes {
    public class DumpHashes : IMode {
        public string Mode => "dumphashes";

        public ModeResult Run(string[] args) {
            var output = args[1];

            Directory.CreateDirectory(output);

            var dataPath                   = StructuredDataInfo.GetDefaultDirectory();
            if (args.Length >= 3) dataPath = args[2];

            var info = new StructuredDataInfo(dataPath);

            WriteInstancesFile(info, Path.Combine(output,        "instances.txt"), null);
            WriteFieldsFile(info.Instances, Path.Combine(output, "fields.txt"));

            WriteInstancesFile(info, Path.Combine(output, "stustatescriptstate_hashes.txt"), new[] { 0xA7213568u });
            WriteInstancesFile(info, Path.Combine(output, "stuentitycomponent_hashes.txt"),  new[] { 0xE46F5A44u });
            WriteInstancesFile(info, Path.Combine(output, "stuunlock_hashes.txt"),           new[] { 0x624B9A14u });

            return ModeResult.Success;
        }

        public static void WriteInstancesFile(StructuredDataInfo info, string output, uint[] allowedBases) {
            using (var writer = new StreamWriter(output)) {
                foreach (var hashPair in info.Instances) {
                    if (allowedBases != null) {
                        var parents = GetParentTree(info, hashPair.Value);

                        var any = parents.Any(allowedBases.Contains);
                        if (!any) continue;
                    }

                    writer.WriteLine($"{hashPair.Key:X8}");
                }
            }
        }

        public static uint[] GetParentTree(StructuredDataInfo info, STUInstanceJSON instanceJSON) {
            if (info.BrokenInstances.Contains(instanceJSON.Hash)) return new uint[0];
            if (instanceJSON.Parent == 0) return new uint[0];

            var parents = new[] { instanceJSON.Parent }.Concat(GetParentTree(info, info.Instances[instanceJSON.Parent]))
                                                       .ToArray();
            return parents;
        }

        public static void WriteFieldsFile(Dictionary<uint, STUInstanceJSON> source, string output) {
            var fields = new HashSet<uint>();

            foreach (var instanceJSON in source)
            foreach (var field in instanceJSON.Value.Fields)
                fields.Add(field.Hash);
            using (var writer = new StreamWriter(output)) {
                foreach (var hashPair in fields) writer.WriteLine($"{hashPair:X8}");
            }
        }
    }
}
