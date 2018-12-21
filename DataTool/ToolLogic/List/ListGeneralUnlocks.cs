using DataTool.DataModels;
using DataTool.Flag;
using DataTool.JSON;
using TankLib.STU.Types;
using static DataTool.Program;
using static DataTool.Helper.STUHelper;

namespace DataTool.ToolLogic.List {
    [Tool("list-general-unlocks", Description = "List general unlocks", CustomFlags = typeof(ListFlags))]
    public class ListGeneralUnlocks : JSONTool, ITool {
        public void Parse(ICLIFlags toolFlags) {
            var unlocks = GetUnlocks();

            if (toolFlags is ListFlags flags)
                if (flags.JSON) {
                    OutputJSON(unlocks, flags);
                    return;
                }

            ListHeroUnlocks.DisplayUnlocks("Other", unlocks.OtherUnlocks);

            if (unlocks.LootBoxesUnlocks != null)
                foreach (var lootBoxUnlocks in unlocks.LootBoxesUnlocks) {
                    var boxName = LootBox.GetName(lootBoxUnlocks.LootBoxType);

                    ListHeroUnlocks.DisplayUnlocks(boxName, lootBoxUnlocks.Unlocks);
                }

            if (unlocks.AdditionalUnlocks != null)
                foreach (var additionalUnlocks in unlocks.AdditionalUnlocks)
                    ListHeroUnlocks.DisplayUnlocks($"Level {additionalUnlocks.Level}", additionalUnlocks.Unlocks);
        }

        public PlayerProgression GetUnlocks() {
            foreach (var key in TrackedFiles[0x54]) {
                var playerProgression = GetInstance<STUGenericSettings_PlayerProgression>(key);
                if (playerProgression == null) continue;

                return new PlayerProgression(playerProgression);
            }

            return null;
        }
    }
}
