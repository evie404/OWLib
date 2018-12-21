using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DataTool.FindLogic;
using DataTool.Flag;
using TankLib;
using static DataTool.Helper.IO;

namespace DataTool.ToolLogic.Extract.Debug {
    [Tool("extract-debug-movies", Description = "Extract movies (debug)", CustomFlags = typeof(ExtractFlags), IsSensitive = true)]
    public class ExtractDebugMovies : ITool {
        private static string FFMPEG     = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Third Party", "ffmpeg.exe"));
        private static bool   HAS_FFMPEG = File.Exists(FFMPEG);

        public void Parse(ICLIFlags toolFlags) { ExtractMOVI(toolFlags); }

        public void ExtractMOVI(ICLIFlags toolFlags) {
            string basePath;
            var    flags = toolFlags as ExtractFlags;
            basePath = flags?.OutputPath;
            if (string.IsNullOrWhiteSpace(basePath)) throw new Exception("no output path");

            const string container = "DebugMovies";

            foreach (var key in Program.TrackedFiles[0xB6])
                using (var videoStream = OpenFile(key)) {
                    if (videoStream != null)
                        using (var reader = new BinaryReader(videoStream)) {
                            var movi = reader.Read<MOVI>();
                            videoStream.Position = 128; // wrapped in "MOVI" for some reason
                            var videoFile = Path.Combine(basePath,
                                                         container,
                                                         teResourceGUID.LongKey(key)
                                                                       .ToString("X12"),
                                                         $"{teResourceGUID.LongKey(key):X12}.bk2");
                            WriteFile(videoStream, videoFile);
                            var audioInfo = new Combo.ComboInfo { SoundFiles = new Dictionary<ulong, Combo.SoundFileInfo> { { movi.MasterAudio, new Combo.SoundFileInfo(movi.MasterAudio) } } };
                            SaveLogic.Combo.SaveSoundFile(flags,
                                                          Path.Combine(basePath,
                                                                       container,
                                                                       teResourceGUID.LongKey(key)
                                                                                     .ToString("X12")),
                                                          audioInfo,
                                                          movi.MasterAudio,
                                                          false);
                        }
                }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOVI {
            public uint   Magic;
            public uint   Version;
            public ushort Unknown1;
            public ushort Flags;
            public uint   Width;
            public uint   Height;
            public uint   Depth;
            public ulong  MasterAudio;
            public ulong  ExtraAudio;
        }
    }
}
