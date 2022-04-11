using NoChestBlock;
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
            TestForItems(inventory, new TestItem("my item", 10, new Vector2i(2, 2)));
        }

        [Test]
        public void MoveItemOnSameNotEnoughSpaceToStack() {
            inventory.CreateItem("my item", 15, 2, 2);
            inventory.CreateItem("my item", 10, 3, 3);

            ItemDrop.ItemData item = inventory.GetItemAt(3, 3);

            bool success = InventoryHelper.MoveItem(inventory, item, item.m_stack, new Vector2i(2, 2));
            Assert.True(success);

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

            bool success = InventoryHelper.MoveItem(inventory, item, item.m_stack, new Vector2i(2, 2));
            Assert.False(success);

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

            bool success = InventoryHelper.MoveItem(inventory, item, item.m_stack, new Vector2i(2, 2));
            Assert.True(success);

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

            bool success = InventoryHelper.MoveItem(inventory, item, 5, new Vector2i(2, 2));
            Assert.False(success);

            TestForItems(inventory, new[] {
                new TestItem("my item 1", 12, new Vector2i(2, 2)),
                new TestItem("my item 2", 10, new Vector2i(3, 3)),
            });
        }
    }
}
