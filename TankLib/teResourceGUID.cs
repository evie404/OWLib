using System;
using System.Runtime.InteropServices;

// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantCaseLabel

namespace TankLib {
    
    /// <summary>Tank GUID</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct teResourceGUID : IFormattable {
        
        /// <summary>GUID Value</summary>
        public ulong GUID { get; set; }

        [Flags]
        public enum teResourceGUIDAttribute : ulong {
            Index    = 0x00000000FFFFFFFF, // 32 bits
            Locale   = 0x0000001F00000000, // 5 bits
            Reserved = 0x0000006000000000, // 2 bits
            Region   = 0x00000F8000000000, // 5 bits
            Platform = 0x0000F00000000000, // 4 bits
            Type     = 0x0FFF000000000000, // 12 bits
            Engine   = 0xF000000000000000, // 4 bits

            //
            Key      = 0x0000FFFFFFFFFFFFF // 48 bits
        }

        public teResourceGUID(ulong guid) {
            GUID = guid;
        }

        public ulong GetAttribute(teResourceGUIDAttribute attributes) {
            return GUID & (ulong) attributes;
        }

        private void SetAttribute(ulong value, teResourceGUIDAttribute attributes) {
            GUID |= (value & (ulong) attributes);
        }

        /// <summary>ID without type info</summary>
        public ulong Key {
            get => GetAttribute(teResourceGUIDAttribute.Key);
            set => SetAttribute(value, teResourceGUIDAttribute.Key);
        }

        /// <summary>Unique ID</summary>
        public uint Index {
            get => (uint) GetAttribute(teResourceGUIDAttribute.Index);
            set => SetAttribute(value, teResourceGUIDAttribute.Index);
        }

        /// <summary>The locale that this GUID was authored for</summary>
        public byte Locale {
            get => (byte) (GetAttribute(teResourceGUIDAttribute.Locale) >> 32);
            set => SetAttribute((ulong) value << 32, teResourceGUIDAttribute.Locale);
        }

        /// <summary>The reserved component</summary>
        public byte Reserved {
            get => (byte) (GetAttribute(teResourceGUIDAttribute.Reserved) >> 37);
            set => SetAttribute((ulong) value << 37, teResourceGUIDAttribute.Reserved);
        }

        /// <summary>Region that this GUID was authored for</summary>
        public byte Region {
            get => (byte) (GetAttribute(teResourceGUIDAttribute.Region) >> 39);
            set => SetAttribute((ulong) value << 39, teResourceGUIDAttribute.Region);
        }

        /// <summary>Platform that this GUID was authored for</summary>
        public byte Platform {
            get => (byte) (GetAttribute(teResourceGUIDAttribute.Platform) >> 44);
            set => SetAttribute((ulong) value << 44, teResourceGUIDAttribute.Platform);
        }

        /// <summary>Demangled GUID type</summary>
        public ushort Type {
            get => (ushort) (FlipTypeBits(MangledType) + 1);
            set => MangledType = (ushort) FlipTypeBits((ulong) (value) - 1);
        }

        private static ulong FlipTypeBits(ulong num) {
            num = ((num >> 1) & 0x55555555) | ((num & 0x55555555) << 1);
            num = ((num >> 2) & 0x33333333) | ((num & 0x33333333) << 2);
            num = ((num >> 4) & 0x0F0F0F0F) | ((num & 0x0F0F0F0F) << 4);
            num = ((num >> 8) & 0x00FF00FF) | ((num & 0x00FF00FF) << 8);
            num = (num >> 16) | (num << 16);
            num >>= 20;
            return num;
        }

        /// <summary>Type of this GUID, but manged</summary>
        public ushort MangledType {
            get => (ushort) (GetAttribute(teResourceGUIDAttribute.Type) >> 48);
            set => SetAttribute((ulong) value << 48, teResourceGUIDAttribute.Type);
        }

        /// <summary>Reserved engine component of this GUID</summary>
        public byte Engine {
            get => (byte) (GetAttribute(teResourceGUIDAttribute.Engine) >> 60);
            set => SetAttribute((ulong) value << 60, teResourceGUIDAttribute.Engine);
        }

        public static implicit operator ulong(teResourceGUID guid) {
            return guid.GUID;
        }

        public static explicit operator teResourceGUID(ulong guid) {
            return new teResourceGUID(guid);
        }

        /// <summary>String representation of this GUID</summary>
        public override string ToString() {
            return ToString("S");
        }

        public string ToString(string format, IFormatProvider formatProvider = null) {
            switch (format) {
                default:
                case "S":
                    return $"{Key.ToString("X12", formatProvider)}.{Type.ToString("X3", formatProvider)}";
                case "X":
                    return GUID.ToString("X16", formatProvider);
            }
        }

        /// <summary>String representation a GUID</summary>
        public static string AsString(ulong guid) {
            return new teResourceGUID(guid).ToString("S");
        }

        /// <summary>String representation a GUID</summary>
        public static string AsHexString(ulong guid) {
            return new teResourceGUID(guid).ToString("X");
        }

        public override int GetHashCode() {
            return GUID.GetHashCode();
        }

        public bool Equals(teResourceGUID other) {
            return GUID == other.GUID;
        }

        public override bool Equals(object obj) {
            switch (obj) {
                case ulong guid:
                    return GUID == guid;
                case teResourceGUID resourceGUID:
                    return Equals(resourceGUID);
                default:
                    return base.Equals(obj);
            }
        }
    }
}