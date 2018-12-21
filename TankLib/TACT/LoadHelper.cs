using System.IO;
using System.Reflection;
using TACTLib;
using TACTLib.Client;

namespace TankLib.TACT {
    public static class LoadHelper {
        public static void PreLoad() {
            Logger.OnInfo  += (category, message) => Helpers.Logger.Info(category, message);
            Logger.OnDebug += (category, message) => Helpers.Logger.Debug(category, message);
            Logger.OnWarn  += (category, message) => Helpers.Logger.Warn(category, message);
            Logger.OnError += (category, message) => Helpers.Logger.Error(category, message);
        }

        public static void PostLoad(ClientHandler clientHandler) {
            var keyFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly()
                                                        .Location) +
                          @"\ow.keys";
            if (File.Exists(keyFile))
                clientHandler.ConfigHandler.Keyring.LoadSupportFile(keyFile);
        }
    }
}
