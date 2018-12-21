using System.Collections.Generic;
using System.Linq;
using DataTool.DataModels;
using DataTool.Flag;
using DataTool.JSON;
using TankLib.STU.Types;
using static DataTool.Helper.IO;
using static DataTool.Program;
using static DataTool.Helper.Logger;
using static DataTool.Helper.STUHelper;

namespace DataTool.ToolLogic.List {
    [Tool("list-unlocks", Description = "List hero unlocks", CustomFlags = typeof(ListFlags))]
    public class ListHeroUnlocks : JSONTool, ITool {
        public void Parse(ICLIFlags toolFlags) {
            var unlocks = GetUnlocks();

            if (toolFlags is ListFlags flags)
                if (flags.JSON) {
                    OutputJSON(unlocks, flags);
                    return;
                }

            foreach (var heroPair in unlocks) {
                Log("Unlocks for {0}", heroPair.Key);

                if (heroPair.Value.LevelUnlocks != null)
                    foreach (var levelUnlocks in heroPair.Value.LevelUnlocks)
                        DisplayUnlocks("Default", levelUnlocks.Unlocks);

                if (heroPair.Value.OtherUnlocks != null) {
                    var owlUnlocks = heroPair.Value.OtherUnlocks.Where(u => u.STU.m_0B1BA7C1 != null)
                                             .ToArray();
                    var otherUnlocks = heroPair.Value.OtherUnlocks.Where(u => u.STU.m_0B1BA7C1 == null)
                                               .ToArray();
                    DisplayUnlocks("Other", otherUnlocks);
                    DisplayUnlocks("OWL",   owlUnlocks);
                }

                if (heroPair.Value.LootBoxesUnlocks != null)
                    foreach (var lootBoxUnlocks in heroPair.Value.LootBoxesUnlocks) {
                        var boxName = LootBox.GetName(lootBoxUnlocks.LootBoxType);

                        DisplayUnlocks(boxName, lootBoxUnlocks.Unlocks);
                    }

                Log(); // New line
            }
        }

        public static void DisplayUnlocks(string category, Unlock[] unlocks, string start = "") {
            if (unlocks == null || unlocks.Length == 0) return;
            Log($"{start}\t{category} Unlocks");

            foreach (var unlock in unlocks) {
                Log($"{start}\t\t{unlock.GetName(true)} ({unlock.Rarity} {unlock.Type})");
                if (!string.IsNullOrEmpty(unlock.Description)) Log($"{start}\t\t\t{unlock.Description}");
                if (!string.IsNullOrEmpty(unlock.AvailableIn)) Log($"{start}\t\t\t{unlock.AvailableIn}");
            }
        }

        public static Dictionary<string, ProgressionUnlocks> GetUnlocks() {
            var @return = new Dictionary<string, ProgressionUnlocks>();
            foreach (var key in TrackedFiles[0x75]) {
                var hero = GetInstance<STUHero>(key);
                if (hero == null) continue;

                var name = GetString(hero.m_0EDCE350);
                if (name == null) continue;

                var unlocks = new ProgressionUnlocks(hero);

                @return[name] = unlocks;
            }

            return @return;
        }
    }
}
