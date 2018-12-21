using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using DataTool.Flag;
using DataTool.Helper;
using DataTool.JSON;
using TankLib.Replay;
using TankLib.STU.Types;
using static DataTool.Helper.IO;
using static DataTool.Helper.STUHelper;
using static DataTool.Helper.Logger;

namespace DataTool.ToolLogic.List {
    [Tool("list-highlights", Description = "List user highlights", CustomFlags = typeof(ListFlags))]
    public class ListHighlights : JSONTool, ITool {
        public void Parse(ICLIFlags toolFlags) {
            var highlights = new List<HighlightJSON>();

            var overwatchAppdataFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Blizzard Entertainment/Overwatch"));
            foreach (var userFolder in overwatchAppdataFolder.GetDirectories()) {
                var highlightsFolder = new DirectoryInfo(Path.Combine(userFolder.FullName, $"{(Program.IsPTR ? "PTR\\" : "")}Highlights"));
                if (!highlightsFolder.Exists) continue;
                foreach (var file in highlightsFolder.GetFiles())
                    try {
                        highlights.Add(GetHighlight(file.FullName));
                    } catch (Exception) {
                        Console.Out.WriteLine($"unable to parse {file.Name}");
                    }
            }

            if (toolFlags is ListFlags flags)
                if (flags.JSON) {
                    OutputJSON(highlights, flags);
                    return;
                }

            // todo: timestamp
            var  indent = new IndentHelper();
            uint count  = 0;
            foreach (var highlight in highlights) {
                if (count != 0) Log($"{Environment.NewLine}");
                Log($"{indent}{highlight.UUID}:");
                Log($"{indent + 1}Map: {highlight.Map}");
                Log($"{indent + 1}Gamemode: {highlight.GameMode}");
                Log($"{indent + 1}HighlightInfo:");
                for (var i = 0; i < highlight.HighlightInfo.Count; i++) {
                    Log($"{indent + 2}[{i}] {{");
                    PrintHighlightInfoJSON(highlight.HighlightInfo[i], indent + 3);
                    Log($"{indent + 2}}}");
                }

                Log($"{indent + 1}Heroes:");
                for (var i = 0; i < highlight.HeroInfo.Count; i++) {
                    Log($"{indent + 2}[{i}] {{");
                    Log($"{indent + 3}Hero: {highlight.HeroInfo[i].Hero}");
                    Log($"{indent + 3}Skin: {highlight.HeroInfo[i].Skin}");
                    Log($"{indent + 3}HiglightIntro: {highlight.HeroInfo[i].HighlightIntro}");

                    Log($"{indent + 3}Sprays:");
                    foreach (var spray in highlight.HeroInfo[i]
                                                   .Sprays) Log($"{indent + 4}{spray}");

                    Log($"{indent + 3}Emotes:");
                    foreach (var emote in highlight.HeroInfo[i]
                                                   .Emotes) Log($"{indent + 4}{emote}");

                    Log($"{indent + 3}VoiceLines:");
                    foreach (var voiceLine in highlight.HeroInfo[i]
                                                       .VoiceLines) Log($"{indent + 4}{voiceLine}");

                    Log($"{indent + 2}}}");
                }

                Log($"{indent + 1}Replay: {{");
                Log($"{indent + 2}Map: {highlight.Replay.Map}");
                Log($"{indent + 2}Gamemode: {highlight.Replay.GameMode}");
                Log($"{indent + 2}HighlightInfo:");
                PrintHighlightInfoJSON(highlight.Replay.HighlightInfo, indent + 3);
                Log($"{indent + 1}}}");
                count++;
            }
        }

        public void PrintHighlightInfoJSON(HighlightInfoJSON info, IndentHelper indent) {
            Log($"{indent}Player: {info.Player}");
            Log($"{indent}Hero: {info.Hero}");
            Log($"{indent}Skin: {info.Skin}");
            Log($"{indent}Weapon: {info.WeaponSkin}");
            Log($"{indent}HighlightType: {info.HighlightType}");
        }

        protected ulong GetCosmeticKey(uint key) { return (key & ~0xFFFFFFFF00000000ul) | 0x0250000000000000ul; }

        protected HighlightInfoJSON GetHighlightInfo(tePlayerHighlight.HighlightInfo infoNew) {
            var outputJson = new HighlightInfoJSON();
            var hero       = GetInstance<STUHero>(infoNew.Hero);

            outputJson.Hero   = GetString(hero?.m_0EDCE350);
            outputJson.Player = infoNew.PlayerName;

            var intro = GetInstance<STUUnlock_POTGAnimation>(infoNew.HighlightIntro);
            outputJson.HighlightIntro = GetString(intro.m_name);

            // todo: outputJson.WeaponSkin
            // todo: outputJson.Skin

            var highlightType = GetInstance<STU_C25281C3>(infoNew.HighlightType);
            outputJson.HighlightType = GetString(highlightType?.m_description) ?? "";
            return outputJson;
        }

