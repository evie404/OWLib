using System;
using System.Collections.Generic;
using DataTool.DataModels;
using DataTool.Flag;
using DataTool.Helper;
using DataTool.JSON;
using TankLib;
using TankLib.Helpers;
using TankLib.STU.Types;
using static DataTool.Program;
using static DataTool.Helper.Logger;
using static DataTool.Helper.STUHelper;
using Logger = TankLib.Helpers.Logger;

namespace DataTool.ToolLogic.List {
    [Tool("list-heroes", Description = "List heroes", CustomFlags = typeof(ListFlags))]
    public class ListHeroes : JSONTool, ITool {
        public void Parse(ICLIFlags toolFlags) {
            var heroes = GetHeroes();

            if (toolFlags is ListFlags flags)
                if (flags.JSON) {
                    OutputJSON(heroes, flags);
                    return;
                }

            var indentLevel = new IndentHelper();

            foreach (var hero in heroes) {
                Log($"{hero.Value.Name}");
                if (hero.Value.Description != null)
                    Log($"{indentLevel + 1}Description: {hero.Value.Description}");

                Log($"{indentLevel + 1}Gender: {hero.Value.Gender}");

                Log($"{indentLevel + 1}Size: {hero.Value.Size}");

                Logger.Log24Bit(ConsoleSwatch.ColorReset,               null, false, Console.Out, null, $"{indentLevel + 1}Color: {hero.Value.GalleryColor.ToHex()} ");
                Logger.Log24Bit(hero.Value.GalleryColor.ToForeground(), null, true,  Console.Out, null, "██████");

                if (hero.Value.Loadouts != null) {
                    Log($"{indentLevel + 1}Loadouts:");
                    foreach (var loadout in hero.Value.Loadouts) {
                        Log($"{indentLevel + 2}{loadout.Name}: {loadout.Category}");
                        Log($"{indentLevel + 3}{loadout.Description}");
                    }
                }

                Log();
            }
        }

        public Dictionary<teResourceGUID, Hero> GetHeroes() {
            var @return = new Dictionary<teResourceGUID, Hero>();

            foreach (teResourceGUID key in TrackedFiles[0x75]) {
                var hero = GetInstance<STUHero>(key);
                if (hero == null) continue;

                @return[key] = new Hero(hero);
            }

            return @return;
        }
    }
}
