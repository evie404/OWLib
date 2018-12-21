using System.IO;
using System.Text;
using TankLib.Helpers.DataSerializer;

namespace TankLib.Replay {
    public sealed class tePlayerReplay : ReadableData {
        [Logical.Skip]
        // ReSharper disable once InconsistentNaming
        public static readonly int MAGIC = Util.GetMagicBytesBE('p', 'r', 'p'); // Player RePlay

        public uint BuildNumber;

        [Logical.ZstdBuffer(Logical.ZstdBufferSize.StreamEnd)]
        public byte[] DecompressedBuffer;

        public byte           FormatVersion;
        public teResourceGUID GameMode;

        [Logical.Conditional("(helper.BitwiseAnd(Unknown1, 4)) != 0", new[] { "Unknown1" })]
        public tePlayerHighlight.HighlightInfo HighlightInfo;

        [Logical.Conditional("(helper.BitwiseAnd(Unknown1, 4)) != 0", new[] { "Unknown1" })]
        public int HighlightInfoLength;

        public teResourceGUID Map;
        public ReplayChecksum MapChecksum;
        public ReplayParams   Params;
        public int            ParamsBlockLength;
        public byte           Unknown1;
        public uint           Unknown2;
        public uint           Unknown3;

        public tePlayerReplay(Stream stream, bool leaveOpen = false) {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen)) {
                if ((reader.ReadInt32() & Util.BYTE_MASK_3) == MAGIC) {
                    stream.Position -= 1;
                    Read(reader);
                }
            }
        }

        public class ReplayParams : ReadableData {
            public uint  EndFrame;
            public ulong EndMS;
            public ulong ExpectedDurationMS;

            [Logical.DynamicSizeArrayAttribute(typeof(int), typeof(HeroData))]
            public HeroData[] Heroes;

            public uint  StartFrame;
            public ulong StartMS;
        }
    }
}
