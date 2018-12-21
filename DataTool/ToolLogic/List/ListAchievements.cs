using System.Collections.Generic;
using DataTool.DataModels;
using DataTool.Flag;
using DataTool.Helper;
using DataTool.JSON;
using TankLib.STU.Types;
using static DataTool.Program;
using static DataTool.Helper.Logger;
using static DataTool.Helper.STUHelper;

namespace DataTool.ToolLogic.List {
    [Tool("list-achievements", Description = "List achievements", CustomFlags = typeof(ListFlags))]
    public class ListAchievements : JSONTool, ITool {
        public void Parse(ICLIFlags toolFlags) {
            var achievements = GetAchievements();

            if (toolFlags is ListFlags flags)
                if (flags.JSON) {
                    OutputJSON(achievements, flags);
                    return;
                }

            foreach (var achievement in achievements) {
                var iD = new IndentHelper();

                Log($"{achievement.Name}");
                Log($"{iD + 1}Description: {achievement.Description}");

                if (achievement.Reward != null)
                    Log($"{iD + 1}Reward: {achievement.Reward.Name} ({achievement.Reward.Rarity} {achievement.Reward.Type})");

                Log();
            }
        }

        public List<Achievement> GetAchievements() {
            var achievements = new List<Achievement>();

            foreach (var key in TrackedFiles[0x68]) {
                var achievement = GetInstance<STUAchievement>(key);
                if (achievement == null) continue;

                var model = new Achievement(achievement);
                achievements.Add(model);
            }

            return achievements;
        }
    }
}
