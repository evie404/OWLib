using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataTool.Flag;
using TankLib;
using static DataTool.Helper.IO;
using static DataTool.Program;

namespace DataTool.ToolLogic.Extract.Debug {
    [Tool("extract-debug-shaderhashes", Description = "Extract shader hashes (debug)", CustomFlags = typeof(ExtractFlags), IsSensitive = true)]
    public class ExtractDebugShaderHashes : ITool {
        public void Parse(ICLIFlags toolFlags) { GetSoundbanks(toolFlags); }

        public void GetSoundbanks(ICLIFlags toolFlags) {
            const string container = "ShaderHashes";

            string basePath;
            if (toolFlags is ExtractFlags flags)
                basePath = flags.OutputPath;
            else
                throw new Exception("no output path");

            var hashes = new HashSet<uint>();

            foreach (var guid in TrackedFiles[0x86]) {
                var instance = new teShaderInstance(OpenFile(guid));
                //teShaderCode shaderCode = new teShaderCode(OpenFile(instance.Header.ShaderCode));

                //if (shaderCode.Header.ShaderType != Enums.teSHADER_TYPE.PIXEL) continue;
                //if (shaderCode.Header.ShaderType != Enums.teSHADER_TYPE.VERTEX) continue;
                //if (shaderCode.Header.ShaderType != Enums.teSHADER_TYPE.COMPUTE) continue;

                //if (instance.ShaderResources != null) {
                //    foreach (teShaderInstance.ShaderResourceDefinition inputDefinition in instance.ShaderResources) {
                //        hashes.Add(inputDefinition.NameHash);
                //    }
                //}

                if (instance.BufferParts != null)
                    foreach (var bufferParts in instance.BufferParts)
                    foreach (var part in bufferParts)
                        hashes.Add(part.Hash);
            }

            var path = Path.Combine(basePath, container, "hashes.txt");
            CreateDirectoryFromFile(path);
            using (var writer = new StreamWriter(path)) {
                foreach (var hash in hashes.OrderBy(x => x)) writer.WriteLine($"{hash:X8}");
            }
        }
    }
}
