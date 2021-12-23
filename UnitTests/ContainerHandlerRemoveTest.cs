using ChestFix;
using HarmonyLib;
using NUnit.Framework;
using UnityEngine;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerRemoveTest {
        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotExactAmountAsContainer() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item", 5, 10), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(false); // no item
            package.SetPos(0);

            ZPackage response = inventory.RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotFewerAmountAsContainer() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item", 5, 10), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(3); // drag amount
            package.Write(false); // no item
            package.SetPos(0);

            ZPackage response = inventory.RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(3, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(2, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotMoreAmountAsContainer() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item", 5, 10), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(7); // drag amount
            package.Write(false); // no item
            package.SetPos(0);

            ZPackage response = inventory.RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotItemNotInContainer() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(false); // no item
            package.SetPos(0);

            ZPackage response = inventory.RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.False(success);
            Assert.AreEqual(0, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveDifferentItemToInventory() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(true); // switch with item
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item B", 3, 15), package, true); //item to switch

            package.SetPos(0);

            ZPackage response = inventory.RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.True(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual("my item B", inventory.m_inventory[0].m_shared.m_name);
            Assert.AreEqual(3, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveDifferentItemToInventoryWithTrySplit() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(3); // drag amount
            package.Write(true); // switch with item
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item B", 3, 15), package, true); //item to switch

            package.SetPos(0);

            ZPackage response = inventory.RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.False(success);
            Assert.AreEqual(0, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual("my item A", inventory.m_inventory[0].m_shared.m_name);
            Assert.AreEqual(5, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemToInventoryCanStack() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(true); // switch with item
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item A", 5, 20), package, true); //item to stack

            package.SetPos(0);

            ZPackage response = inventory.RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemNotAllToInventoryCanStack() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(3); // drag amount
            package.Write(true); // switch with item
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item A", 5, 20), package, true); //item to stack

            package.SetPos(0);

            ZPackage response = inventory.RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(3, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(2, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemToInventoryCanStackNotAll() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(true); // switch with item
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item A", 19, 20), package, true); //item to stack

            package.SetPos(0);

            ZPackage response = inventory.RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(1, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(4, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveGetItemDataBack() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(false); // switch with item

            package.SetPos(0);

            ZPackage response = inventory.RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();
            Vector2i inventoryPos = response.ReadVector2i();
            bool hasResponseItem = response.ReadBool();
            ItemDrop.ItemData returnedItem = InventoryHelper.LoadItemFromPackage(response, inventoryPos);

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(0, inventory.m_inventory.Count);
            Assert.AreEqual("my item A", returnedItem.m_shared.m_name);
            Assert.AreEqual(5, returnedItem.m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveOddSplitEvenContainer() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            ZPackage request = new RequestItemRemove(new Vector2i(2, 2), new Vector2i(4, 4), 3, null).WriteToPackage();
            request.SetPos(0);

            ZPackage responseData = inventory.RequestItemRemove(0L, request);
            responseData.SetPos(0);
            RequestRemoveResponse response = new RequestRemoveResponse(responseData);

            Assert.True(response.success);
            Assert.AreEqual(3, response.amount);
            Assert.AreEqual(3, response.responseItem.m_stack);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(2, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveOddSplitOddContainer() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            ZPackage request = new RequestItemRemove(new Vector2i(2, 2), new Vector2i(4, 4), 2, null).WriteToPackage();
            request.SetPos(0);

            ZPackage responseData = inventory.RequestItemRemove(0L, request);
            responseData.SetPos(0);
            RequestRemoveResponse response = new RequestRemoveResponse(responseData);

            Assert.True(response.success);
            Assert.AreEqual(2, response.amount);
            Assert.AreEqual(2, response.responseItem.m_stack);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(3, inventory.m_inventory[0].m_stack);
        }
    }
}
