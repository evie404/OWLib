using System.IO;
using System.Runtime.InteropServices;

namespace TankLib.Chunks {
    // ReSharper disable once InconsistentNaming
    public class teEffect_CAT : IChunk {
        public Structure Header;
        public string    ID => "ECAT";

        public void Parse(Stream stream) {
            using (var reader = new BinaryReader(stream)) {
                Header = reader.Read<Structure>();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Structure {
            public teResourceGUID Hardpoint;
        }
    }
}
