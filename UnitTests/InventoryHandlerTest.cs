using ChestFix;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class InventoryHandlerTest {
        [Test]
        public void RPC_RequestItemAddResponseExactAmount() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 3);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 3)); // inventoryPos
            data.Write(true); // is success
            data.Write(5); // amount
            data.Write(false); // not has switched
            data.SetPos(0);

            InventoryHandler.RPC_RequestItemAddResponse(inventory, 0L, data);

            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemAddResponseLowerAmount() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 3);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 3)); // inventoryPos
            data.Write(true); // is success
            data.Write(3); // amount
            data.Write(false); // not has switched
            data.SetPos(0);

            InventoryHandler.RPC_RequestItemAddResponse(inventory, 0L, data);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(2, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", inventory.m_inventory[0].m_shared.m_name);
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
