using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DataTool.DataModels;
using DataTool.DataModels.Hero;
using DataTool.Helper;
using TankLib;
using TankLib.Helpers;
using TankLib.STU.Types;

namespace ReplayMp4Tool {
    public static class ReplayThing {
        private static IEnumerable<string[]> ProcessAtoms(Memory<byte> buffer) {
            var cursor = 0;
            while (cursor < buffer.Length) {
                var atom = new MP4Atom(buffer.Span.Slice(cursor));
                cursor += atom.Size;
                if (atom.Name == "moov" || atom.Name == "udta") {
                    foreach (var str in ProcessAtoms(atom.Buffer)) {
                        yield return str;
                    }

                    continue;
                }

                if (atom.Name != "Xtra") continue; // moov -> udta -> Xtra

                if (atom.Buffer.Length < 0x1F) {
                    Console.Out.WriteLine("\nReplay is fucked\n");
                    continue;
                }

                var localCursor = 0;
                var blockSize = BinaryPrimitives.ReadInt32BigEndian(atom.Buffer.Span);
                if (blockSize != atom.Buffer.Length) {
                    Console.Out.WriteLine("\nReplay has a lot of data?\n");
                }

                localCursor += 4;
                var blockNameLength = BinaryPrimitives.ReadInt32BigEndian(atom.Buffer.Span.Slice(localCursor));
                if (blockNameLength == 0) continue;
                localCursor += 4;
                var name = Encoding.ASCII.GetString(atom.Buffer.Span.Slice(localCursor, blockNameLength).ToArray());
                localCursor += blockNameLength;
                if (name != "WM/EncodingSettings") {
                    Console.Out.WriteLine("\nReplay is fucked\n");
                    continue;
                }

                var settingCount = BinaryPrimitives.ReadInt32BigEndian(atom.Buffer.Span.Slice(localCursor));
                localCursor += 4;
                for (var i = 0; i < settingCount; ++i) {
                    var encodedSettingLength = BinaryPrimitives.ReadInt32BigEndian(atom.Buffer.Span.Slice(localCursor));
                    if (encodedSettingLength == 0) continue;
                    var type = BinaryPrimitives.ReadInt16BigEndian(atom.Buffer.Span.Slice(localCursor + 4));
                    if (type != 8) {
                        Console.Out.WriteLine("\nNot Type 8?\n");
                    }

                    var data = atom.Buffer.Span.Slice(localCursor + 6, encodedSettingLength - 6);
                    var b64Str = Encoding.Unicode.GetString(data.ToArray());
                    localCursor += encodedSettingLength;

                    yield return b64Str.Split(':');
                }
            }
        }

        public static void ParseReplay(string filePath) {
            var buffer = (Memory<byte>) File.ReadAllBytes(filePath);
            if (buffer.Length == 0) return;

            foreach (var b64Str in ProcessAtoms(buffer)) { // hash, payload, settinghash?
                byte[] bytes = Convert.FromBase64String(b64Str[1]);
                // string hex = BitConverter.ToString(bytes);

                var replayInfo = new Mp4Replay();
                replayInfo.Parse(bytes);

                var heroStu = STUHelper.GetInstance<STUHero>(replayInfo.Header.HeroGuid);
                var hero = new Hero(heroStu);
                var unlocks = new ProgressionUnlocks(heroStu);
                var skins = unlocks.GetUnlocksOfType("Skin");
                var skinTheme = skins.FirstOrDefault(skin => ((STUUnlock_SkinTheme) skin.STU)?.m_skinTheme == replayInfo.Header.SkinGuid);

                ulong mapHeaderGuid = (replayInfo.Header.MapGuid & ~0xFFFFFFFF00000000ul) | 0x0790000000000000ul;
                var mapData = new MapHeader(mapHeaderGuid);

                Console.Out.WriteLine("\nReplay Info\n");
                Console.Out.WriteLine($"Hero: {hero.Name}");
                Console.Out.WriteLine($"Map: {mapData.Name}");
                Console.Out.WriteLine($"Skin: {skinTheme?.Name ?? "Unknown"}");
                Console.Out.WriteLine($"Recorded At: {DateTimeOffset.FromUnixTimeSeconds(replayInfo.Header.Timestamp)}");
                Console.Out.WriteLine($"Type: {replayInfo.Header.Type:G}");
                Console.Out.WriteLine($"Quality: {replayInfo.Header.QualityPct}% ({(ReplayQuality) replayInfo.Header.QualityPct})");
            }
        }

        public class Mp4Replay {
            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            public struct Structure {
                public teResourceGUID MapGuid;
                public teResourceGUID HeroGuid;
                public teResourceGUID SkinGuid;
                public long Timestamp;
                public ulong UserId;
                public ReplayType Type;
                public int QualityPct;
            }

            public Structure Header;

            public void Parse(byte[] bytes) {
                Header = FastStruct<Structure>.ArrayToStructure(bytes);
            }
        }

        public enum ReplayType {
            Manual = 0,
            Play = 2,
            Top5 = 8,
        }
        
        public enum ReplayQuality
        {
            Low = 30,
            Medium = 50,
            High = 80,
            Ultra = 100
        }
    }
}
