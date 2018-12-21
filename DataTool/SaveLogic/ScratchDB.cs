using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TankLib.Helpers;

namespace DataTool.SaveLogic {
    public class ScratchDB : IEnumerable<KeyValuePair<ulong, ScratchDB.ScratchPath>> {
        private Dictionary<ulong, ScratchPath> Records = new Dictionary<ulong, ScratchPath>();

        private List<ScratchDBLogicMethod> ScratchDBLogic = new List<ScratchDBLogicMethod> {
                                                                                               null,
                                                                                               (reader, dbPath, cb) => {
                                                                                                   var amount = reader.ReadUInt64();
                                                                                                   for (ulong i = 0; i < amount; ++i) {
                                                                                                       var guid = reader.ReadUInt64();
                                                                                                       var path = reader.ReadString();
                                                                                                       cb(guid, new ScratchPath(path));
                                                                                                   }
                                                                                               },
                                                                                               (reader, dbPath, cb) => {
                                                                                                   if (reader.ReadString() != dbPath) return;
                                                                                                   var amount = reader.ReadUInt64();
                                                                                                   for (ulong i = 0; i < amount; ++i) {
                                                                                                       var guid = reader.ReadUInt64();
                                                                                                       var path = reader.ReadString();
                                                                                                       cb(guid, new ScratchPath(path));
                                                                                                   }
                                                                                               }
                                                                                           };

        public ScratchPath this[ulong guid] {
            get => GetRecord(guid);
            set => SetRecord(guid, value);
        }

        public int  Count     => Records.Count;
        public long LongCount => Records.LongCount();

        public IEnumerator<KeyValuePair<ulong, ScratchPath>> GetEnumerator() { return Records.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return Records.GetEnumerator(); }

        public bool HasRecord(ulong guid) {
            if (Records.ContainsKey(guid)) {
                if (!Records[guid]
                        .CheckedExistence) {
                    if (!File.Exists(Records[guid]
                                         .AbsolutePath)) {
                        RemoveRecord(guid);
                        return false;
                    }

                    Records[guid]
                        .CheckedExistence = true;
                }

                return true;
            }

            return false;
        }

        public void SetRecord(ulong guid, ScratchPath path) { Records[guid] = path; }

        public ScratchPath GetRecord(ulong guid) {
            if (HasRecord(guid)) return Records[guid];
            return null;
        }

        public bool RemoveRecord(ulong guid) { return Records.Remove(guid); }

        public void Save(string dbPath) {
            if (Count == 0) return;
            if (File.Exists(dbPath)) File.Delete(dbPath);

            var dir = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (Stream file = File.OpenWrite(dbPath))
            using (var writer = new BinaryWriter(file, Encoding.Unicode)) {
                writer.Write((short) 2);
                writer.Write(dbPath);
                writer.Write(LongCount);
                foreach (var pair in this) {
                    writer.Write(pair.Key);
                    writer.Write(pair.Value.AbsolutePath);
                }
            }
        }

        public void Load(string dbPath) {
            if (!File.Exists(dbPath)) {
                Logger.Error("ScratchDB", $"Database {dbPath} does not exist");
                return;
            }

            using (Stream file = File.OpenRead(dbPath))
            using (var reader = new BinaryReader(file, Encoding.Unicode)) {
                if (file.Length - file.Position < 4) Logger.Error("ScratchDB", "File is not long enough");
                var version = reader.ReadInt16();
                var method  = ScratchDBLogic.ElementAtOrDefault(version);
                if (method == null) {
                    Logger.Error("ScratchDB", $"Database is version {version} which is not supported");
                    return;
                }

                try {
                    method(reader, dbPath, SetRecord);
                } catch (Exception e) {
                    Logger.Error("ScratchDB", e.ToString());
                }
            }
        }

        public class ScratchPath {
            public ScratchPath(string path) {
                AbsolutePath = Path.GetFullPath(path);
                AbsoluteUri  = new Uri(AbsolutePath);
            }

            public  string AbsolutePath     { get; }
            private Uri    AbsoluteUri      { get; }
            public  bool   CheckedExistence { get; set; }

            public string MakeRelative(string cwd) {
                var folder = new Uri(Path.GetFullPath(cwd) + Path.DirectorySeparatorChar);
                return Uri.UnescapeDataString(folder.MakeRelativeUri(AbsoluteUri)
                                                    .ToString()
                                                    .Replace('/', Path.DirectorySeparatorChar));
            }
        }

        private delegate void ScratchDBLogicCallback(ulong guid, ScratchPath scratchPath);

        private delegate void ScratchDBLogicMethod(BinaryReader reader, string dbPath, ScratchDBLogicCallback cb);
    }
}
