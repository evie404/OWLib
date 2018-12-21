using System.Diagnostics;
using System.Linq;

namespace DataTool.Helper {
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class IndentHelper {
        protected static string IndentString         = " ";
        protected static uint   IndentStringPerLevel = 4;
        private readonly uint   _indentLevel;


        public IndentHelper(uint level) { _indentLevel = level; }

        public IndentHelper() { _indentLevel = 0; }

        public IndentHelper(IndentHelper obj) { _indentLevel = obj._indentLevel; }

        public IndentHelper(string existingValue) {           // create from existing string
            var find = GetIndentString(IndentStringPerLevel); // 1 indent
            var s2   = existingValue.Replace(find, "");       // how many times is 1 indent string in existing string
            _indentLevel = ((uint) existingValue.Length - (uint) s2.Length) / (uint) find.Length;

            // Debug.Assert(new IndentHelper(1).GetLevel() == new IndentHelper("    ").GetLevel());
            // Debug.Assert(new IndentHelper(2).GetLevel() == new IndentHelper("        ").GetLevel());
            // Debug.Assert(new IndentHelper(3).GetLevel() == new IndentHelper("            ").GetLevel());
            // Debug.Assert(new IndentHelper(4).GetLevel() == new IndentHelper("                ").GetLevel());
        }

        protected string DebuggerDisplay => $"Level: {GetLevel()}";

        protected string GetIndentString(int count) { return string.Concat(Enumerable.Repeat(IndentString, count)); }

        protected string GetIndentString(uint count) { return GetIndentString((int) count); }

        public override string ToString() { return GetIndentString(_indentLevel * IndentStringPerLevel); }

        public static IndentHelper operator +(IndentHelper c1, uint c2) { return new IndentHelper(c1._indentLevel + c2); }

        public static IndentHelper operator +(IndentHelper c1, IndentHelper c2) { return new IndentHelper(c1._indentLevel + c2._indentLevel); }

        public static IndentHelper operator -(IndentHelper c1, uint c2) { return new IndentHelper(c1._indentLevel - c2); }

        public static IndentHelper operator -(IndentHelper c1, IndentHelper c2) { return new IndentHelper(c1._indentLevel - c2._indentLevel); }

        public static IndentHelper operator *(IndentHelper c1, uint c2) { return new IndentHelper(c1._indentLevel * c2); }

        public static IndentHelper operator *(IndentHelper c1, IndentHelper c2) { return new IndentHelper(c1._indentLevel * c2._indentLevel); }

        public static IndentHelper operator /(IndentHelper c1, uint c2) { return new IndentHelper(c1._indentLevel / c2); }

        public static IndentHelper operator /(IndentHelper c1, IndentHelper c2) { return new IndentHelper(c1._indentLevel / c2._indentLevel); }

        public static implicit operator string(IndentHelper obj) { return obj.ToString(); }

        public uint GetLevel() { return _indentLevel; }
    }
}
