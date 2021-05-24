using DragonLib.CLI;

namespace TankView {
    public class ToolFlags : ICLIFlags {
        [CLIFlag("language", Category = "Tank", Help = "Language to load", Aliases = new[] { "L", "lang" }, ValidValues = new[] { "deDE", "enUS", "esES", "esMX", "frFR", "itIT", "jaJP", "koKR", "plPL", "ptBR", "ruRU", "zhCN", "zhTW" })]
        public string Language { get; set; }

        [CLIFlag("speech-language", Category = "Tank", Help = "Speech Language to load", Aliases = new[] { "T", "speechlang" }, ValidValues = new[] { "deDE", "enUS", "esES", "esMX", "frFR", "itIT", "jaJP", "koKR", "plPL", "ptBR", "ruRU", "zhCN", "zhTW" })]
        public string SpeechLanguage { get; set; }

        [CLIFlag("online", Category = "Tank", Help = "Allow downloading of corrupted files")]
        public bool Online { get; set; }
    }
}