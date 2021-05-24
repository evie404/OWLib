using System;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace DataTool.ToolLogic.List {
    [Serializable, UsedImplicitly]
    public class ListFlags : ICLIFlags {
        [CLIFlag("json", Category = "List", Help = "Output JSON to stderr")]
        public bool JSON { get; set; }

        [CLIFlag("out", Aliases = new[] {"o"}, Category = "List", Help = "Output JSON file")]
        public string Output { get; set; }

        [CLIFlag("flatten", Category = "List", Help = "Flatten output", Hidden = true)]
        public bool Flatten { get; set; }

        [CLIFlag("simplify", Category = "List", Help = "Reduces the amount of information output by -list commands (doesn't work for JSON out)")]
        public bool Simplify { get; set; }
    }
}
