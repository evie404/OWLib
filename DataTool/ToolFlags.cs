using System;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace DataTool {
    [Serializable, UsedImplicitly]
    public class ToolFlags : ICLIFlags {
        [CLIFlag("directory", Positional = 0, IsRequired = true, Category = "General", Help = "Overwatch Directory")]
        public string OverwatchDirectory { get; set; }

        [CLIFlag("mode", Positional = 1, IsRequired = true, Category = "General", Help = "Extraction Mode")]
        public string Mode { get; set; }

        [CLIFlag("online", Default = false, Category = "General", Help = "Allow downloading of corrupted files")]
        public bool Online { get; set; }

        [CLIFlag("language", Aliases = new[] { "L" }, Category = "General", Help = "Language to load", ValidValues = new[] {"deDE", "enUS", "esES", "esMX", "frFR", "itIT", "jaJP", "koKR", "plPL", "ptBR", "ruRU", "zhCN", "zhTW"})]
        public string Language { get; set; }

        [CLIFlag("speech-language", Aliases = new[] { "T" }, Category = "General", Help = "Speech Language to load", ValidValues = new[] {"deDE", "enUS", "esES", "esMX", "frFR", "itIT", "jaJP", "koKR", "plPL", "ptBR", "ruRU", "zhCN", "zhTW"})]
        public string SpeechLanguage { get; set; }

        [CLIFlag("graceful-exit", Category = "General", Help = "When enabled don't crash on invalid CMF Encryption")]
        public bool GracefulExit { get; set; }

        [CLIFlag("cache", Default = true, Category = "General", Help = "Cache Index files from CDN")]
        public bool UseCache { get; set; }

        [CLIFlag("cache-data", Default = true, Category = "General", Help = "Cache Data files from CDN")]
        public bool CacheCDNData { get; set; }

        [CLIFlag("validate-cache", Category = "General", Help = "Validate files from CDN")]
        public bool ValidateCache { get; set; }

        [CLIFlag("quiet", Aliases = new[] { "q", "silent" }, Category = "General", Help = "Suppress majority of output messages")]
        public bool Quiet { get; set; }

        [CLIFlag("string-guid", Category = "General", Help = "Returns all strings as their GUID instead of their value", Hidden = true)]
        public bool StringsAsGuids { get; set; }

        [CLIFlag("skip-keys", Category = "General", Help = "Skip key detection", Hidden = true)]
        public bool SkipKeys { get; set; }

        [CLIFlag("rcn", Category = "General", Help = "use (R)CN? CMF", Hidden = true)]
        public bool RCN { get; set; }

        [CLIFlag("deduplicate-textures", Aliases = new [] { "0" }, Category = "General", Help = "Re-use textures from other models")]
        public bool Deduplicate { get; set; }

        [CLIFlag("scratchdb", Category = "General", Help = "Directory for persistent database storage for deduplication info")]
        public string ScratchDBPath { get; set; }

        [CLIFlag("no-names", Category = "General", Help = "Don't use names for textures")]
        public bool NoNames { get; set; }

        [CLIFlag("canonical-names", Category = "General", Help = "Only use canonical names", Hidden = true)]
        public bool OnlyCanonical { get; set; }

        [CLIFlag("no-guid-names", Category = "General", Help = "Completely disables using GUIDNames", Hidden = true)]
        public bool NoGuidNames { get; set; }

        [CLIFlag("extract-shaders", Category = "General", Help = "Extract shader files", Hidden = true)]
        public bool ExtractShaders { get; set; }

        [CLIFlag("enable-async-save", Category = "General", Help = "Enable asynchronous saving", Hidden = true)]
        public bool EnableAsyncSave { get; set; }

        [CLIFlag("disable-language-registry", Category = "General", Help = "Disable fetching language from registry", Hidden = true)]
        public bool NoLanguageRegistry { get; set; }

        [CLIFlag("allow-manifest-fallback", Category = "General", Help = "Allows falling back to older versions if manfiest doesn't exist", Hidden = true)]
        public bool TryManifestFallback { get; set; }
    }
}
