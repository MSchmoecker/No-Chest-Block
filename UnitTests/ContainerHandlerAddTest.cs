using ChestFix;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerAddTest {
        [Test]
        public void RPC_RequestItemAddToEmptySlotExactAmountAsInventory() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 2)); // from inventory pos
            data.Write(new Vector2i(3, 3)); // to container pos
            data.Write(5); // drag amount
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item", 5, 20), data, true); // item to add
            data.Write(true); // allow switch
            data.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemAdd(0L, data);
            response.SetPos(0);

            Vector2i inventoryPos = response.ReadVector2i();
            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(5, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item", inventory.m_inventory[0].m_shared.m_name);
        }
        
        [Test]
        public void RPC_RequestItemAddToEmptySlotNotAllowSwitch() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 2)); // from inventory pos
            data.Write(new Vector2i(3, 3)); // to container pos
            data.Write(5); // drag amount
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item", 5, 20), data, true); // item to add
            data.Write(false); // not allow switch
            data.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemAdd(0L, data);
            response.SetPos(0);

            Vector2i inventoryPos = response.ReadVector2i();
            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(5, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item", inventory.m_inventory[0].m_shared.m_name);
        }


        [Test]
        public void RPC_RequestItemAddToEmptySlotMoreAmountAsInventory() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 2)); // from inventory pos
            data.Write(new Vector2i(3, 3)); // to container pos
            data.Write(5); // drag amount
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item", 3, 20), data, true); // item to add
            data.Write(true); // allow switch
            data.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemAdd(0L, data);
            response.SetPos(0);

            Vector2i inventoryPos = response.ReadVector2i();
            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(3, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(3, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item", inventory.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotFewerAmountAsInventory() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 2)); // from inventory pos
            data.Write(new Vector2i(3, 3)); // to container pos
            data.Write(3); // drag amount
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item", 5, 20), data, true); // item to add
            data.Write(true); // allow switch
            data.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemAdd(0L, data);
            response.SetPos(0);

            Vector2i inventoryPos = response.ReadVector2i();
            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(3, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(3, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item", inventory.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemSlotDragExact() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 3, 3);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 2)); // from inventory pos
            data.Write(new Vector2i(3, 3)); // to container pos
            data.Write(5); // drag amount
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item B", 5, 20), data, true); // item to add
            data.Write(true); // allow switch
            data.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemAdd(0L, data);
            response.SetPos(0);

            Vector2i inventoryPos = response.ReadVector2i();
            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();
            Inventory tmp = new Inventory("players inventory", null, 4, 5);
            InventoryHelper.LoadItemIntoInventory(response, tmp, new Vector2i(2, 2), -1, -1);

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.True(hasSwitched);

            Assert.AreEqual(1, tmp.m_inventory.Count);
            Assert.AreEqual(5, tmp.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", tmp.m_inventory[0].m_shared.m_name);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(5, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item B", inventory.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemSlotDragTooFew() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 3, 3);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 2)); // from inventory pos
            data.Write(new Vector2i(3, 3)); // to container pos
            data.Write(3); // drag amount
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item B", 5, 20), data, true); // item to add
            data.Write(true); // allow switch
            data.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemAdd(0L, data);
            response.SetPos(0);

            Vector2i inventoryPos = response.ReadVector2i();
            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.False(success);
            Assert.AreEqual(0, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(5, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", inventory.m_inventory[0].m_shared.m_name);
        }
        
        [Test]
        public void RPC_RequestItemAddToDifferentItemNotAllowSwitch() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 3, 3);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 2)); // from inventory pos
            data.Write(new Vector2i(3, 3)); // to container pos
            data.Write(5); // drag amount
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item B", 5, 20), data, true); // item to add
            data.Write(false); // not allow switch
            data.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemAdd(0L, data);
            response.SetPos(0);

            Vector2i inventoryPos = response.ReadVector2i();
            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.False(success);
            Assert.AreEqual(0, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(5, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", inventory.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToSameItemSlotDragCanStack() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 3, 3);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 2)); // from inventory pos
            data.Write(new Vector2i(3, 3)); // to container pos
            data.Write(5); // drag amount
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item A", 5, 20), data, true); // item to add
            data.Write(true); // allow switch
            data.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemAdd(0L, data);
            response.SetPos(0);

            Vector2i inventoryPos = response.ReadVector2i();
            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(10, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", inventory.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToSameItemSlotDragCanStackNotAll() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 19, 20), 19, 3, 3);

            ZPackage data = new ZPackage();
            data.Write(new Vector2i(2, 2)); // from inventory pos
            data.Write(new Vector2i(3, 3)); // to container pos
            data.Write(5); // drag amount
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item A", 5, 20), data, true); // item to add
            data.Write(true); // allow switch
            data.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemAdd(0L, data);
            response.SetPos(0);

            Vector2i inventoryPos = response.ReadVector2i();
            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(1, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(20, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", inventory.m_inventory[0].m_shared.m_name);
        }
    }
}
