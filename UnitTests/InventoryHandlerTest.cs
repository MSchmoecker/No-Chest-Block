using MultiUserChest;
using MultiUserChest.Patches;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class InventoryHandlerTest : ItemTestBase {
        private Inventory inventory;

        [SetUp]
        public void SetUp() {
            inventory = new Inventory("inventory", null, 4, 5);
        }

        [Test]
        public void AddItemToChestExactAmount() {
            Container container = Helper.CreateContainer();
            inventory.CreateItem("my item A", 5, 2, 3);

            Assert.False(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 3)));

            container.AddItemToChest(inventory.GetItemAt(2, 3), inventory, new Vector2i(1, 1), ZDOID.None, 5);

            Assert.True(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 3)));
            TestForItems(inventory);
        }

        [Test]
        public void AddItemToChestLowerAmount() {
            Container container = Helper.CreateContainer();
            inventory.CreateItem("my item A", 5, 2, 3);

            Assert.False(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 3)));

            container.AddItemToChest(inventory.GetItemAt(2, 3), inventory, new Vector2i(1, 1), ZDOID.None, 3);

            Assert.True(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 3)));
            TestForItems(inventory, new TestItem("my item A", 2, new Vector2i(2, 3)));
        }

        [Test]
        public void RPC_RequestItemAddResponseNoItemAtResponsePos() {
            RequestChestAddResponse response = new RequestChestAddResponse(true, new Vector2i(2, 3), 3, "inv".GetStableHashCode(), null, ZDOID.None);
            Assert.DoesNotThrow(() => InventoryHandler.RPC_RequestItemAddResponse(inventory, response));
        }

        [Test]
        public void SlotIsBlocked() {
            Assert.False(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 4)));
            InventoryBlock.Get(inventory).BlockSlot(new Vector2i(2, 4));
            Assert.True(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 4)));
        }

        [Test]
        public void SlotIsNotBlocked() {
            Assert.False(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 4)));
            InventoryBlock.Get(inventory).BlockSlot(new Vector2i(2, 4));
            Assert.False(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(4, 2)));
        }

        [Test]
        public void SlotIsReleased() {
            Assert.False(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 4)));
            InventoryBlock.Get(inventory).BlockSlot(new Vector2i(2, 4));
            Assert.True(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 4)));
            InventoryBlock.Get(inventory).ReleaseSlot(new Vector2i(2, 4));
            Assert.False(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 4)));
        }

        [Test]
        public void SlotTwoBlock() {
            Assert.False(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 4)));
            InventoryBlock.Get(inventory).BlockSlot(new Vector2i(2, 4));
            InventoryBlock.Get(inventory).BlockSlot(new Vector2i(2, 4));
            Assert.True(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 4)));
            InventoryBlock.Get(inventory).ReleaseSlot(new Vector2i(2, 4));
            Assert.True(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 4)));
            InventoryBlock.Get(inventory).ReleaseSlot(new Vector2i(2, 4));
            Assert.False(InventoryBlock.Get(inventory).IsSlotBlocked(new Vector2i(2, 4)));
        }
    }
}
