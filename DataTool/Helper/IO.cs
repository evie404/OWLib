using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TankLib;
using TACTLib.Core;
using static DataTool.Program;

namespace DataTool.Helper {
    // ReSharper disable once InconsistentNaming
    public static class IO {
        public static Dictionary<uint, Dictionary<uint, string>> GUIDTable = new Dictionary<uint, Dictionary<uint, string>>();

        public static HashSet<ulong> MissingKeyLog = new HashSet<ulong>();

        public static string GetValidFilename(string filename) {
            if (filename == null) return null;
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = $@"[{invalidChars}]+";

            string[] reservedWords = {
                                         "CON",
                                         "PRN",
                                         "AUX",
                                         "CLOCK$",
                                         "NUL",
                                         "COM0",
                                         "COM1",
                                         "COM2",
                                         "COM3",
                                         "COM4",
                                         "COM5",
                                         "COM6",
                                         "COM7",
                                         "COM8",
                                         "COM9",
                                         "LPT0",
                                         "LPT1",
                                         "LPT2",
                                         "LPT3",
                                         "LPT4",
                                         "LPT5",
                                         "LPT6",
                                         "LPT7",
                                         "LPT8",
                                         "LPT9"
                                     };

            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");

            return reservedWords.Select(reservedWord => $"^{reservedWord}\\.")
                                .Aggregate(sanitisedNamePart, (current, reservedWordPattern) => Regex.Replace(current, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase));
        }

        public static void LoadGUIDTable() {
            if (!File.Exists("GUIDNames.csv")) return;
            var i = 0;
            foreach (var line in File.ReadAllLines("GUIDNames.csv")) {
                if (i == 0) {
                    i++;
                    continue;
                }

                var parts       = line.Split(',');
                var indexString = parts[0];
                var typeString  = parts[1];
                var name        = parts[2];

                var index = uint.Parse(indexString, NumberStyles.HexNumber);
                var type  = uint.Parse(typeString,  NumberStyles.HexNumber);

                if (!GUIDTable.ContainsKey(type)) GUIDTable[type] = new Dictionary<uint, string>();
                GUIDTable[type][index] = name;

                i++;
            }
        }

        public static string GetFileName(ulong guid) { return teResourceGUID.AsString(guid); }

        public static void WriteFile(Stream stream, string filename) {
            if (stream == null) return;
            var path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path) && path != null) Directory.CreateDirectory(path);

            using (Stream file = File.OpenWrite(filename)) {
                file.SetLength(0); // ensure no leftover data
                stream.CopyTo(file);
            }
        }

        public static void WriteFile(ulong guid, string path) {
            if (!TankHandler.Assets.ContainsKey(guid)) return;
            WriteFile(OpenFile(guid), guid, path);
        }

        public static void WriteFile(Stream stream, ulong guid, string path) {
            if (stream == null || guid == 0) return;

            // string filename = GUIDTable.ContainsKey(guid) ? GUIDTable[guid] : GetFileName(guid);
            var filename = GetFileName(guid);

            WriteFile(stream, Path.Combine(path, filename));

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            using (Stream file = File.OpenWrite(Path.Combine(path, filename))) {
                stream.CopyTo(file);
            }
        }

        public static Stream OpenFile(ulong guid) {
            try {
                return TankHandler.OpenFile(guid);
            } catch (Exception e) {
                if (e is BLTEKeyException keyException)
                    if (MissingKeyLog.Add(keyException.MissingKey) && Debugger.IsAttached)
                        TankLib.Helpers.Logger.Warn("BLTE", $"Missing key: {keyException.MissingKey:X16}");
                TankLib.Helpers.Logger.Debug("Core", $"Unable to load file: {guid:X8}");
                return null;
            }
        }

        public static void CreateDirectoryFromFile(string path) {
            var dir = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(dir)) return;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        public static void CreateDirectorySafe(string david) {
            var cylde = Path.GetFullPath(david);
            if (string.IsNullOrWhiteSpace(cylde)) return;
            if (!Directory.Exists(cylde)) Directory.CreateDirectory(cylde);
        }

        public static string GetString(ulong guid) {
            if (guid == 0) return null; // don't even try
            try {
                using (var stream = OpenFile(guid)) {
                    return stream == null ? null : new teString(stream);
                }
            } catch {
                return null;
            }
        }
    }
}
