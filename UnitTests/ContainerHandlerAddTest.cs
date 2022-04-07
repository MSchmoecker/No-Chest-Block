using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerAddTest {
        private Inventory container;

        [SetUp]
        public void SetUp() {
            container = new Inventory("inventory", null, 4, 5);
        }

        private RequestAddResponse GetResponse(RequestAdd request) {
            return container.RequestItemAdd(request);
        }

        private static RequestAdd MakeRequest(bool allowSwitch, int itemAmount = 5, int dragAmount = 5) {
            ItemDrop.ItemData item = Helper.CreateItem("my item", itemAmount, 20);
            return new RequestAdd(new Vector2i(2, 2), new Vector2i(3, 3), dragAmount, item, "inv", allowSwitch, ZDOID.None);
        }

        private static RequestAdd MakeRequest(bool allowSwitch, Vector2i target, int itemAmount = 5) {
            ItemDrop.ItemData item = Helper.CreateItem("my item", itemAmount, 20);
            return new RequestAdd(new Vector2i(2, 2), target, itemAmount, item, "inv", allowSwitch, ZDOID.None);
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotExactAmountAsInventory() {
            RequestAdd request = MakeRequest(true);
            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.Null(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotNotAllowSwitch() {
            RequestAdd request = MakeRequest(false);

            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.Null(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotMoreAmountAsInventory() {
            RequestAdd request = MakeRequest(true, 3);
            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(3, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual(2, response.switchItem.m_stack);
            Assert.AreEqual("my item", response.switchItem.m_shared.m_name);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(3, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotFewerAmountAsInventory() {
            RequestAdd request = MakeRequest(true, 5, 3);
            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(3, response.amount);
            Assert.Null(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(3, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemSlotDragExact() {
            container.CreateItem("my item A", 5, 3, 3);

            RequestAdd request = MakeRequest(true);
            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual(5, response.switchItem.m_stack);
            Assert.AreEqual("my item A", response.switchItem.m_shared.m_name);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemSlotDragTooFew() {
            container.CreateItem("my item A", 5, 3, 3);

            RequestAdd request = MakeRequest(true, 5, 3);
            RequestAddResponse response = GetResponse(request);

            Assert.False(response.success);
            Assert.AreEqual(0, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual("my item", response.switchItem.m_shared.m_name);
            Assert.AreEqual(3, response.switchItem.m_stack);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemNotAllowSwitch() {
            container.CreateItem("my item A", 5, 3, 3);

            RequestAdd request = MakeRequest(false, 6);

            RequestAddResponse response = GetResponse(request);

            Assert.False(response.success);
            Assert.AreEqual(0, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual("my item", response.switchItem.m_shared.m_name);
            Assert.AreEqual(5, response.switchItem.m_stack);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToSameItemSlotDragCanStack() {
            container.CreateItem("my item", 5, 3, 3);

            RequestAdd request = MakeRequest(true);

            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.Null(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(10, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToSameItemSlotDragCanStackNotAll() {
            container.CreateItem("my item", 19, 3, 3);

            RequestAdd request = MakeRequest(true);
            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(1, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual("my item", response.switchItem.m_shared.m_name);
            Assert.AreEqual(4, response.switchItem.m_stack);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(20, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemFullSlot() {
            container.CreateItem("my item", 20, 3, 3);

            RequestAdd request = MakeRequest(true);
            RequestAddResponse response = GetResponse(request);

            Assert.False(response.success);
            Assert.AreEqual(0, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual("my item", response.switchItem.m_shared.m_name);
            Assert.AreEqual(5, response.switchItem.m_stack);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(20, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemSwitched() {
            container.CreateItem("my item A", 20, 3, 3);

            RequestAdd request = MakeRequest(true);

            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(20, response.switchItem.m_stack);
            Assert.AreEqual("my item A", response.switchItem.m_shared.m_name);
        }

        [Test]
        public void RPC_AddItemFast_NoSpecialSlot_Full() {
            RequestAdd request = MakeRequest(true, new Vector2i(-1, -1));
            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.Null(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_AddItemFast_LastSlot_OverStack() {
            container = new Inventory("inv", null, 1, 1);
            container.CreateItem("my item", 15, 0, 0);

            RequestAdd request = MakeRequest(false, new Vector2i(-1, -1), 15);
            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual(10, response.switchItem.m_stack);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(20, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }
    }
}
