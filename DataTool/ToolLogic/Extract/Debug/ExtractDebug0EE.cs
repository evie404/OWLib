using System;
using System.IO;
using DataTool.FindLogic;
using DataTool.Flag;
using TankLib;
using TankLib.STU;
using TankLib.STU.Types;
using static DataTool.Program;
using static DataTool.Helper.IO;

namespace DataTool.ToolLogic.Extract.Debug {
    [Tool("extract-debug-0EE", Description = "Extract 0EE (debug)", CustomFlags = typeof(ExtractFlags), IsSensitive = true)]
    public class ExtractDebug0EE : ITool {
        public void Parse(ICLIFlags toolFlags) { Extract0EE(toolFlags); }

        public void Extract0EE(ICLIFlags toolFlags) {
            string basePath;
            if (toolFlags is ExtractFlags flags)
                basePath = flags.OutputPath;
            else
                throw new Exception("no output path");

            const string container = "Debug0EE";
            var          path      = Path.Combine(basePath, container);

            foreach (var key in TrackedFiles[0xEE])
                using (var stream = OpenFile(key)) {
                    var structuredData = new teStructuredData(stream);

                    var inst = structuredData.GetMainInstance<STU_E3594B8E>();

                    if (inst == null) continue;

                    var name        = GetString(inst.m_name);
                    var description = GetString(inst.m_description);

                    var info = new Combo.ComboInfo();
                    Combo.Find(info, (ulong) inst.m_21EB3E73);
                    info.SetTextureName((ulong) inst.m_21EB3E73, name);

                    OpenSTUTest(inst.m_7B7CCF55); // ux1
                    OpenSTUTest(inst.m_E81C5302); // ux2
                    OpenSTUTest(inst.m_FD9B53F4); // ux3
                    //{
                    //    teStructuredData uxScreenData = new teStructuredData();
                    //}

                    SaveLogic.Combo.SaveLooseTextures(flags, path, info);
                }
        }

        public void OpenSTUTest(ulong guid) {
            if (guid == 0) return;
            using (var stream = OpenFile(guid)) {
                using (Stream file = File.OpenWrite(teResourceGUID.AsString(guid))) {
                    stream.CopyTo(file);
                }

                stream.Position = 0;
                var structuredData = new teStructuredData(stream);
            }
        }
    }
}
