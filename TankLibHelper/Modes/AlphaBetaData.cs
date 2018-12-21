using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TankLibHelper.Modes {
    public class AlphaBetaData : IMode { // wow nice name
        public string Mode => "alphabetadata";

        public ModeResult Run(string[] args) {
            var output = args[1];

            Directory.CreateDirectory(output);

            var dataPath                   = StructuredDataInfo.GetDefaultDirectory();
            if (args.Length >= 3) dataPath = args[2];

            var info = new StructuredDataInfo(dataPath);

            WriteFile(info.KnownEnums,     Path.Combine(output, "KnownEnums.csv"));
            WriteFile(info.KnownInstances, Path.Combine(output, "KnownTypes.csv"));
            WriteFile(info.KnownFields,    Path.Combine(output, "KnownFields.csv"));

            return ModeResult.Success;
        }

        public static void WriteFile(Dictionary<uint, string> source, string output) {
            using (var writer = new StreamWriter(output)) {
                foreach (var hashPair in source.OrderBy(x => x.Value)) writer.WriteLine($"{hashPair.Key:X8}, {hashPair.Value}");
            }
        }
    }
}
