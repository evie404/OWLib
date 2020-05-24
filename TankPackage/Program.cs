using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DataTool;
using DataTool.Flag;
using DataTool.ToolLogic.Extract;
using static DataTool.Program;
using static DataTool.Helper.IO;
using static DataTool.Helper.Logger;
using System.Threading.Tasks;
using TankLib;
using TACTLib.Core.Product.Tank;
using static TACTLib.Core.Product.Tank.ApplicationPackageManifest;

namespace TankPackage
{
    internal static class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Flags = FlagParser.Parse<ToolFlags>();
            if (Flags == null)
            {
                return;
            }
            var f = Flags;
            
            InitStorage();
            InitMisc();
            InitKeys();
            
            string[] modeArgs = Flags.Positionals.Skip(2).ToArray();

            switch (Flags.Mode.ToLower())
            {
                case "extract":
                    Extract(modeArgs);
                    break;
                case "extract-type":
                    ExtractType(modeArgs);
                    break;
                case "search":
                    Search(modeArgs);
                    break;
                case "search-type":
                    SearchType(modeArgs);
                    break;
                case "info":
                    Info(modeArgs);
                    break;
                case "convert":
                    Convert(modeArgs);
                    break;
                case "types":
                    Types(modeArgs);
                    break;
                default:
                    Console.Out.WriteLine("Available modes: extract, search, search-type, info");
                    break;
            }
        }

        private static void Types(string[] args)
        {
            IOrderedEnumerable<teResourceGUID> unique = new HashSet<teResourceGUID>(TankHandler.m_assets.Keys.Select(x => new teResourceGUID(x))).OrderBy(x => x.GetAttribute(teResourceGUID.teResourceGUIDAttribute.Type) >> 48);

            foreach (teResourceGUID key in unique)
            {
                ushort sh = (ushort)(key >> 48);
                ushort shBE = (ushort)(((sh & 0xFF) << 8) | sh >> 8);
                Console.Out.WriteLine($"{shBE:X4} : {sh:X4} : {key.Type:X3}");
            }
        }

        private static void ExtractType(string[] args)
        {
            string output = args.FirstOrDefault();
            ulong[] guids = args.Skip(1).Select(x => ulong.Parse(x, NumberStyles.HexNumber)).ToArray();
            if (string.IsNullOrWhiteSpace(output))
            {
                return;
            }

            ApplicationPackageManifest apm = TankHandler.m_packageManifest;
            foreach (ContentManifestFile contentManifest in new [] {TankHandler.m_textContentManifest, TankHandler.m_speechContentManifest}) {
                var ids = contentManifest.m_indexMap.Where(x => guids.Length == 0 || guids.Contains(new teResourceGUID(x.Key).Type)).Select(x => x.Key);
                Save(output, apm.m_header.m_checksum, ids, contentManifest);
            }
        }

        private static void Extract(string[] args)
        {
            string output = args.FirstOrDefault();
            ulong[] guids = args.Skip(1).Select(x => ulong.Parse(x, NumberStyles.HexNumber)).ToArray();
            if (string.IsNullOrWhiteSpace(output))
            {
                return;
            }

            Dictionary<ulong, PackageRecord[]> records = new Dictionary<ulong, PackageRecord[]>();
            Dictionary<ulong, PackageRecord[]> totalRecords = new Dictionary<ulong, PackageRecord[]>();
            Dictionary<ulong, PackageHeader> packages = new Dictionary<ulong, PackageHeader>();

            ApplicationPackageManifest apm = TankHandler.m_packageManifest;
            for (int i = 0; i < apm.m_packageEntries.Length; ++i)
            {
                PackageEntry entry = apm.m_packageEntries[i];
                if (guids.Contains(new teResourceGUID(entry.m_packageGUID).Key) || guids.Contains(new teResourceGUID(entry.m_packageGUID).Index))
                {
                    packages[entry.m_packageGUID] = apm.m_packages[i];
                    records[entry.m_packageGUID] = apm.m_packageRecords[i];
                }
                totalRecords[entry.m_packageGUID] = apm.m_packageRecords[i];
            }

            foreach (ulong key in records.Keys)
            {
                Save(output, key, records[key]);
            }
        }

        private static void Convert(string[] args)
        {
            string output = args.FirstOrDefault();
            ulong[] guids = args.Skip(1).Select(x => ulong.Parse(x, NumberStyles.HexNumber)).ToArray();
            if (string.IsNullOrWhiteSpace(output))
            {
                return;
            }

            Dictionary<ulong, PackageRecord[]> records = new Dictionary<ulong, PackageRecord[]>();
            Dictionary<ulong, PackageHeader> packages = new Dictionary<ulong, PackageHeader>();

            var apm = TankHandler.m_packageManifest;
            for (int i = 0; i < apm.m_packageEntries.Length; ++i)
            {
                PackageEntry entry = apm.m_packageEntries[i];
                if (!guids.Contains(new teResourceGUID(entry.m_packageGUID).Key) && !guids.Contains(new teResourceGUID(entry.m_packageGUID).Index) && !guids.Contains(entry.m_packageGUID)) continue;
                packages[entry.m_packageGUID] = apm.m_packages[i];
                records[entry.m_packageGUID] = apm.m_packageRecords[i];
            }

            ICLIFlags flags = FlagParser.Parse<ExtractFlags>();

            void Body(ulong key) {
                DataTool.FindLogic.Combo.ComboInfo info = new DataTool.FindLogic.Combo.ComboInfo();
                string dest = Path.Combine(output, teResourceGUID.AsString(key));
                foreach (PackageRecord record in records[key]) {
                    DataTool.FindLogic.Combo.Find(info, record.m_GUID);
                }

                var context = new DataTool.SaveLogic.Combo.SaveContext(info);
                DataTool.SaveLogic.Combo.Save(flags, dest, context);
                context.Wait();
            }

            Parallel.ForEach(records.Keys, Body);
        }

        private static void Save(string output, ulong key, IEnumerable<PackageRecord> value) => Save(output, key, key, value);

        private static void Save(string output, ulong parentKey, ulong myKey, IEnumerable<PackageRecord> records)
        {
            string dest = Path.Combine(output, teResourceGUID.AsString(parentKey));
            if (myKey != parentKey)
            {
                dest = Path.Combine(dest, "sib", teResourceGUID.AsString(myKey));
            }

            void Body(PackageRecord record) {
                using (Stream file = OpenFile(record.m_GUID)) {
                    string tmp = Path.Combine(dest, $"{new teResourceGUID(record.m_GUID).Type:X3}");
                    if (!Directory.Exists(tmp)) {
                        Directory.CreateDirectory(tmp);
                    }

                    tmp = Path.Combine(tmp, teResourceGUID.AsString(record.m_GUID));
                    InfoLog("Saved {0}", tmp);
                    WriteFile(file, tmp);
                }
            }

            Parallel.ForEach(records, Body);
        }
        private static void Save(string output, ulong myKey, IEnumerable<ulong> records, ContentManifestFile cmf)
        {
            string dest = Path.Combine(output, teResourceGUID.AsString(myKey));

            void Body(ulong guid) {
                using (Stream file = cmf.OpenFile(Client, guid)) {
                    string tmp = Path.Combine(dest, $"{new teResourceGUID(guid).Type:X3}");
                    if (!Directory.Exists(tmp)) {
                        Directory.CreateDirectory(tmp);
                    }

                    tmp = Path.Combine(tmp, teResourceGUID.AsString(guid));
                    InfoLog("Saved {0}", tmp);
                    WriteFile(file, tmp);
                }
            }

            Parallel.ForEach(records, Body);
        }

        private static void Search(IEnumerable<string> args)
        {
            ulong[] guids = args.Select(x => ulong.Parse(x, NumberStyles.HexNumber)).ToArray();

            ApplicationPackageManifest apm = TankHandler.m_packageManifest;
            for (int i = 0; i < apm.m_packageEntries.Length; ++i)
            {
                PackageEntry entry = apm.m_packageEntries[i];
                PackageRecord[] records = apm.m_packageRecords[i];

                foreach (PackageRecord record in records.Where(x => guids.Contains(x.m_GUID) || guids.Contains(new teResourceGUID(x.m_GUID).Type) || guids.Contains(new teResourceGUID(x.m_GUID).Index) || guids.Contains(new teResourceGUID(x.m_GUID).Key)))
                {
                    Log("Found {0} in package {1:X12}", teResourceGUID.AsString(record.m_GUID), new teResourceGUID(entry.m_packageGUID).Key);
                }
            }
        }

        private static void SearchType(IEnumerable<string> args)
        {
            ulong[] guids = args.Select(x => ulong.Parse(x, NumberStyles.HexNumber)).ToArray();

            ApplicationPackageManifest apm = TankHandler.m_packageManifest;
            for (int i = 0; i < apm.m_packageEntries.Length; ++i)
            {
                PackageEntry entry = apm.m_packageEntries[i];
                PackageRecord[] records = apm.m_packageRecords[i];

                foreach (PackageRecord record in records.Where(x => guids.Contains(new teResourceGUID(x.m_GUID).Type)))
                {
                    Log("Found {0} in package {1:X12}", teResourceGUID.AsString(record.m_GUID), new teResourceGUID(entry.m_packageGUID).Type);
                }
            }
        }

        private static void Info(IEnumerable<string> args)
        {
            ulong[] guids = args.Select(x => ulong.Parse(x, NumberStyles.HexNumber)).ToArray();

            ApplicationPackageManifest apm = TankHandler.m_packageManifest;
            for (int i = 0; i < apm.m_packageEntries.Length; ++i)
            {
                PackageEntry entry = apm.m_packageEntries[i];
                if (!guids.Contains(new teResourceGUID(entry.m_packageGUID).Key) && !guids.Contains(new teResourceGUID(entry.m_packageGUID).Index)) continue;
                Log("Package {0:X12}:", new teResourceGUID(entry.m_packageGUID).Key);
                Log("\tUnknowns: {0}, {1}", entry.m_unknown1, entry.m_unknown2);
                Log("\t{0} records", apm.m_packageRecords[i].Length);
            }
        }
    }
}
