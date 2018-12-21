using System.IO;
using DataTool;
using TankView.ViewModel;
using TACTLib.Container;
using TACTLib.Core.Product.Tank;

namespace TankView.Helper {
    public static class IOHelper {
        public static Stream OpenFile(CKey ckey) { return Program.Client.OpenCKey(ckey); }

        public static Stream OpenFile(EKey ekey) { return Program.Client.OpenEKey(ekey); }

        public static Stream OpenFile(ApplicationPackageManifest.PackageRecord packageRecord) { return Program.TankHandler.OpenFile(packageRecord.GUID); }

        public static Stream OpenFile(GUIDEntry entry) { return entry.GUID != 0 ? Program.TankHandler.OpenFile(entry.GUID) : Program.Client.OpenCKey(entry.ContentKey); }

        public static Stream OpenFile(ulong guid) { return Program.TankHandler.OpenFile(guid); }

        public static bool HasFile(ulong guid) { return Program.TankHandler.Assets.ContainsKey(guid); }
    }
}
