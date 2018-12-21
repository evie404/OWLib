using System.IO;
using System.Linq;
using DataTool.SaveLogic;
using DataTool.WPF.IO;
using DirectXTexNet;
using TankLib;
using TankLib.STU;
using TankLib.STU.Types;
using TankView.ViewModel;

namespace TankView.Helper {
    public static class DataHelper {
        public enum DataType {
            Unknown,
            Image,
            Sound,
            Model,
            String
        }

        public static DataType GetDataType(GUIDEntry value) {
            if (value == null || value.GUID == 0) return DataType.Unknown;

            var type = teResourceGUID.Type(value.GUID);
            return GetDataType(type);
        }

        public static DataType GetDataType(ushort type) {
            if (type == 0x004 || type == 0x0F1) return DataType.Image;

            if (type == 0x03F || type == 0x0B2 || type == 0x0BB) return DataType.Sound;

            if (type == 0x00C) return DataType.Model;

            if (type == 0x07C || type == 0x0A9 || type == 0x071) return DataType.String;

            return DataType.Unknown;
        }

        internal static object ConvertSound(GUIDEntry value) {
            var ms = new MemoryStream();
            Combo.ConvertSoundFile(IOHelper.OpenFile(value), ms);
            ms.Position = 0;
            return ms;
        }

        public static byte[] ConvertDDS(GUIDEntry                value,
                                        DXGI_FORMAT              targetFormat,
                                        DDSConverter.ImageFormat imageFormat,
                                        int                      frame) {
            try {
                if (GetDataType(value) != DataType.Image) return null;

                var texture = new teTexture(IOHelper.OpenFile(value));
                if (texture.PayloadRequired) {
                    var payload = texture.GetPayloadGUID(value.GUID);
                    if (IOHelper.HasFile(payload))
                        texture.LoadPayload(IOHelper.OpenFile(payload));
                    else
                        return null;
                }

                var ms = texture.SaveToDDS();

                return DDSConverter.ConvertDDS(ms, targetFormat, imageFormat, frame);
            } catch {
                // ignored
            }

            return null;
        }

        internal static object GetString(GUIDEntry value) {
            if (teResourceGUID.Type(value.GUID) == 0x071) return GetSubtitle(value);

            try {
                var str = new teString(IOHelper.OpenFile(value));
                return str.Value;
            } catch {
                return string.Empty;
            }
        }

        private static object GetSubtitle(GUIDEntry value) {
            var stu       = new teStructuredData(IOHelper.OpenFile(value));
            var container = stu.GetInstance<STU_7A68A730>();
            var strings   = new[] { container.m_798027DE?.m_text?.Value, container.m_A84AA2B5?.m_text?.Value, container.m_D872E45C?.m_text?.Value, container.m_1485B834?.m_text?.Value }.Where(x => !string.IsNullOrEmpty(x));
            return string.Join("\n", strings);
        }
    }
}
