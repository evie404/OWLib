using TankLib.Helpers.DataSerializer;

namespace TankLib.Replay {
    public class HeroData : ReadableData {
        public uint AnnouncerId; // this actually is an educated guess, every hero has a cosmetic category which has one cosmetic (total.) This used to be the same for weapon skins.

        [Logical.DynamicSizeArrayAttribute(typeof(int), typeof(uint))]
        public uint[] EmoteIds;

        // Since there's an underlying system for announcer logic including it's own STU object, it's safe to assume this.
        public teResourceGUID Hero;
        public uint           POTGAnimation;
        public uint           SkinId;

        [Logical.DynamicSizeArrayAttribute(typeof(int), typeof(uint))]
        public uint[] SprayIds;

        [Logical.DynamicSizeArrayAttribute(typeof(int), typeof(uint))]
        public uint[] VoiceLineIds;

        public uint WeaponSkinId;
    }
}
