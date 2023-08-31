using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class SlotPreviewTest : ItemTestBase {
        private SlotPreview preview;
        private Inventory inventory;

        [SetUp]
        public void SetUp() {
            inventory = new Inventory("inventory", null, 4, 5);
            preview = new SlotPreview(inventory);
        }

        [Test]
        public void OriginalItemIsNotModified() {
            ItemDrop.ItemData original = Helper.CreateItem("item A", 10);

            preview.Add(new Vector2i(2, 3), original);
            preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData _);

            TestForItem(original, new TestItem("item A", 10, new Vector2i(0, 0)));
        }

        [Test]
        public void NoPreviewAdded_NoItem() {
            bool changed = preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData item);

            Assert.IsFalse(changed);
            TestForItem(item, null);
        }

        [Test]
        public void NoPreviewAdded_ItemExists() {
            inventory.CreateItem("item A", 10, 2, 3);
            preview = new SlotPreview(inventory);

            bool changed = preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData item);

            Assert.IsFalse(changed, "changed");
            TestForItem(item, new TestItem("item A", 10, new Vector2i(2, 3)));
        }

        [Test]
        public void AddItemOnEmptySlot() {
            preview.Add(new Vector2i(2, 3), Helper.CreateItem("item A", 10));
            bool changed = preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData item);

            Assert.IsTrue(changed);
            TestForItem(item, new TestItem("item A", 10, new Vector2i(2, 3)));
        }

        [Test]
        public void AddItemOnMergeableSlot() {
            inventory.CreateItem("item A", 10, 2, 3);
            preview = new SlotPreview(inventory);

            preview.Add(new Vector2i(2, 3), Helper.CreateItem("item A", 5));

            bool changed = preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData item);
            Assert.IsTrue(changed);
            TestForItem(item, new TestItem("item A", 15, new Vector2i(2, 3)));
        }

        [Test]
        public void RemoveSameItem_Partially() {
            inventory.CreateItem("item A", 10, 2, 3);
            preview = new SlotPreview(inventory);

            preview.Remove(new Vector2i(2, 3), Helper.CreateItem("item A", 5));

            bool changed = preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData item);
            Assert.IsTrue(changed);
            TestForItem(item, new TestItem("item A", 5, new Vector2i(2, 3)));
        }

        [Test]
        public void RemoveSameItem_Completely() {
            inventory.CreateItem("item A", 10, 2, 3);
            preview = new SlotPreview(inventory);

            preview.Remove(new Vector2i(2, 3), Helper.CreateItem("item A", 10));

            bool changed = preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData item);
            Assert.IsTrue(changed);
            TestForItem(item, null);
        }

        [Test]
        public void RemoveItem_TwiceTooMuch_FirstFull() {
            inventory.CreateItem("item A", 10, 2, 3);
            preview = new SlotPreview(inventory);

            preview.Remove(new Vector2i(2, 3), Helper.CreateItem("item A", 10));
            preview.Remove(new Vector2i(2, 3), Helper.CreateItem("item A", 10));

            bool changed = preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData item);
            Assert.IsTrue(changed);
            TestForItem(item, null);
        }

        [Test]
        public void RemoveItem_TwiceTooMuch_FirstPartial() {
            inventory.CreateItem("item A", 10, 2, 3);
            preview = new SlotPreview(inventory);

            preview.Remove(new Vector2i(2, 3), Helper.CreateItem("item A", 7));
            preview.Remove(new Vector2i(2, 3), Helper.CreateItem("item A", 5));

            bool changed = preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData item);
            Assert.IsTrue(changed);
            TestForItem(item, null);
        }

        [Test]
        public void AddItem_FastMove() {
            preview.Add(new Vector2i(-1, -1), Helper.CreateItem("item A", 10));
            bool changed = preview.GetSlot(new Vector2i(0, 0), out ItemDrop.ItemData item);

            Assert.IsTrue(changed, "changed");
            TestForItem(item, new TestItem("item A", 10, new Vector2i(0, 0)));
        }

        [Test]
        public void AddItem_UnrelatedNotChanged() {
            inventory.CreateItem("item A", 10, 2, 3);
            preview = new SlotPreview(inventory);
            preview.Add(new Vector2i(3, 3), Helper.CreateItem("item B", 5));

            Assert.IsFalse(preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData item));
        }

        [Test]
        public void AddItem_EquippedNotChanged() {
            ItemDrop.ItemData tool = inventory.CreateItem("item A", 1, 2, 3);
            tool.m_shared.m_maxStackSize = 1;
            tool.m_equipped = true;

            preview = new SlotPreview(inventory);
            preview.Add(new Vector2i(3, 3), Helper.CreateItem("item B", 5));

            bool changed = preview.GetSlot(new Vector2i(2, 3), out ItemDrop.ItemData item);
            Assert.IsFalse(changed);
            TestForItem(item, new TestItem("item A", 1, new Vector2i(2, 3)));
        }
    }
}
