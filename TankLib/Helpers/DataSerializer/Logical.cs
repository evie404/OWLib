using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DynamicExpresso;
using ZstdNet;

namespace TankLib.Helpers.DataSerializer {
    public class Logical {
        public enum ZstdBufferSize : uint {
            None      = 0,
            StreamEnd = 1
        }

        public class Skip : ConditionalType {
            public override bool ShouldDo(FieldInfo[] fields, object owner) { return false; }
        }

        public class Default : ReadableType {
            public override object Read(BinaryReader reader, FieldInfo field) { return ReaderHelper.ReadType(field.FieldType, reader); }

            public override long GetSize(FieldInfo field, object obj) { return ReadableData.GetObjectSize(obj.GetType(), obj); }
        }

        public class Conditional : ConditionalType {
            public string   Expression;
            public string[] Variables;

            public Conditional(string expression, string[] variables) {
                Expression = expression;
                Variables  = variables;
            }

            public override bool ShouldDo(FieldInfo[] fields, object owner) {
                var interpreter = new Interpreter().SetVariable("helper", new ExpressionHelper());

                foreach (var variable in Variables) {
                    var variableField = fields.First(x => x.Name == variable);
                    interpreter.SetVariable(variable, variableField.GetValue(owner));
                }

                return interpreter.Eval<bool>(Expression);
            }
        }

        public class FixedSizeArrayAttribute : ReadableType {
            protected uint Count;
            protected Type Type;

            public FixedSizeArrayAttribute(Type type, uint count) {
                Type  = type;
                Count = count;
            }

            public override object Read(BinaryReader reader, FieldInfo field) {
                var array = Array.CreateInstance(Type, Count);
                for (var i = 0; i < Count; i++) array.SetValue(ReaderHelper.ReadType(Type, reader), i);
                return array;
            }

            public override long GetSize(FieldInfo field, object obj) {
                var array = ((IEnumerable) obj).Cast<object>()
                                               .ToArray();
                long size = 0;

                foreach (var o in array) size += ReadableData.GetObjectSize(Type, o);

                return size;
            }
        }

        public class DynamicSizeArrayAttribute : ReadableType {
            public Type CountType;
            public Type Type;

            public DynamicSizeArrayAttribute(Type countType, Type type) {
                CountType = countType;
                Type      = type;
            }

            public override object Read(BinaryReader reader, FieldInfo field) {
                var count = Convert.ToInt64(ReaderHelper.ReadType(CountType, reader));

                var array = Array.CreateInstance(Type, count);
                for (var i = 0; i < count; i++) array.SetValue(ReaderHelper.ReadType(Type, reader), i);
                return array;
            }

            public override long GetSize(FieldInfo field, object obj) {
                var array = ((IEnumerable) obj).Cast<object>()
                                               .ToArray();
                var size = ReadableData.GetObjectSize(CountType, 0);

                foreach (var o in array) size += ReadableData.GetObjectSize(Type, o);

                return size;
            }

            public override long GetNoDataStartSize(FieldInfo field, object obj) { return ReadableData.GetObjectSize(CountType, 0); }
        }

        public class NullPaddedStringAttribute : ReadableType {
            public Encoding EncodingType;
            public char     PadChar;
            public int?     TotalWidth;

            public NullPaddedStringAttribute(int totalWidth, char padChar) {
                EncodingType = Encoding.UTF8;
                TotalWidth   = totalWidth;
                PadChar      = padChar;
            }


            public NullPaddedStringAttribute(object encoding = null) { EncodingType = (Encoding) encoding ?? Encoding.UTF8; }

            public override object Read(BinaryReader reader, FieldInfo field) {
                var  bytes = new List<byte>();
                byte b;
                while ((b = reader.ReadByte()) != 0)
                    bytes.Add(b);
                return EncodingType.GetString(bytes.ToArray());
            }

            public override long GetSize(FieldInfo field, object obj) { return EncodingType.GetByteCount((string) obj) + 1; }
        }

        public class ZstdBuffer : ReadableType {
            public long           CompressedSize;
            public ZstdBufferSize Size;

            public ZstdBuffer(ZstdBufferSize size) { Size = size; }

            public override object Read(BinaryReader reader, FieldInfo field) {
                if (Size == ZstdBufferSize.StreamEnd) {
                    var compressedBuffer = reader.ReadBytes((int) (reader.BaseStream.Length - reader.BaseStream.Position));
                    CompressedSize = compressedBuffer.Length;
                    // return Decompress(compressedBuffer);
                    return compressedBuffer;
                }

                return null;
            }

            public static byte[] Decompress(byte[] compressedBuffer) {
                var compressedMagic = BitConverter.ToUInt32(compressedBuffer, 0);
                Debug.Assert(compressedMagic == 0xFD2FB528);

                var decompressedBuffer = new byte[1024 * 1024]; // 1MB should be enough for anyone!
                int length;
                using (var dec = new Decompressor()) {
                    length = dec.Unwrap(compressedBuffer, decompressedBuffer, 0);
                }

                var shrunkBuffer = new byte[length];
                Array.Copy(decompressedBuffer, 0, shrunkBuffer, 0, length);
                return shrunkBuffer;
            }

            public static byte[] Compress(byte[] decompressedBuffer) {
                byte[] compressedBuffer;
                using (var options = new CompressionOptions(null, 4))
                using (var compressor = new Compressor(options)) {
                    compressedBuffer = compressor.Wrap(decompressedBuffer);
                }

                var compressedMagic = BitConverter.ToUInt32(compressedBuffer, 0);
                Debug.Assert(compressedMagic == 0xFD2FB528);
                return compressedBuffer;
            }

            public override void Write(BinaryWriter writer, FieldInfo field, object obj) {
                if (Size == ZstdBufferSize.StreamEnd) writer.Write((byte[]) obj);
            }

            public override long GetSize(FieldInfo field, object obj) { return CompressedSize; }
        }
    }
}
