using NoChestBlock;
using NoChestBlock.Patches;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class InventoryHandlerTest {
        [SetUp]
        public void SetUp() {
            InventoryHandler.blockedSlots.Clear();
        }

        [Test]
        public void AddItemToChestExactAmount() {
            Container container = Helper.CreateContainer();
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 3);

            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(2, 3)));

            ContainerHandler.AddItemToChest(container, inventory, ZDOID.None, new Vector2i(2, 3), new Vector2i(1, 1), 5, true);

            Assert.True(InventoryHandler.IsSlotBlocked(new Vector2i(2, 3)));
            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void AddItemToChestLowerAmount() {
            Container container = Helper.CreateContainer();
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 3);

            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(2, 3)));

            ContainerHandler.AddItemToChest(container, inventory, ZDOID.None,new Vector2i(2, 3), new Vector2i(1, 1), 3, true);

            Assert.True(InventoryHandler.IsSlotBlocked(new Vector2i(2, 3)));
            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(2, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemAddResponseNoItemAtResponsePos() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);

            RequestAddResponse response = new RequestAddResponse(true, new Vector2i(2, 3), 3, "inv".GetStableHashCode(), null, ZDOID.None);
            Assert.DoesNotThrow(() => InventoryHandler.RPC_RequestItemAddResponse(inventory, response));
        }

        [Test]
        public void SlotIsBlocked() {
            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(2, 4)));
            InventoryHandler.BlockSlot(new Vector2i(2, 4));
            Assert.True(InventoryHandler.IsSlotBlocked(new Vector2i(2, 4)));
        }

        [Test]
        public void SlotIsNotBlocked() {
            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(2, 4)));
            InventoryHandler.BlockSlot(new Vector2i(2, 4));
            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(4, 2)));
        }

        [Test]
        public void SlotIsReleased() {
            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(2, 4)));
            InventoryHandler.BlockSlot(new Vector2i(2, 4));
            Assert.True(InventoryHandler.IsSlotBlocked(new Vector2i(2, 4)));
            InventoryHandler.ReleaseSlot(new Vector2i(2, 4));
            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(2, 4)));
        }

        [Test]
        public void SlotTwoBlockOneReleased() {
            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(2, 4)));
            InventoryHandler.BlockSlot(new Vector2i(2, 4));
            InventoryHandler.BlockSlot(new Vector2i(2, 4));
            Assert.True(InventoryHandler.IsSlotBlocked(new Vector2i(2, 4)));
            InventoryHandler.ReleaseSlot(new Vector2i(2, 4));
            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(2, 4)));
        }
    }
}
