using System;
using System.Data.HashFunction.CRC;
using System.IO;
using System.Text;

namespace TankLibHelper.Modes {
    public class FindMirrorTypes : IMode {
        private StructuredDataInfo _info;
        public  string             Mode => "findmirrortypes";

        public ModeResult Run(string[] args) {
            if (args.Length < 2) {
                Console.Out.WriteLine("Missing required arg: \"output\"");
                return ModeResult.Fail;
            }

            string dataDirectory;

            if (args.Length >= 2)
                dataDirectory = args[1];
            else
                dataDirectory = StructuredDataInfo.GetDefaultDirectory();

            _info = new StructuredDataInfo(dataDirectory);

            var crc32 = CRCFactory.Instance.Create(CRCConfig.CRC32);

            foreach (var instance in _info.KnownInstances) {
                //if (instance.Value.StartsWith("STUStatescript")) {
                if (instance.Value.StartsWith("M")) continue;
                var mirrorType = instance.Value.Replace("STU", "M");
                if (!mirrorType.StartsWith("M")) continue;
                var hash = BitConverter.ToUInt32(crc32.ComputeHash(Encoding.ASCII.GetBytes(mirrorType.ToLowerInvariant()))
                                                      .Hash,
                                                 0);

                if (_info.Instances.ContainsKey(hash)) Console.Out.WriteLine($"{hash:X8}, {mirrorType}");
                //}
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
