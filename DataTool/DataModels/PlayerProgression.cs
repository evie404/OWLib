using System.Runtime.Serialization;
using TankLib.STU.Types;

namespace DataTool.DataModels {
    [DataContract]
    public class PlayerProgression {
        [DataMember]
        public AdditionalUnlocks[] AdditionalUnlocks;

        [DataMember]
        public LootBoxUnlocks[] LootBoxesUnlocks;

        [DataMember]
        public Unlock[] OtherUnlocks;

        public PlayerProgression(STUGenericSettings_PlayerProgression progression) {
            if (progression.m_lootBoxesUnlocks != null) {
                LootBoxesUnlocks = new LootBoxUnlocks[progression.m_lootBoxesUnlocks.Length];

                for (var i = 0; i < progression.m_lootBoxesUnlocks.Length; i++) {
                    var lootBoxUnlocks = progression.m_lootBoxesUnlocks[i];
                    LootBoxesUnlocks[i] = new LootBoxUnlocks(lootBoxUnlocks);
                }
            }

            if (progression.m_additionalUnlocks != null) {
                AdditionalUnlocks = new AdditionalUnlocks[progression.m_additionalUnlocks.Length];
                for (var i = 0; i < progression.m_additionalUnlocks.Length; i++) AdditionalUnlocks[i] = new AdditionalUnlocks(progression.m_additionalUnlocks[i]);
            }

            OtherUnlocks = Unlock.GetArray(progression.m_otherUnlocks);
        }
    }

    /// <summary>
    ///     Additional Unlocks data model
    /// </summary>
    [DataContract]
    public class AdditionalUnlocks {
        /// <summary>
        ///     Level unlocked at
        /// </summary>
        [DataMember]
        public uint Level;

        /// <summary>
        ///     Unlocks
        /// </summary>
        [DataMember]
        public Unlock[] Unlocks;

        public AdditionalUnlocks(STUAdditionalUnlocks additionalUnlocks) {
            Level   = additionalUnlocks.m_level;
            Unlocks = Unlock.GetArray(additionalUnlocks.m_unlocks);
        }
    }
}
