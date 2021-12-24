using ChestFix;
using ChestFix.Patches;
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

            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(2,3)));

            ContainerPatch.AddItemToChest(new RequestItemAdd(new Vector2i(2, 3), new Vector2i(1, 1), 5, inventory.GetItemAt(2, 3), true),
                inventory, container);

            Assert.True(InventoryHandler.IsSlotBlocked(new Vector2i(2,3)));
            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void AddItemToChestLowerAmount() {
            Container container = Helper.CreateContainer();
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 3);

            Assert.False(InventoryHandler.IsSlotBlocked(new Vector2i(2,3)));

            ContainerPatch.AddItemToChest(new RequestItemAdd(new Vector2i(2, 3), new Vector2i(1, 1), 3, inventory.GetItemAt(2, 3), true),
                inventory, container);

            Assert.True(InventoryHandler.IsSlotBlocked(new Vector2i(2,3)));
            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(2, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemAddResponseNoItemAtResponsePos() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 3)); // inventoryPos
            data.Write(true); // is success
            data.Write(3); // amount
            data.Write(false); // not has switched
            data.SetPos(0);

            Assert.DoesNotThrow(() => InventoryHandler.RPC_RequestItemAddResponse(inventory, 0L, data));
        }
    }
}
