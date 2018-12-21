using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TankLib {
    /// <summary>Tank string</summary>
    public class teString {
        public Enums.SDAM Mutability;

        /// <summary>Value of the string</summary>
        public string Value;

        public teString() { }

        public teString(string value) {
            Value      = value;
            Mutability = Enums.SDAM.NONE;
        }

        public teString(string value, Enums.SDAM mutability) {
            Value      = value;
            Mutability = mutability;
        }

        /// <summary>
        ///     Load "ArchiveString" from a stream
        /// </summary>
        /// <param name="stream">The stream to load from</param>
        public teString(Stream stream) {
            if (stream == null) return;
            using (var reader = new BinaryReader(stream, Encoding.UTF8)) {
                var header = reader.Read<DisplayTextHeader>();
                var bytes  = reader.ReadChars((int) (stream.Length - stream.Position));

                Value = new string(bytes).TrimEnd('\0');
            }
        }

        public static implicit operator string(teString @string) { return @string?.Value; }

        public override string ToString() { return Value; }

        /// <summary>Header for 07C and 0A9 strings</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct DisplayTextHeader {
            public short Unknown;
        }
    }
}
