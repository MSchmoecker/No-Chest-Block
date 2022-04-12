using System;
using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class DataTest : ItemTestBase {
        private Vector2i pos;
        private ItemDrop.ItemData item;

        [SetUp]
        public void Setup() {
            pos = new Vector2i(1, 2);
            item = Helper.CreateItem("my item", 3);
        }

        private static T GetFromZPackage<T>(T request, Func<ZPackage, T> newRequest) where T : IPackage {
            ZPackage package = request.WriteToPackage();
            package.SetPos(0);
            return newRequest(package);
        }

        [Test]
        public void RequestAdd_PackageReadWrite() {
            RequestAdd requestAdd = new RequestAdd(pos, 4, item, "inv", true, ZDOID.None);
            RequestAdd fromPackage = GetFromZPackage(requestAdd, package => new RequestAdd(package));

            Assert.AreEqual(fromPackage.toPos, pos);
            Assert.AreEqual(fromPackage.dragAmount, 4);
            TestForItem(fromPackage.dragItem, new TestItem("my item", 3, Vector2i.zero));
            Assert.AreEqual(fromPackage.fromInventoryHash, "inv".GetStableHashCode());
            Assert.True(fromPackage.allowSwitch);
            Assert.AreEqual(fromPackage.sender, ZDOID.None);
        }

        [Test]
        public void RequestAddResponse_PackageReadWrite() {
            RequestAddResponse requestAdd = new RequestAddResponse(true, pos, 4, 5, item, ZDOID.None);
            RequestAddResponse fromPackage = GetFromZPackage(requestAdd, package => new RequestAddResponse(package));

            Assert.True(fromPackage.Success);
            Assert.AreEqual(fromPackage.inventoryPos, pos);
            Assert.AreEqual(fromPackage.Amount, 4);
            Assert.AreEqual(fromPackage.inventoryHash, 5);
            TestForItem(fromPackage.switchItem, new TestItem("my item", 3, Vector2i.zero));
            Assert.AreEqual(fromPackage.sender, ZDOID.None);
        }
    }
}
