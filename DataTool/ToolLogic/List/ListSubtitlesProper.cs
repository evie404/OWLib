using System;
using System.Collections.Generic;
using DataTool.FindLogic;
using DataTool.Flag;
using static DataTool.Program;
using static DataTool.Helper.STUHelper;
using static DataTool.Helper.IO;

namespace DataTool.ToolLogic.List {
    [Tool("list-subtitles-real", Description = "List subtitles (from audio data)", CustomFlags = typeof(ListFlags))]
    public class ListSubtitlesProper : ITool {
        public void Parse(ICLIFlags toolFlags) {
            GetSubtitles();

            // todo: json

            // if (toolFlags is ListFlags flags)
            //     if (flags.JSON) {
            //         ParseJSON(subtitles, flags);
            //         return;
            //     }
        }

        public void GetSubtitles() {
            var comboInfo = new Combo.ComboInfo();

            var done = new HashSet<KeyValuePair<ulong, ulong>>();

            foreach (var key in TrackedFiles[0x5F]) {
                Combo.Find(comboInfo, key);
                if (!comboInfo.VoiceSets.ContainsKey(key)) continue;

                var voiceSetInfo = comboInfo.VoiceSets[key];
                if (voiceSetInfo.VoiceLineInstances == null) continue;
                foreach (var lineInstance in voiceSetInfo.VoiceLineInstances)
                foreach (var lineInstanceInfo in lineInstance.Value)
                    if (lineInstanceInfo.Subtitle != 0)
                        foreach (var soundInfoSound in lineInstanceInfo.SoundFiles)
                            PrintSubtitle(done, key, soundInfoSound, lineInstanceInfo.Subtitle);
            }
        }

        public void PrintSubtitle(HashSet<KeyValuePair<ulong, ulong>> done,
                                  ulong                               voiceSet,
                                  ulong                               guid,
                                  ulong                               subtitleGUID) {
            var pair = new KeyValuePair<ulong, ulong>(guid, subtitleGUID);
            if (done.Contains(pair)) return;
            done.Add(pair);
            Console.Out.WriteLine($"{GetFileName(voiceSet)}: {GetFileName(guid)} - {GetSubtitleString(subtitleGUID)}");
        }
    }
}
