using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class InventoryHelperTest : ItemTestBase {
        private Inventory inventory;

        [SetUp]
        public void SetUp() {
            inventory = new Inventory("inventory", null, 4, 5);
        }

        [Test]
        public void MoveItemEmptyExceptOneItem() {
            inventory.CreateItem("my item", 5, 3, 3);

            bool success = InventoryHelper.MoveItem(inventory, inventory.GetItemAt(3, 3), 5, new Vector2i(2, 2), out int movedAmount);
            Assert.True(success);
            Assert.AreEqual(5, movedAmount);
        }

        [Test]
        public void MoveItemNull() {
            bool success = InventoryHelper.MoveItem(inventory, null, 5, new Vector2i(2, 2), out int movedAmount);
            Assert.False(success);
            Assert.AreEqual(0, movedAmount);
        }

        [Test]
        public void MoveItemOnEqualSame() {
            inventory.CreateItem("my item", 5, 3, 3);

            bool success = InventoryHelper.MoveItem(inventory, inventory.GetItemAt(3, 3), 5, new Vector2i(3, 3), out int movedAmount);
            Assert.True(success);
            Assert.AreEqual(0, movedAmount);
        }

        [Test]
        public void MoveItemOnSameEnoughSpaceToStack() {
            inventory.CreateItem("my item", 5, 2, 2);
            inventory.CreateItem("my item", 5, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, 5, new Vector2i(2, 2), out int movedAmount);
            Assert.True(success);
            Assert.AreEqual(5, movedAmount);
            TestForItems(inventory, new TestItem("my item", 10, new Vector2i(2, 2)));
        }

        [Test]
        public void MoveItemOnSameNotEnoughSpaceToStack() {
            inventory.CreateItem("my item", 15, 2, 2);
            inventory.CreateItem("my item", 10, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, item.m_stack, new Vector2i(2, 2), out int movedAmount);
            Assert.True(success);
            Assert.AreEqual(5, movedAmount);

            TestForItems(inventory, new[] {
                new TestItem("my item", 20, new Vector2i(2, 2)),
                new TestItem("my item", 5, new Vector2i(3, 3)),
            });
        }

        [Test]
        public void MoveItemOnSameNoSpaceToStack() {
            inventory.CreateItem("my item", 20, 2, 2);
            inventory.CreateItem("my item", 10, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, item.m_stack, new Vector2i(2, 2), out int movedAmount);
            Assert.False(success);
            Assert.AreEqual(0, movedAmount);

            TestForItems(inventory, new[] {
                new TestItem("my item", 20, new Vector2i(2, 2)),
                new TestItem("my item", 10, new Vector2i(3, 3)),
            });
        }

        [Test]
        public void MoveItemSwitch() {
            inventory.CreateItem("my item 1", 12, 2, 2);
            inventory.CreateItem("my item 2", 10, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, item.m_stack, new Vector2i(2, 2), out int movedAmount);
            Assert.True(success);
            Assert.AreEqual(10, movedAmount);

            TestForItems(inventory, new[] {
                new TestItem("my item 1", 12, new Vector2i(3, 3)),
                new TestItem("my item 2", 10, new Vector2i(2, 2)),
            });
        }

        [Test]
        public void MoveItemSplitCannotSwitch() {
            inventory.CreateItem("my item 1", 12, 2, 2);
            inventory.CreateItem("my item 2", 10, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, 5, new Vector2i(2, 2), out int movedAmount);
            Assert.False(success);
            Assert.AreEqual(0, movedAmount);

            TestForItems(inventory, new[] {
                new TestItem("my item 1", 12, new Vector2i(2, 2)),
                new TestItem("my item 2", 10, new Vector2i(3, 3)),
            });
        }

        [Test]
        public void AddItem_FreeSlot() {
            ItemDrop.ItemData item = Helper.CreateItem("itemA", 5);
            bool success = inventory.AddItemToInventory(item, 5, new Vector2i(2, 2));

            Assert.True(success);
            TestForItems(inventory, new TestItem("itemA", 5, new Vector2i(2, 2)));
        }

        [Test]
        public void AddItem_Stack() {
            inventory.CreateItem("itemA", 5, 2, 2);
            ItemDrop.ItemData item = Helper.CreateItem("itemA", 5);
            bool success = inventory.AddItemToInventory(item, 5, new Vector2i(2, 2));

            Assert.True(success);
            TestForItems(inventory, new TestItem("itemA", 10, new Vector2i(2, 2)));
        }

        [Test]
        public void AddItem_ExactStack() {
            inventory.CreateItem("itemA", 15, 2, 2);
            ItemDrop.ItemData item = Helper.CreateItem("itemA", 5);
            bool success = inventory.AddItemToInventory(item, 5, new Vector2i(2, 2));

            Assert.True(success);
            TestForItems(inventory, new TestItem("itemA", 20, new Vector2i(2, 2)));
        }

        [Test]
        public void AddItem_DifferentStack() {
            inventory.CreateItem("itemA", 5, 2, 2);
            ItemDrop.ItemData item = Helper.CreateItem("itemB", 5);
            bool success = inventory.AddItemToInventory(item, 5, new Vector2i(2, 2));

            Assert.False(success);
            TestForItems(inventory, new TestItem("itemA", 5, new Vector2i(2, 2)));
        }

        [Test]
        public void AddItem_NoSpace() {
            inventory.CreateItem("itemA", 20, 2, 2);
            ItemDrop.ItemData item = Helper.CreateItem("itemA", 5);
            bool success = inventory.AddItemToInventory(item, 5, new Vector2i(2, 2));

            Assert.False(success);
            TestForItems(inventory, new TestItem("itemA", 20, new Vector2i(2, 2)));
        }

        [Test]
        public void AddItem_NotEnoughSpace() {
            inventory.CreateItem("itemA", 17, 2, 2);
            ItemDrop.ItemData item = Helper.CreateItem("itemA", 5);
            bool success = inventory.AddItemToInventory(item, 5, new Vector2i(2, 2));

            Assert.False(success);
            TestForItems(inventory, new TestItem("itemA", 17, new Vector2i(2, 2)));
        }

        [Test]
        public void AddItem_IsCloned() {
            ItemDrop.ItemData item = Helper.CreateItem("itemA", 5);
            inventory.AddItemToInventory(item, 3, new Vector2i(2, 2));

            Assert.AreNotSame(item, inventory.GetItemAt(2, 2));
        }

        [Test]
        public void AddItem_IsNotModified() {
            ItemDrop.ItemData item = Helper.CreateItem("itemA", 5);
            inventory.AddItemToInventory(item, 3, new Vector2i(2, 2));

            Assert.AreEqual(5, item.m_stack);
        }

        [Test]
        public void CanStack_SameItem() {
            ItemDrop.ItemData itemA = Helper.CreateItem("itemA", 5);
            ItemDrop.ItemData itemB = Helper.CreateItem("itemA", 5);

            Assert.True(InventoryHelper.CanStack(itemA, itemB));
        }

        [Test]
        public void CanStack_SameItem_Tool() {
            ItemDrop.ItemData itemA = Helper.CreateItem("itemA", 1);
            itemA.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Tool;
            itemA.m_shared.m_maxStackSize = 1;

            ItemDrop.ItemData itemB = Helper.CreateItem("itemA", 1);
            itemB.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Material;
            itemB.m_shared.m_maxStackSize = 1;

            Assert.False(InventoryHelper.CanStack(itemA, itemB));
        }

        [Test]
        public void CanStack_DifferentItem_Name() {
            ItemDrop.ItemData itemA = Helper.CreateItem("itemA", 5);
            ItemDrop.ItemData itemB = Helper.CreateItem("itemB", 5);

            Assert.False(InventoryHelper.CanStack(itemA, itemB));
        }

        [Test]
        public void CanStack_DifferentItem_Quality() {
            ItemDrop.ItemData itemA = Helper.CreateItem("itemA", 1);
            itemA.m_shared.m_maxQuality = 3;
            itemA.m_quality = 1;
            ItemDrop.ItemData itemB = Helper.CreateItem("itemA", 1);
            itemB.m_shared.m_maxQuality = 3;
            itemB.m_quality = 2;

            Assert.False(InventoryHelper.CanStack(itemA, itemB));
        }
    }
}
