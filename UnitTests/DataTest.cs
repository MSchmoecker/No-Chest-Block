using System;
using System.Collections.Generic;
using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class DataTest : ItemTestBase {
        private Vector2i posA;
        private Vector2i posB;
        private ItemDrop.ItemData item;
        private ZDOID zdoid;

        [SetUp]
        public void Setup() {
            posA = new Vector2i(1, 2);
            posB = new Vector2i(2, 1);
            item = Helper.CreateItem("my item", 3);
            zdoid = new ZDOID(1234, 1);
        }

        private static T GetFromZPackage<T>(T request, Func<ZPackage, T> newRequest) where T : IPackage {
            ZPackage package = request.WriteToPackage();
            package.SetPos(0);
            return newRequest(package);
        }

        private static ItemDrop.ItemData GetFromZPackage(ItemDrop.ItemData item) {
            ZPackage package = new ZPackage();
            InventoryHelper.WriteItemToPackage(item, package);
            package.SetPos(0);
            return InventoryHelper.LoadItemFromPackage(package);
        }

        [Test]
        public void RequestAdd_PackageReadWrite() {
            RequestChestAdd requestChestAdd = new RequestChestAdd(posA, 4, item, "inv", true, zdoid);
            RequestChestAdd fromPackage = GetFromZPackage(requestChestAdd, package => new RequestChestAdd(package));

            Assert.AreEqual(fromPackage.toPos, posA);
            Assert.AreEqual(fromPackage.dragAmount, 4);
            TestForItem(fromPackage.dragItem, new TestItem("my item", 3, Vector2i.zero));
            Assert.AreEqual(fromPackage.fromInventoryHash, "inv".GetStableHashCode());
            Assert.True(fromPackage.allowSwitch);
            Assert.AreEqual(fromPackage.sender, zdoid);
        }

        [Test]
        public void RequestAddResponse_PackageReadWrite() {
            RequestChestAddResponse requestChestAdd = new RequestChestAddResponse(true, posA, 4, 5, item, zdoid);
            RequestChestAddResponse fromPackage = GetFromZPackage(requestChestAdd, package => new RequestChestAddResponse(package));

            Assert.True(fromPackage.Success);
            Assert.AreEqual(fromPackage.inventoryPos, posA);
            Assert.AreEqual(fromPackage.Amount, 4);
            Assert.AreEqual(fromPackage.inventoryHash, 5);
            TestForItem(fromPackage.switchItem, new TestItem("my item", 3, Vector2i.zero));
            Assert.AreEqual(fromPackage.sender, zdoid);
        }

        [Test]
        public void RequestConsume_PackageReadWrite() {
            item.m_gridPos = posA;
            RequestConsume requestAdd = new RequestConsume(item);
            RequestConsume fromPackage = GetFromZPackage(requestAdd, package => new RequestConsume(package));

            Assert.AreEqual(fromPackage.itemPosX, 1);
            Assert.AreEqual(fromPackage.itemPosY, 2);
        }

        [Test]
        public void RequestConsumeResponse_PackageReadWrite() {
            item.m_gridPos = posA;
            RequestConsumeResponse requestAdd = new RequestConsumeResponse(item, true, 3);
            RequestConsumeResponse fromPackage = GetFromZPackage(requestAdd, package => new RequestConsumeResponse(package));

            TestForItem(fromPackage.item, new TestItem("my item", 3, posA));
            Assert.True(fromPackage.Success);
            Assert.AreEqual(fromPackage.Amount, 3);
        }

        [Test]
        public void RequestDrop_PackageReadWrite() {
            RequestDrop requestAdd = new RequestDrop(posA, 3, zdoid);
            RequestDrop fromPackage = GetFromZPackage(requestAdd, package => new RequestDrop(package));

            Assert.AreEqual(fromPackage.targetContainerSlot, posA);
            Assert.AreEqual(fromPackage.amount, 3);
            Assert.AreEqual(fromPackage.sender, zdoid);
        }

        [Test]
        public void RequestDropResponse_PackageReadWrite() {
            item.m_gridPos = posA;
            RequestDropResponse requestAdd = new RequestDropResponse(item, zdoid, true, 7);
            RequestDropResponse fromPackage = GetFromZPackage(requestAdd, package => new RequestDropResponse(package));

            TestForItem(fromPackage.responseItem, new TestItem("my item", 3, posA));
            Assert.AreEqual(fromPackage.sender, zdoid);
            Assert.True(fromPackage.Success);
            Assert.AreEqual(fromPackage.Amount, 7);
        }

        [Test]
        public void RequestMove_PackageReadWrite() {
            RequestMove requestAdd = new RequestMove(posA, posB, 3);
            RequestMove fromPackage = GetFromZPackage(requestAdd, package => new RequestMove(package));

            Assert.AreEqual(fromPackage.fromPos, posA);
            Assert.AreEqual(fromPackage.toPos, posB);
            Assert.AreEqual(fromPackage.dragAmount, 3);
        }

        [Test]
        public void RequestRemove_PackageReadWrite() {
            RequestChestRemove requestChestAdd = new RequestChestRemove(posA, posB, 3, "inv", item, zdoid);
            RequestChestRemove fromPackage = GetFromZPackage(requestChestAdd, package => new RequestChestRemove(package));

            Assert.AreEqual(fromPackage.fromPos, posA);
            Assert.AreEqual(fromPackage.toPos, posB);
            Assert.AreEqual(fromPackage.dragAmount, 3);
            TestForItem(fromPackage.switchItem, new TestItem("my item", 3, Vector2i.zero));
            Assert.AreEqual(fromPackage.fromInventoryHash, "inv".GetStableHashCode());
            Assert.AreEqual(fromPackage.sender, zdoid);
        }

        [Test]
        public void RequestRemoveResponse_PackageReadWrite() {
            RequestChestRemoveResponse requestChestAdd = new RequestChestRemoveResponse(true, 3, false, posA, "inv".GetStableHashCode(), item, zdoid);
            RequestChestRemoveResponse fromPackage = GetFromZPackage(requestChestAdd, package => new RequestChestRemoveResponse(package));

            Assert.True(fromPackage.Success);
            Assert.AreEqual(fromPackage.Amount, 3);
            Assert.AreEqual(fromPackage.inventoryPos, posA);
            Assert.AreEqual(fromPackage.inventoryHash, "inv".GetStableHashCode());
            TestForItem(fromPackage.responseItem, new TestItem("my item", 3, Vector2i.zero));
            Assert.AreEqual(fromPackage.sender, zdoid);
            Assert.False(fromPackage.hasSwitched);
        }

        [Test]
        public void RequestTakeAll_PackageReadWrite() {
            RequestTakeAll requestAdd = new RequestTakeAll(new List<ItemDrop.ItemData> { item });
            RequestTakeAll fromPackage = GetFromZPackage(requestAdd, package => new RequestTakeAll(package));

            Assert.AreEqual(fromPackage.items.Count, 1);
            TestForItem(fromPackage.items[0], new TestItem("my item", 3, Vector2i.zero));
        }

        [Test]
        public void LoadItemPosition() {
            item.m_gridPos = new Vector2i(0, 0);
            ItemDrop.ItemData fromPackage = GetFromZPackage(item);

            Assert.AreEqual(fromPackage.m_gridPos, new Vector2i(0, 0));

            item.m_gridPos = new Vector2i(1, 2);
            fromPackage = GetFromZPackage(item);

            Assert.AreEqual(fromPackage.m_gridPos, new Vector2i(1, 2));
        }

        [Test]
        public void LoadItemNull() {
            ItemDrop.ItemData fromPackage = GetFromZPackage(null);
            Assert.AreEqual(fromPackage, null);
        }

        [Test]
        public void LoadItemData() {
            item.m_customData["testkey"] = "test";
            ItemDrop.ItemData fromPackage = GetFromZPackage(item);

            Assert.AreEqual(fromPackage.m_customData["testkey"], "test");
            Assert.AreEqual(fromPackage.m_customData, item.m_customData);
            Assert.AreNotSame(fromPackage.m_customData, item.m_customData);
        }
    }
}
