using System.Collections.Generic;
using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    public class GetAllMoveableItemsTest : ItemTestBase {
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

            TestForItems(from, new TestItem("my item 1", 10, new Vector2i(0, 0)));
            TestForItems(to, new TestItem("my item 2", 10, new Vector2i(0, 0)));
        }

        [Test]
        public void OriginalInventoryNotChanged_CanStack() {
            from.CreateItem("my item 2", 10, 0, 0);
            to.CreateItem("my item 2", 10, 0, 0);

            InventoryHelper.GetAllMoveableItems(from, to);

            TestForItems(from, new TestItem("my item 2", 10, new Vector2i(0, 0)));
            TestForItems(to, new TestItem("my item 2", 10, new Vector2i(0, 0)));
        }

        [Test]
        public void GetAllMoveableItems_EnoughSpace() {
            from.CreateItem("my item 1", 10, 0, 0);

            List<ItemDrop.ItemData> moved = InventoryHelper.GetAllMoveableItems(from, to);

            Assert.AreEqual(1, moved.Count);
            TestForItem(moved[0], new TestItem("my item 1", 10, new Vector2i(0, 0)));
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
            TestForItem(moved[0], new TestItem("my item 2", 8, new Vector2i(0, 0)));
        }
    }
}
