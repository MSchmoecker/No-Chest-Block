using System;
using ChestFix;
using HarmonyLib;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class InventoryHelperTest {
        [Test]
        public void MoveItemEmptyExceptOneItem() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            ItemDrop.ItemData item = Helper.CreateItem("my item", 5, 10);
            inventory.AddItem(item, 5, 3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, 5, new Vector2i(2, 2));
            Assert.True(success);
        }

        [Test]
        public void MoveItemNull() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            bool success = InventoryHelper.MoveItem(inventory, null, 5, new Vector2i(2, 2));
            Assert.False(success);
        }

        [Test]
        public void MoveItemOnEqualSame() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            ItemDrop.ItemData item = Helper.CreateItem("my item", 5, 10);
            inventory.AddItem(item, 5, 3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, 5, new Vector2i(3, 3));
            Assert.True(success);
        }

        [Test]
        public void MoveItemOnSameEnoughSpaceToStack() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item", 5, 15), 5, 2, 2);
            inventory.AddItem(Helper.CreateItem("my item", 5, 15), 5, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, 5, new Vector2i(2, 2));
            Assert.True(success);
            Assert.AreEqual(10, inventory.GetItemAt(2, 2).m_stack);
            Assert.Null(inventory.GetItemAt(3, 3));
        }

        [Test]
        public void MoveItemOnSameNotEnoughSpaceToStack() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item", 10, 15), 10, 2, 2);
            inventory.AddItem(Helper.CreateItem("my item", 10, 15), 10, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, item.m_stack, new Vector2i(2, 2));
            Assert.False(success); // is false for not completely moved good?
            Assert.AreEqual(15, inventory.GetItemAt(2, 2).m_stack);
            Assert.AreEqual(5, inventory.GetItemAt(3, 3).m_stack);
        }

        [Test]
        public void MoveItemSwitch() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item 1", 10, 15), 10, 2, 2);
            inventory.AddItem(Helper.CreateItem("my item 2", 10, 15), 10, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);
            
            bool success = InventoryHelper.MoveItem(inventory, item, item.m_stack, new Vector2i(2, 2));
            Assert.True(success);
            
            Assert.AreEqual("my item 2", inventory.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual("my item 1", inventory.GetItemAt(3, 3).m_shared.m_name);
        }
    }
}
