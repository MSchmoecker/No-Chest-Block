using NoChestBlock;
using HarmonyLib;
using NUnit.Framework;
using UnityEngine;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerRemoveTest {
        private Inventory container;

        [SetUp]
        public void SetUp() {
            container = new Inventory("inventory", null, 4, 5);
        }

        private RequestRemoveResponse GetResponse(RequestRemove request) {
            return container.RequestItemRemove(request);
        }

        private static RequestRemove MakeMessage(int dragAmount) {
            return new RequestRemove(new Vector2i(2, 2),
                                     new Vector2i(4, 4),
                                     dragAmount,
                                     "inv",
                                     null);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotExactAmountAsContainer() {
            container.AddItem(Helper.CreateItem("my item", 5, 10), 5, 2, 2);

            RequestRemove request = MakeMessage(5);
            RequestRemoveResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(0, container.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotFewerAmountAsContainer() {
            container.AddItem(Helper.CreateItem("my item", 5, 10), 5, 2, 2);

            RequestRemove request = MakeMessage(3);
            RequestRemoveResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(3, response.amount);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(2, container.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotMoreAmountAsContainer() {
            container.AddItem(Helper.CreateItem("my item", 5, 10), 5, 2, 2);

            RequestRemove request = MakeMessage(7);
            RequestRemoveResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(0, container.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotItemNotInContainer() {
            RequestRemove request = MakeMessage(5);
            RequestRemoveResponse response = GetResponse(request);

            Assert.False(response.success);
            Assert.AreEqual(0, response.amount);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(0, container.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveDifferentItemToInventory() {
            container.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 2);

            RequestRemove request = MakeMessage(5);
            request.switchItem = Helper.CreateItem("my item B", 3, 15);
            RequestRemoveResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.True(response.hasSwitched);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual("my item B", container.m_inventory[0].m_shared.m_name);
            Assert.AreEqual(3, container.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveDifferentItemToInventoryWithTrySplit() {
            container.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 2);

            RequestRemove request = MakeMessage(3);
            request.switchItem = Helper.CreateItem("my item B", 3, 15);
            RequestRemoveResponse response = GetResponse(request);

            Assert.False(response.success);
            Assert.AreEqual(0, response.amount);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual("my item A", container.m_inventory[0].m_shared.m_name);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemToInventoryCanStack() {
            container.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            RequestRemove request = MakeMessage(5);
            request.switchItem = Helper.CreateItem("my item A", 5, 20);
            RequestRemoveResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(0, container.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemNotAllToInventoryCanStack() {
            container.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            RequestRemove request = MakeMessage(3);
            request.switchItem = Helper.CreateItem("my item A", 5, 20);
            RequestRemoveResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(3, response.amount);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(2, container.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemToInventoryCanStackNotAll() {
            container.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            RequestRemove request = MakeMessage(5);
            request.switchItem = Helper.CreateItem("my item A", 19, 20);
            RequestRemoveResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(1, response.amount);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(4, container.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveGetItemDataBack() {
            container.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            RequestRemove request = MakeMessage(5);
            RequestRemoveResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(0, container.m_inventory.Count);
            Assert.AreEqual("my item A", response.responseItem.m_shared.m_name);
            Assert.AreEqual(5, response.responseItem.m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveOddSplitEvenContainer() {
            container.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            RequestRemove request = MakeMessage(3);
            RequestRemoveResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(3, response.amount);
            Assert.AreEqual(3, response.responseItem.m_stack);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(2, container.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveOddSplitOddContainer() {
            container.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            RequestRemove request = MakeMessage(2);
            RequestRemoveResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(2, response.amount);
            Assert.AreEqual(2, response.responseItem.m_stack);
            Assert.False(response.hasSwitched);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(3, container.m_inventory[0].m_stack);
        }
    }
}
