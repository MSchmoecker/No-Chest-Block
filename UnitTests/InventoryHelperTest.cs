using System;
using NoChestBlock;
using HarmonyLib;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class InventoryHelperTest {
        private Inventory inventory;

        [SetUp]
        public void SetUp() {
            inventory = new Inventory("inventory", null, 4, 5);
        }

        [Test]
        public void MoveItemEmptyExceptOneItem() {
            inventory.CreateItem("my item", 5, 3, 3);

            bool success = InventoryHelper.MoveItem(inventory, inventory.GetItemAt(3, 3), 5, new Vector2i(2, 2));
            Assert.True(success);
        }

        [Test]
        public void MoveItemNull() {
            bool success = InventoryHelper.MoveItem(inventory, null, 5, new Vector2i(2, 2));
            Assert.False(success);
        }

        [Test]
        public void MoveItemOnEqualSame() {
            inventory.CreateItem("my item", 5, 3, 3);

            bool success = InventoryHelper.MoveItem(inventory, inventory.GetItemAt(3, 3), 5, new Vector2i(3, 3));
            Assert.True(success);
        }

        [Test]
        public void MoveItemOnSameEnoughSpaceToStack() {
            inventory.CreateItem("my item", 5, 2, 2);
            inventory.CreateItem("my item", 5, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, 5, new Vector2i(2, 2));
            Assert.True(success);
            Assert.AreEqual(10, inventory.GetItemAt(2, 2).m_stack);
            Assert.Null(inventory.GetItemAt(3, 3));
        }

        [Test]
        public void MoveItemOnSameNotEnoughSpaceToStack() {
            inventory.CreateItem("my item", 15, 2, 2);
            inventory.CreateItem("my item", 10, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, item.m_stack, new Vector2i(2, 2));
            Assert.False(success); // TODO is false for not completely moved good?
            Assert.AreEqual(20, inventory.GetItemAt(2, 2).m_stack);
            Assert.AreEqual(5, inventory.GetItemAt(3, 3).m_stack);
        }

        [Test]
        public void MoveItemSwitch() {
            inventory.CreateItem("my item 1", 10, 2, 2);
            inventory.CreateItem("my item 2", 10, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, item.m_stack, new Vector2i(2, 2));
            Assert.True(success);

            Assert.AreEqual("my item 2", inventory.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual("my item 1", inventory.GetItemAt(3, 3).m_shared.m_name);
        }

        [Test]
        public void MoveItemSplitCannotSwitch() {
            inventory.CreateItem("my item 1", 10, 2, 2);
            inventory.CreateItem("my item 2", 10, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, 5, new Vector2i(2, 2));
            Assert.False(success);

            Assert.AreEqual("my item 1", inventory.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(10, inventory.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("my item 2", inventory.GetItemAt(3, 3).m_shared.m_name);
            Assert.AreEqual(10, inventory.GetItemAt(3, 3).m_stack);
        }
    }
}
