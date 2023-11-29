using MultiUserChest;
using NUnit.Framework;
using UnityEngine;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerRemoveTest : ItemTestBase {
        private Inventory container;
        private Inventory groundInv;

        [SetUp]
        public void SetUp() {
            container = new Inventory("inventory", null, 4, 5);
            groundInv = new Inventory("ground", null, 5, 5);
            Patches.DropPatch.OnDrop += AddItemToGround;
        }

        [TearDown]
        public void TearDown() {
            Patches.DropPatch.OnDrop -= AddItemToGround;
        }

        private void AddItemToGround(ItemDrop.ItemData item, int amount) {
            bool preventAddItem = Patches.PreventAddItem.Enabled;

            if (preventAddItem) {
                Patches.PreventAddItem.Disable();
            }

            if (item != null) {
                ItemDrop.ItemData clone = item.Clone();
                clone.m_stack = amount;
                groundInv.AddItem(clone);
            }

            if (preventAddItem) {
                Patches.PreventAddItem.Enable();
            }
        }

        private RequestChestRemoveResponse GetResponse(RequestChestRemove request) {
            return container.RequestItemRemove(request);
        }

        private RequestChestRemove MakeMessage(int dragAmount, ItemDrop.ItemData switchItem = null) {
            return new RequestChestRemove(new Vector2i(2, 2),
                                          new Vector2i(4, 4),
                                          dragAmount,
                                          switchItem,
                                          container,
                                          new Inventory("player", null, 1, 1));
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotExactAmountAsContainer() {
            container.CreateItem("my item", 5, 2, 2);

            RequestChestRemove request = MakeMessage(5);
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            Assert.False(response.hasSwitched);
            TestForItems(container);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotFewerAmountAsContainer() {
            container.CreateItem("my item", 5, 2, 2);

            RequestChestRemove request = MakeMessage(3);
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 3);
            Assert.False(response.hasSwitched);
            TestForItems(container, new TestItem("my item", 2, new Vector2i(2, 2)));
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotMoreAmountAsContainer() {
            container.CreateItem("my item", 5, 2, 2);

            RequestChestRemove request = MakeMessage(7);
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            Assert.False(response.hasSwitched);
            TestForItems(container);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotItemNotInContainer() {
            RequestChestRemove request = MakeMessage(5);
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, false, 0);
            Assert.False(response.hasSwitched);
            TestForItems(container);
        }

        [Test]
        public void RPC_RequestItemRemoveDifferentItemToInventory() {
            container.CreateItem("my item A", 5, 2, 2);

            RequestChestRemove request = MakeMessage(5, Helper.CreateItem("my item B", 3));
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            Assert.True(response.hasSwitched);
            TestForItems(container, new TestItem("my item B", 3, new Vector2i(2, 2)));
        }

        [Test]
        public void RPC_RequestItemRemoveDifferentItemToInventoryWithTrySplit() {
            container.CreateItem("my item A", 5, 2, 2);

            RequestChestRemove request = MakeMessage(3, Helper.CreateItem("my item B", 3));
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, false, 0);
            Assert.False(response.hasSwitched);
            TestForItems(container, new TestItem("my item A", 5, new Vector2i(2, 2)));
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemToInventoryCanStack() {
            container.CreateItem("my item A", 5, 2, 2);

            RequestChestRemove request = MakeMessage(5, Helper.CreateItem("my item A", 5));
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            Assert.False(response.hasSwitched);
            TestForItems(container);
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemNotAllToInventoryCanStack() {
            container.CreateItem("my item A", 5, 2, 2);

            RequestChestRemove request = MakeMessage(3, Helper.CreateItem("my item A", 5));
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 3);
            Assert.False(response.hasSwitched);
            TestForItems(container, new TestItem("my item A", 2, new Vector2i(2, 2)));
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemToInventoryCanStackNotAll() {
            container.CreateItem("my item A", 5, 2, 2);

            RequestChestRemove request = MakeMessage(5, Helper.CreateItem("my item A", 19));
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 1);
            Assert.False(response.hasSwitched);
            TestForItems(container, new TestItem("my item A", 4, new Vector2i(2, 2)));
        }

        [Test]
        public void RPC_RequestItemRemoveGetItemDataBack() {
            container.CreateItem("my item A", 5, 2, 2);

            RequestChestRemove request = MakeMessage(5);
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            Assert.False(response.hasSwitched);
            TestForItem(response.responseItem, new TestItem("my item A", 5, new Vector2i(2, 2)));
            TestForItems(container);
        }

        [Test]
        public void RPC_RequestItemRemoveOddSplitEvenContainer() {
            container.CreateItem("my item A", 5, 2, 2);

            RequestChestRemove request = MakeMessage(3);
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 3);
            Assert.False(response.hasSwitched);
            TestForItem(response.responseItem, new TestItem("my item A", 3, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item A", 2, new Vector2i(2, 2)));
        }

        [Test]
        public void RPC_RequestItemRemoveOddSplitOddContainer() {
            container.CreateItem("my item A", 5, 2, 2);

            RequestChestRemove request = MakeMessage(2);
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 2);
            Assert.False(response.hasSwitched);
            TestForItem(response.responseItem, new TestItem("my item A", 2, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item A", 3, new Vector2i(2, 2)));
        }

        [Test]
        public void RPC_RequestItemRemove_SwitchLowerAmount() {
            container.CreateItem("my item A", 5, 2, 2);
            var switchItem = Helper.CreateItem("my item B", 3);

            RequestChestRemove request = MakeMessage(2, switchItem);
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, false, 0);
            Assert.False(response.hasSwitched);
            TestForItem(response.responseItem, new TestItem("my item B", 3, new Vector2i(0, 0)));
            TestForItems(container, new TestItem("my item A", 5, new Vector2i(2, 2)));
        }

        [Test]
        public void RPC_RequestItemRemove_Switch_RequestHigherAmount() {
            container.CreateItem("my item A", 5, 2, 2);
            var switchItem = Helper.CreateItem("my item B", 3);

            RequestChestRemove request = MakeMessage(7, switchItem);
            RequestChestRemoveResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            Assert.True(response.hasSwitched);
            TestForItem(response.responseItem, new TestItem("my item A", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item B", 3, new Vector2i(2, 2)));
        }

        [Test]
        public void RPC_RemoveItem_Switch_FailsByPatch() {
            container.CreateItem("my item A", 5, 2, 2);
            var switchItem = Helper.CreateItem("my item B", 7);

            RequestChestRemove request = MakeMessage(7, switchItem);
            Patches.PreventAddItem.Enable();
            RequestChestRemoveResponse response = GetResponse(request);
            Patches.PreventAddItem.Disable();

            TestResponse(response, true, 5);
            Assert.False(response.hasSwitched);
            TestForItem(response.responseItem, new TestItem("my item A", 5, new Vector2i(2, 2)));
            TestForItems(container);
            TestForItems(groundInv, new TestItem("my item B", 7, new Vector2i(0, 0)));
        }
    }
}
