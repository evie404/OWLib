using System;
using System.IO;

namespace TankLib.STU.Primitives {
    /// <inheritdoc />
    /// <summary>teString STU primitive</summary>
    public class STUteStringPrimitive : IStructuredDataPrimitiveFactory {
        public object Deserialize(teStructuredData data, STUField_Info field) {
            if (data.Format == teStructuredDataFormat.V2) {
                var offset = data.Data.ReadInt32();
                if (offset == -1) return null;
                data.DynData.BaseStream.Position = offset;
                Deserialize(data, data.DynData, out var value);

                return new teString(value);
            }

            if (data.Format == teStructuredDataFormat.V1) {
                var infoOffset = data.Data.ReadInt64();
                data.Data.ReadInt64(); // haHAAA 64-bit
                if (infoOffset == -1 || infoOffset == 0) return null;

                var posAfter = data.Data.Position();
                data.Data.BaseStream.Position = infoOffset + data.StartPos;

                Deserialize(data, data.Data, out var value);
                data.Data.BaseStream.Position = posAfter;

                return new teString(value);
            }

            throw new NotImplementedException();
        }

        public object DeserializeArray(teStructuredData data, STUField_Info field) {
            var dynData = data.DynData;
            var offset  = dynData.ReadInt64();

            var mutability = (Enums.SDAM) dynData.ReadInt64(); // SDAM_NONE = 0, SDAM_MUTABLE = 1, SDAM_IMMUTABLE = 2
            // Debug.Assert(Mutability == teEnums.SDAM.IMMUTABLE, "teString.unk != 2 (not immutable)");

            var pos = dynData.BaseStream.Position;
            dynData.Seek(offset);

            Deserialize(data, dynData, out var value);
            dynData.Seek(pos);

            return new teString(value, mutability);
        }

        public Type GetValueType() { return typeof(teString); }

        private void Deserialize(teStructuredData data, BinaryReader reader, out string value) {
            var size = reader.ReadInt32();
            if (size != 0) {
                var checksum = reader.ReadUInt32();
                var offset   = reader.ReadInt64();
                reader.BaseStream.Position = offset + data.StartPos;
                value                      = reader.ReadString(size);
            } else {
                value = string.Empty;
            }
        }
    }
}
