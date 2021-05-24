﻿using System.Collections.Generic;
using System.Linq;
using DataTool.DataModels;
using DragonLib.CLI;
using DataTool.JSON;
using static DataTool.Program;
using static DataTool.Helper.Logger;

namespace DataTool.ToolLogic.List.Misc {
    [Tool("list-arcade-modes", Description = "List arcade modes", CustomFlags = typeof(ListFlags), IsSensitive = true)]
    public class ListArcadeModes : JSONTool, ITool {
        public void Parse(ICLIFlags toolFlags) {
            var data = GetData();

            if (toolFlags is ListFlags flags)
                if (flags.JSON) {
                    OutputJSON(data, flags);
                    return;
                }

            foreach (var arcade in data) {
                Log($"{arcade.Name}:");
                Log($"\tDescription: {arcade.Description}");

                if (arcade.Brawl != 0)
                    Log($"\tBrawl: {arcade.Brawl.ToString()}");

                if (arcade.Children != null)
                    Log($"\tChildren: {string.Join(", ", arcade.Children.Select(x => x.ToString()))}");

                Log();
            }
        }

        private static List<ArcadeMode> GetData() {
            var arcades = new List<ArcadeMode>();

            foreach (ulong key in TrackedFiles[0xEE]) {
                var arcade = new ArcadeMode(key);

                if (arcade.GUID != 0)
                    arcades.Add(arcade);
            }

            return arcades;
        }
    }
}
