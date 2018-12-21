using System;
using System.IO;
using System.Text;
using TankLib.Helpers.DataSerializer;
using TankLib.Math;
using TankLib.STU.Types.Enums;

namespace TankLib.Replay {
    public sealed class tePlayerHighlight : ReadableData {
        [Flags]
        public enum HighlightKind : byte {
            Top5Highlight   = 0x0, // Displayed in the "Top 5" section
            PlayOfTheGame   = 0x1, // POTG
            ManualHighlight = 0x4  // if false, game will display "New" label
        }

        [Flags]
        public enum HighlightUIFlags {
            Top5Highlight   = 0x1, // Displayed in the "Top 5" section
            ManualHighlight = 0x2, // Displayed in the "36 manual highlights" section
            Viewed          = 0x4, // if false, game will display "New" label
            HasBeenExported = 0x8  // if true, game will display "Recorded" label
        }

        [Logical.Skip]
        // ReSharper disable once InconsistentNaming
        public static readonly int MAGIC = Util.GetMagicBytesBE('p', 'h', 'l'); // Player HighLight

        public int            BuildVersion;
        public ReplayChecksum Checksum;
        public int            DataLength;

        [Logical.DynamicSizeArrayAttribute(typeof(byte), typeof(FillerStruct))]
        public FillerStruct[] FillerStructs;

        public HighlightUIFlags Flags;

        public byte           FormatVersion;
        public teResourceGUID GameMode;

        [Logical.DynamicSizeArrayAttribute(typeof(int), typeof(HeroData))]
        public HeroData[] Heroes;

        [Logical.DynamicSizeArrayAttribute(typeof(int), typeof(HighlightInfo))]
        public HighlightInfo[] Info;

        public teResourceGUID Map;
        public long           PlayerId;

        [Logical.Skip]
        public MemoryStream Replay;

        public int  ReplayVersion;
        public long Unknown1;
        public long Unknown2;
        public long Unknown3;
        public long Unknown4;
        public uint Unknown5;
        public uint Unknown6;

        public tePlayerHighlight(Stream stream, bool leaveOpen = false) {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen)) {
                if ((reader.ReadInt32() & Util.BYTE_MASK_3) == MAGIC) {
                    stream.Position -= 1;
                    Read(reader);
                    var size = reader.ReadInt32();

                    // todo: data is sometimes wrong. too many "filler structs" read.
                    //int expected = (int)reader.BaseStream.Length - (int)reader.BaseStream.Position;
                    //if (size > reader.BaseStream.Length) {
                    //    
                    //}

                    Replay = new MemoryStream(reader.ReadBytes(size));
                }
            }
        }

        public class FillerStruct : ReadableData {
            public uint Unknown62;
            public uint Unknown63;
            public uint Unknown64;
            public byte Unknown65;
        }

        public class HighlightInfo : ReadableData {
            public teVec3         Direction;
            public uint           Elevation;
            public teResourceGUID Hero;

            public teResourceGUID HighlightIntro;
            public teResourceGUID HighlightType;
            public teResourceGUID ItemSkinKey;
            public teResourceGUID ItemWSkinKey;

            public HighlightKind Kind;

            [Logical.NullPaddedStringAttribute]
            public string PlayerName;

            public teVec3 Position;

            public teResourceGUID TeamA;
            public teResourceGUID TeamB;
            public TeamIndex      TeamIndex;
            public ulong          Timestamp;
            public uint           Unknown1;
            public uint           Unknown2;
            public float          Unknown3;
            public float          Unknown4;

            public byte   UnknownByte;
            public teVec3 Up;

            public teUUID UUID;
        }
    }
}
