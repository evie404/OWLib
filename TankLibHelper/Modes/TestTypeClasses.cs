using System.Globalization;
using TankLib;
using TankLib.STU;
using TankLib.TACT;
using TACTLib.Client;
using TACTLib.Core.Product.Tank;

namespace TankLibHelper.Modes {
    public class TestTypeClasses : IMode {
        private ProductHandler_Tank _tankHandler;
        public  string              Mode => "testtypeclasses";

        public ModeResult Run(string[] args) {
            var gameDir = args[1];
            var type    = ushort.Parse(args[2], NumberStyles.HexNumber);

            var createArgs = new ClientCreateArgs { SpeechLanguage = "enUS", TextLanguage = "enUS" };

            LoadHelper.PreLoad();
            var client = new ClientHandler(gameDir, createArgs);
            _tankHandler = (ProductHandler_Tank) client.ProductHandler;
            LoadHelper.PostLoad(client);

            foreach (var asset in _tankHandler.Assets) {
                if (teResourceGUID.Type(asset.Key) != type) continue;
                var filename = teResourceGUID.AsString(asset.Key);
                using (var stream = _tankHandler.OpenFile(asset.Key)) {
                    if (stream == null) continue;
                    var structuredData = new teStructuredData(stream);
                }
            }

            return ModeResult.Success;
        }

        public teStructuredData GetStructuredData(ulong guid) {
            using (var stream = _tankHandler.OpenFile(guid)) {
                if (stream == null) return null;
                return new teStructuredData(stream);
            }
        }

        public string GetString(ulong guid) {
            if (guid == 0) return null;
            using (var stream = _tankHandler.OpenFile(guid)) {
                if (stream == null) return null;
                return new teString(stream);
            }
        }

        public T GetInst<T>(ulong guid) where T : STUInstance {
            return GetStructuredData(guid)
                ?.GetMainInstance<T>();
        }
    }
}
