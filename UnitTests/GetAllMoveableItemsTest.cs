using System.Collections.Generic;
using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    public class GetAllMoveableItemsTest {
        private Inventory from;
        private Inventory to;

        [SetUp]
        public void Setup() {
            from = new Inventory("inventory", null, 2, 1);
            to = new Inventory("inventory", null, 2, 1);
        }

        [Test]
        public void OriginalInventoryNotChanged_DifferentItems() {
            from.CreateItem("my item 1", 10, 0, 0);
            to.CreateItem("my item 2", 10, 0, 0);

            InventoryHelper.GetAllMoveableItems(from, to);

            Assert.AreEqual(1, from.m_inventory.Count);
            Assert.AreEqual("my item 1", from.m_inventory[0].m_shared.m_name);
            Assert.AreEqual(10, from.m_inventory[0].m_stack);
            Assert.AreEqual(1, to.m_inventory.Count);
            Assert.AreEqual("my item 2", to.m_inventory[0].m_shared.m_name);
            Assert.AreEqual(10, to.m_inventory[0].m_stack);
        }

        [Test]
        public void OriginalInventoryNotChanged_CanStack() {
            from.CreateItem("my item 2", 10, 0, 0);
            to.CreateItem("my item 2", 10, 0, 0);

            InventoryHelper.GetAllMoveableItems(from, to);

            Assert.AreEqual(1, from.m_inventory.Count);
            Assert.AreEqual("my item 2", from.m_inventory[0].m_shared.m_name);
            Assert.AreEqual(10, from.m_inventory[0].m_stack);
            Assert.AreEqual(1, to.m_inventory.Count);
            Assert.AreEqual("my item 2", to.m_inventory[0].m_shared.m_name);
            Assert.AreEqual(10, to.m_inventory[0].m_stack);
        }

        [Test]
        public void GetAllMoveableItems_EnoughSpace() {
            from.CreateItem("my item 1", 10, 0, 0);

            List<ItemDrop.ItemData> moved = InventoryHelper.GetAllMoveableItems(from, to);

            Assert.AreEqual(1, moved.Count);
            Assert.AreEqual("my item 1", moved[0].m_shared.m_name);
        }

        [Test]
        public void GetAllMoveableItems_NotEnoughSpace() {
            from.CreateItem("my item 1", 10, 0, 0);
            to.CreateItem("my item 2", 10, 0, 0);
            to.CreateItem("my item 2", 10, 1, 0);

            List<ItemDrop.ItemData> moved = InventoryHelper.GetAllMoveableItems(from, to);

            Assert.AreEqual(0, moved.Count);
        }

        [Test]
        public void GetAllMoveableItems_CanStack() {
            from.CreateItem("my item 2", 15, 0, 0);
            to.CreateItem("my item 2", 16, 0, 0);
            to.CreateItem("my item 2", 16, 1, 0);

            List<ItemDrop.ItemData> moved = InventoryHelper.GetAllMoveableItems(from, to);

            Assert.AreEqual(1, moved.Count);
            Assert.AreEqual("my item 2", moved[0].m_shared.m_name);
            Assert.AreEqual(8, moved[0].m_stack);
        }
    }
}
