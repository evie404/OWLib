using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DragonLib.CLI;

namespace TankTonka {
    [Serializable]
    public class TonkaFlags : ICLIFlags {
        [CLIFlag("directory", Positional = 0, Category = "Tonka", Help = "Overwatch Directory", IsRequired = true)]
        public string OverwatchDirectory { get; set; }

        [CLIFlag("language", Category = "Tonka", Help = "Language to load", Aliases = new[] { "L", "lang" }, ValidValues = new[] { "deDE", "enUS", "esES", "esMX", "frFR", "itIT", "jaJP", "koKR", "plPL", "ptBR", "ruRU", "zhCN", "zhTW" })]
        public string Language { get; set; }

        [CLIFlag("speech-language", Category = "Tonka", Help = "Speech Language to load", Aliases = new[] { "T", "speechlang" }, ValidValues = new[] { "deDE", "enUS", "esES", "esMX", "frFR", "itIT", "jaJP", "koKR", "plPL", "ptBR", "ruRU", "zhCN", "zhTW" })]
        public string SpeechLanguage { get; set; }
    }

    // todo: i cba to make this work. maybe in the future or whatever
}