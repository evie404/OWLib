using System.Runtime.Serialization;
using DataTool.Helper;
using TankLib.STU.Types;
using TankLib.STU.Types.Enums;

namespace DataTool.DataModels {
    /// <summary>
    ///     Progression data model
    /// </summary>
    [DataContract]
    public class ProgressionUnlocks {
        /// <summary>
        ///     Unlocks granted at a specific level
        /// </summary>
        [DataMember]
        public LevelUnlocks[] LevelUnlocks;

        /// <summary>
        ///     Loot Box specific unlocks
        /// </summary>
        [DataMember]
        public LootBoxUnlocks[] LootBoxesUnlocks;

        /// <summary>
        ///     "Other" Unlocks. Common examples are OWL skins and achievement rewards
        /// </summary>
        [DataMember]
        public Unlock[] OtherUnlocks;

        /// <summary>
        ///     Unknown Unlocks
        /// </summary>
        [DataMember]
        public Unlock[] UnknownUnlocks;

        public ProgressionUnlocks(STUHero hero) {
            var unlocks = STUHelper.GetInstance<STUProgressionUnlocks>(hero.m_heroProgression);
            Init(unlocks);
        }

        public ProgressionUnlocks(ulong guid) {
            var unlocks = STUHelper.GetInstance<STUProgressionUnlocks>(guid);
            Init(unlocks);
        }

        private void Init(STUProgressionUnlocks progressionUnlocks) {
            if (progressionUnlocks == null) return;

            if (progressionUnlocks.m_lootBoxesUnlocks != null) {
                LootBoxesUnlocks = new LootBoxUnlocks[progressionUnlocks.m_lootBoxesUnlocks.Length];

                for (var i = 0; i < progressionUnlocks.m_lootBoxesUnlocks.Length; i++) {
                    var lootBoxUnlocks = progressionUnlocks.m_lootBoxesUnlocks[i];
                    LootBoxesUnlocks[i] = new LootBoxUnlocks(lootBoxUnlocks);
                }
            }

            if (progressionUnlocks.m_7846C401 != null) {
                LevelUnlocks = new LevelUnlocks[progressionUnlocks.m_7846C401.Length];
                for (var i = 0; i < LevelUnlocks.Length; i++) {
                    var levelUnlocks = progressionUnlocks.m_7846C401[i];
                    LevelUnlocks[i] = new LevelUnlocks(levelUnlocks);
                }
            }

            OtherUnlocks   = Unlock.GetArray(progressionUnlocks.m_otherUnlocks);
            UnknownUnlocks = Unlock.GetArray(progressionUnlocks.m_9135A4B2);
        }
    }

    [DataContract]
    public class LootBoxUnlocks {
        /// <summary>
        ///     Loot Box type
        /// </summary>
        /// <see cref="Enum_BABC4175" />
        public Enum_BABC4175 LootBoxType;

        /// <summary>
        ///     Unlocks
        /// </summary>
        [DataMember]
        public Unlock[] Unlocks;

        public LootBoxUnlocks(STULootBoxUnlocks lootBoxUnlocks) {
            LootBoxType = lootBoxUnlocks.m_lootboxType;
            Unlocks     = Unlock.GetArray(lootBoxUnlocks.m_unlocks);
        }
    }

    /// <summary>
    ///     Level Unlocks data model
    /// </summary>
    [DataContract]
    public class LevelUnlocks {
        /// <summary>
        ///     Level unlocked at
        /// </summary>
        public int Level;

        /// <summary>
        ///     Unlocks
        /// </summary>
        public Unlock[] Unlocks;

        public LevelUnlocks(STU_1757E817 levelUnlocks) {
            Level   = levelUnlocks.m_level;
            Unlocks = Unlock.GetArray(levelUnlocks.m_unlocks);
        }
    }
}
