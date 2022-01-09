using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerAddTest {
        [Test]
        public void RPC_RequestItemAddToEmptySlotExactAmountAsInventory() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item", 5, 20),
                                                true);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = inventory.RequestItemAdd(0L, data);
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

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item", 5, 20),
                                                false);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = inventory.RequestItemAdd(0L, data);
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

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item", 3, 20),
                                                true);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = inventory.RequestItemAdd(0L, data);
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

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                3,
                                                Helper.CreateItem("my item", 5, 20),
                                                true);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = inventory.RequestItemAdd(0L, data);
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

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item B", 5, 20),
                                                true);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = inventory.RequestItemAdd(0L, data);
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

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                3,
                                                Helper.CreateItem("my item B", 5, 20),
                                                true);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = inventory.RequestItemAdd(0L, data);
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

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item B", 5, 20),
                                                false);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = inventory.RequestItemAdd(0L, data);
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

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item A", 5, 20),
                                                true);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = inventory.RequestItemAdd(0L, data);
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

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item A", 5, 20),
                                                true);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = inventory.RequestItemAdd(0L, data);
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

        [Test]
        public void RPC_RequestItemFullSlot() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 20, 20), 20, 3, 3);

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item A", 5, 20),
                                                true);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = inventory.RequestItemAdd(0L, data);
            response.SetPos(0);

            Vector2i inventoryPos = response.ReadVector2i();
            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.False(success);
            Assert.AreEqual(0, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(20, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", inventory.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemSwitched() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 20, 20), 20, 3, 3);

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item B", 5, 20),
                                                true);
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage responsePackage = inventory.RequestItemAdd(0L, data);
            responsePackage.SetPos(0);

            RequestAddResponse response = new RequestAddResponse(responsePackage);

            bool success = response.success;
            int addedAmount = response.amount;
            bool hasSwitched = response.switchItem != null;

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.True(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(5, inventory.m_inventory[0].m_stack);
            Assert.AreEqual("my item B", inventory.m_inventory[0].m_shared.m_name);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(20, response.switchItem.m_stack);
            Assert.AreEqual("my item A", response.switchItem.m_shared.m_name);
        }
    }
}
