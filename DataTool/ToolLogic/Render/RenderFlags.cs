using System;
using DragonLib.CLI;
using JetBrains.Annotations;

namespace DataTool.ToolLogic.Render {
    [Serializable, UsedImplicitly]
    public class RenderFlags : ToolFlags {
        [CLIFlag("out-path", Category = "Render", Help = "Output path", Positional = 2, IsRequired = true)]
        public string OutputPath { get; set; }

        [CLIFlag("width", Default = 1920, Aliases = new [] {"w"}, Category = "Render", Help = "Screen Width")]
        public int Width { get; set; }

        [CLIFlag("height", Default = 1080, Aliases = new [] { "H"}, Flag = "height", Category = "Render", Help = "Screen Height")]
        public int Height { get; set; }
    }
}
