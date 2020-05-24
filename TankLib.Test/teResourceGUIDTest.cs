using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TankLib.Test {
    [TestClass]
    public class teResourceGUIDTest {
        [TestMethod]
        public void TestConstructor() {
            Assert.AreEqual(42UL, new teResourceGUID(42).GUID);
        }

        [TestMethod]
        public void TestEquality() {
            Assert.IsTrue(new teResourceGUID(42) == 42UL);
        }

        [TestMethod]
        public void TestGetAttribute1() {
            var GUID = new teResourceGUID(0x7666526211111111UL);
            Assert.AreEqual(0x11111111U, GUID.Index);
            Assert.AreEqual((byte) 0x2, GUID.Locale);
            Assert.AreEqual((byte) 0x3, GUID.Reserved);
            Assert.AreEqual((byte) 0x4, GUID.Region);
            Assert.AreEqual((byte) 0x5, GUID.Platform);
            Assert.AreEqual((ushort) 0x666, GUID.MangledType);
            Assert.AreEqual((byte) 0x7, GUID.Engine);
        }

        [TestMethod]
        public void TestGetAttribute2() {
            var GUID = new teResourceGUID(ulong.MaxValue);
            Assert.AreEqual(0xFFFFFFFFU, GUID.Index);
            Assert.AreEqual((byte) 0x1F, GUID.Locale);
            Assert.AreEqual((byte) 0x3, GUID.Reserved);
            Assert.AreEqual((byte) 0x1F, GUID.Region);
            Assert.AreEqual((byte) 0xF, GUID.Platform);
            Assert.AreEqual((ushort) 0xFFF, GUID.MangledType);
            Assert.AreEqual((byte) 0xF, GUID.Engine);
        }

        [TestMethod]
        public void TestSetAttribute1() {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var GUID = new teResourceGUID(0);
            GUID.Index = 0x11111111U;
            Assert.AreEqual(0x11111111U, GUID.Index);
            GUID.Locale = 0x2;
            Assert.AreEqual((byte) 0x2, GUID.Locale);
            GUID.Reserved = 0x3;
            Assert.AreEqual((byte) 0x3, GUID.Reserved);
            GUID.Region = 0x4;
            Assert.AreEqual((byte) 0x4, GUID.Region);
            GUID.Platform = 0x5;
            Assert.AreEqual((byte) 0x5, GUID.Platform);
            GUID.MangledType = 0x666;
            Assert.AreEqual((ushort) 0x666, GUID.MangledType);
            GUID.Engine = 0x7;
            Assert.AreEqual((byte) 0x7, GUID.Engine);
            Assert.AreEqual(0x7666526211111111UL, GUID.GUID);
        }

        [TestMethod]
        public void TestSetAttribute2() {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var GUID = new teResourceGUID(0);
            GUID.Index = uint.MaxValue;
            Assert.AreEqual(0xFFFFFFFFU, GUID.Index);
            GUID.Locale = 0x1F;
            Assert.AreEqual((byte) 0x1F, GUID.Locale);
            GUID.Reserved = 0x3;
            Assert.AreEqual((byte) 0x3, GUID.Reserved);
            GUID.Region = 0x1F;
            Assert.AreEqual((byte) 0x1F, GUID.Region);
            GUID.Platform = 0xF;
            Assert.AreEqual((byte) 0xF, GUID.Platform);
            GUID.MangledType = 0xFFF;
            Assert.AreEqual((ushort) 0xFFF, GUID.MangledType);
            GUID.Engine = 0xF;
            Assert.AreEqual((byte) 0xF, GUID.Engine);
            Assert.AreEqual(ulong.MaxValue, GUID.GUID);
        }

        [TestMethod]
        public void TestSetAttribute3() {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var GUID = new teResourceGUID(0);
            GUID.Index = uint.MaxValue;
            Assert.AreEqual(0xFFFFFFFFU, GUID.Index);
            GUID.Locale = 0xFF;
            Assert.AreEqual((byte) 0x1F, GUID.Locale);
            GUID.Reserved = 0xFF;
            Assert.AreEqual((byte) 0x3, GUID.Reserved);
            GUID.Region = 0xFF;
            Assert.AreEqual((byte) 0x1F, GUID.Region);
            GUID.Platform = 0xFF;
            Assert.AreEqual((byte) 0xF, GUID.Platform);
            GUID.MangledType = 0xFFFF;
            Assert.AreEqual((ushort) 0xFFF, GUID.MangledType);
            GUID.Engine = 0xFF;
            Assert.AreEqual((byte) 0xF, GUID.Engine);
            Assert.AreEqual(ulong.MaxValue, GUID.GUID);
        }

        [TestMethod]
        public void TestGetType() {
            var GUID = new teResourceGUID(0x0250000000000000);
            Assert.AreEqual((ushort) 0xA5, GUID.Type);
        }

        [TestMethod]
        public void TestSetType() {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var GUID = new teResourceGUID(0);
            GUID.Type = 0xA5;
            Assert.AreEqual((ushort) 0xA5, GUID.Type);
        }
    }
}