        protected static string GetMapName(ulong key) {
            var map = GetInstance<STUMapHeader>(key);
            return GetString(map.m_displayName);
        }

        protected HeroInfoJSON GetHeroInfo(HeroData heroInfo) {
            var hero = GetInstance<STUHero>(heroInfo.Hero);

            var outputHero = new HeroInfoJSON {
                                                  Hero       = GetString(hero.m_0EDCE350),
                                                  Sprays     = new List<string>(),
                                                  Emotes     = new List<string>(),
                                                  VoiceLines = new List<string>()
                                              };
            foreach (var sprayId in heroInfo.SprayIds) {
                var spray = GetInstance<STUUnlock_SprayPaint>(GetCosmeticKey(sprayId));
                outputHero.Sprays.Add(GetString(spray.m_name));
            }

            foreach (var emoteId in heroInfo.EmoteIds) {
                var emote = GetInstance<STUUnlock_Emote>(GetCosmeticKey(emoteId));
                outputHero.Emotes.Add(GetString(emote.m_name));
            }

            foreach (var voiceLineId in heroInfo.VoiceLineIds) {
                var voiceLine = GetInstance<STUUnlock_VoiceLine>(GetCosmeticKey(voiceLineId));
                outputHero.VoiceLines.Add(GetString(voiceLine.m_name));
            }

            var intro = GetInstance<STUUnlock_POTGAnimation>(GetCosmeticKey(heroInfo.POTGAnimation));
            outputHero.HighlightIntro = GetString(intro.m_name);

            // Skin skin = GetInstance<Skin>(GetSkinKey(heroInfo.SkinId));  // todo: this is by skin override
            // outputHero.Skin = GetString(skin?.CosmeticName);

            // Weapon weaponSkin = GetInstance<Weapon>(GetCosmeticKey(heroInfo.WeaponSkinId));  // todo: this is by weapon skin override
            // outputHero.WeaponSkin = GetString(weaponSkin?.CosmeticName);

            return outputHero;
        }

        protected string GetGamemode(ulong guid) {
            var gamemode = GetInstance<STUGameMode>(guid);
            return GetString(gamemode?.m_displayName);
        }

        protected ReplayJSON GetReplay(tePlayerReplay playerReplay) {
            var output = new ReplayJSON { BuildNumber = playerReplay.BuildNumber };

            var mapMetadataKey = (playerReplay.Map & ~0xFFFFFFFF00000000ul) | 0x0790000000000000ul;
            output.Map           = GetMapName(mapMetadataKey);
            output.HighlightInfo = GetHighlightInfo(playerReplay.HighlightInfo);
            output.GameMode      = GetGamemode(playerReplay.GameMode);

            return output;
        }

        public HighlightJSON GetHighlight(string file) {
            var playerHighlight = new tePlayerHighlight(File.OpenRead(file));

            var output = new HighlightJSON {
                                               PlayerID      = playerHighlight.PlayerId,
                                               Flags         = playerHighlight.Flags.ToString(),
                                               HeroInfo      = new List<HeroInfoJSON>(),
                                               HighlightInfo = new List<HighlightInfoJSON>(),
                                               UUID = playerHighlight.Info.FirstOrDefault()
                                                                     ?.UUID.Value.ToString()
                                           };

            var mapHeaderGuid = (playerHighlight.Map & ~0xFFFFFFFF00000000ul) | 0x0790000000000000ul;
            output.Map = GetMapName(mapHeaderGuid);

            foreach (var heroInfo in playerHighlight.Heroes) output.HeroInfo.Add(GetHeroInfo(heroInfo));

            foreach (var infoNew in playerHighlight.Info) output.HighlightInfo.Add(GetHighlightInfo(infoNew));

            output.Replay   = GetReplay(new tePlayerReplay(playerHighlight.Replay));
            output.GameMode = GetGamemode(playerHighlight.GameMode);

            return output;
        }

        [DataContract]
        public class ReplayJSON {
            public uint              BuildNumber;
            public string            GameMode;
            public HighlightInfoJSON HighlightInfo;
            public string            Map;
        }


        [DataContract]
        public class HeroInfoJSON {
            public List<string> Emotes;
            public string       Hero;
            public string       HighlightIntro;
            public string       Skin;
            public List<string> Sprays;
            public List<string> VoiceLines;
        }

        [DataContract]
        public class HighlightInfoJSON {
            public string Hero;
            public string HighlightIntro;
            public string HighlightType;
            public string Player;
            public string Skin;
            public string WeaponSkin;
        }

        [DataContract]
        public class HighlightJSON {
            public string                  Flags;
            public string                  GameMode;
            public List<HeroInfoJSON>      HeroInfo;
            public List<HighlightInfoJSON> HighlightInfo;
            public string                  Map;
            public long                    PlayerID;
            public ReplayJSON              Replay;
            public string                  UUID;
        }
    }
}
