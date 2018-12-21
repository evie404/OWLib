using System.IO;
using System.Runtime.InteropServices;
using TankLib.STU;
using TankLib.STU.Types;
using TACTLib.Helpers;

namespace TankLib.Chunks {
    /// <inheritdoc />
    /// <summary>MSTU: StructuredData for model definitions</summary>
    public class teModelChunk_STU : IChunk {
        /// <summary>Header data</summary>
        public ModelSTUHeader Header;

        public STUModel StructuredData;
        public string   ID => "MSTU";

        public void Parse(Stream input) {
            using (var reader = new BinaryReader(input)) {
                Header = reader.Read<ModelSTUHeader>();

                reader.BaseStream.Position = Header.Offset;

                var stream = new MemoryStream();
                input.CopyBytes(stream, (int) Header.Size);
                stream.Position = 0;

                StructuredData = new teStructuredData(stream).GetMainInstance<STUModel>();
            }
        }

        /// <summary>MSTU header</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct ModelSTUHeader {
            public long Offset;
            public long Size;
        }
    }
}
