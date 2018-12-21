using System.IO;
using System.Runtime.InteropServices;

namespace TankLib.Chunks {
    public class teEffectComponentParticle : IChunk {
        public Structure Header;
        public string    ID => "ECPR";

        public void Parse(Stream stream) {
            using (var reader = new BinaryReader(stream)) {
                Header = reader.Read<Structure>();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Structure {
            public ulong Unknown1;
            public ulong Unknown2;
            public ulong Unknown3;
            public ulong Unknown4;
            public ulong Model;
        }
    }
}
