using System;
using System.IO;
using DataTool.DataModels;
using DataTool.FindLogic;
using DataTool.Flag;
using TankLib.STU.Types;
using static DataTool.Program;
using static DataTool.Helper.STUHelper;

namespace DataTool.ToolLogic.Extract {
    [Tool("extract-lootbox", Description = "Extract lootbox models", CustomFlags = typeof(ExtractFlags))]
    public class ExtractLootbox : ITool {
        public const string Container = "LootBoxes";

        public void Parse(ICLIFlags toolFlags) { GetLootboxes(toolFlags); }

        public void GetLootboxes(ICLIFlags toolFlags) {
            string basePath;
            if (toolFlags is ExtractFlags flags)
                basePath = flags.OutputPath;
            else
                throw new Exception("no output path");


            foreach (var key in TrackedFiles[0xCF]) {
                var lootbox = GetInstance<STULootBox>(key);
                if (lootbox == null) continue;

                var name = LootBox.GetName(lootbox.m_lootboxType);

                var info = Combo.Find(null, lootbox.m_baseEntity); // 003
                Combo.Find(info, lootbox.m_chestEntity);           // 003
                Combo.Find(info, lootbox.m_idleEffect);            // 00D
                Combo.Find(info, lootbox.m_FEC3ED62);              // 00D
                Combo.Find(info, lootbox.m_FFE7768F);              // 00D
                Combo.Find(info, lootbox.m_baseModelLook);         // 01A
                Combo.Find(info, lootbox.m_modelLook);

                Combo.Find(info, 0x400000000001456); // coin chest, todo
                // 00000000315A.00C in 000000001456.003 (288230376151716950)

                foreach (var lootboxShopCard in lootbox.m_shopCards) Combo.Find(info, lootboxShopCard.m_cardTexture); // 004
                SaveLogic.Combo.SaveLooseTextures(flags, Path.Combine(basePath, Container, name, "ShopCards"), info);
                SaveLogic.Combo.Save(flags, Path.Combine(basePath,              Container, name), info);
            }
        }
    }
}
