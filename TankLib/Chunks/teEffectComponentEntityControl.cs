using System.IO;
using System.Runtime.InteropServices;

namespace TankLib.Chunks {
    public class teEffectComponentEntityControl : IChunk {
        public enum Action : byte { // todo: maybe u16/i16
            Show     = 1,           // Animation is 0
            PlayAnim = 4
        }

        // todo: these mean something
        // controlComponent->m_ejectInitialVelocity != (0xFFFF)
        // controlComponent->m_ejectRotationAxis != (0xFFFF)

        public Structure Header;
        public string    ID => "ECEC";

        public void Parse(Stream stream) {
            using (var reader = new BinaryReader(stream)) {
                Header = reader.Read<Structure>();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct Structure {
            public teResourceGUID Animation;
            public teResourceGUID Identifier;
            public Action         Action;
        }
    }
}
