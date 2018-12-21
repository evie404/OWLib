using System.Globalization;
using System.IO;
using System.Linq;
using DataTool.Flag;
using DataTool.Helper;
using DataTool.ToolLogic.Extract;
using TankLib;
using TankLib.Chunks;

namespace DataTool.ToolLogic.Dbg {
    [Tool("te-model-chunk-dump", Description = "", IsSensitive = true, CustomFlags = typeof(ExtractFlags))]
    internal class DebugModelDump : ITool {
        public void Parse(ICLIFlags toolFlags) {
            var flags = toolFlags as ExtractFlags;
            var testGuids = flags?.Positionals.Skip(3)
                                 .Select(x => uint.Parse(x, NumberStyles.HexNumber));
            teChunkedData.Manager.ChunkTypes.Clear();
            foreach (var guid in Program.TrackedFiles[0xC]) {
                if (!(testGuids ?? throw new InvalidDataException()).Contains(teResourceGUID.Index(guid))) continue;
                var path = Path.Combine(flags.OutputPath,
                                        "teModelChunk",
                                        teResourceGUID.Index(guid)
                                                      .ToString("X"));
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                using (var file = IO.OpenFile(guid))
                using (var reader = new BinaryReader(file)) {
                    var chunk = new teChunkedData(reader);
                    for (var i = 0; i < chunk.Chunks.Length; ++i) {
                        if (!(chunk.Chunks[i] is teDataChunk_Dummy dummy)) continue;
                        var filename = Path.Combine(path, chunk.ChunkTags[i]);
                        using (Stream target = File.OpenWrite(filename)) {
                            dummy.Data.CopyTo(target);
                        }
                    }
                }
            }
        }
    }
}
