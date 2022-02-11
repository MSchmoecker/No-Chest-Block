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
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = container.RequestItemAdd(0L, data);
            response.SetPos(0);

            return new RequestAddResponse(response);
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotExactAmountAsInventory() {
            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item", 5, 20),
                                                true);

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
            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item", 5, 20),
                                                false);

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
            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item", 3, 20),
                                                true);

            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(3, response.amount);
            Assert.Null(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(3, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotFewerAmountAsInventory() {
            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                3,
                                                Helper.CreateItem("my item", 5, 20),
                                                true);

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
            container.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 3, 3);

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item B", 5, 20),
                                                true);

            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual(5, response.switchItem.m_stack);
            Assert.AreEqual("my item A", response.switchItem.m_shared.m_name);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item B", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemSlotDragTooFew() {
            container.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 3, 3);

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                3,
                                                Helper.CreateItem("my item B", 5, 20),
                                                true);

            RequestAddResponse response = GetResponse(request);

            Assert.False(response.success);
            Assert.AreEqual(0, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual("my item B", response.switchItem.m_shared.m_name);
            Assert.AreEqual(3, response.switchItem.m_stack);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemNotAllowSwitch() {
            container.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 3, 3);

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item B", 6, 20),
                                                false);

            RequestAddResponse response = GetResponse(request);

            Assert.False(response.success);
            Assert.AreEqual(0, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual("my item B", response.switchItem.m_shared.m_name);
            Assert.AreEqual(5, response.switchItem.m_stack);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToSameItemSlotDragCanStack() {
            container.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 3, 3);

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item A", 5, 20),
                                                true);

            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.Null(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(10, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemAddToSameItemSlotDragCanStackNotAll() {
            container.AddItem(Helper.CreateItem("my item A", 19, 20), 19, 3, 3);

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item A", 5, 20),
                                                true);
            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(1, response.amount);
            Assert.Null(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(20, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemFullSlot() {
            container.AddItem(Helper.CreateItem("my item A", 20, 20), 20, 3, 3);

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item A", 5, 20),
                                                true);

            RequestAddResponse response = GetResponse(request);

            Assert.False(response.success);
            Assert.AreEqual(0, response.amount);
            Assert.Null(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(20, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item A", container.m_inventory[0].m_shared.m_name);
        }

        [Test]
        public void RPC_RequestItemSwitched() {
            container.AddItem(Helper.CreateItem("my item A", 20, 20), 20, 3, 3);

            RequestAdd request = new RequestAdd(new Vector2i(2, 2),
                                                new Vector2i(3, 3),
                                                5,
                                                Helper.CreateItem("my item B", 5, 20),
                                                true);
            
            RequestAddResponse response = GetResponse(request);

            Assert.True(response.success);
            Assert.AreEqual(5, response.amount);
            Assert.NotNull(response.switchItem);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(5, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item B", container.m_inventory[0].m_shared.m_name);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(20, response.switchItem.m_stack);
            Assert.AreEqual("my item A", response.switchItem.m_shared.m_name);
        }
    }
}
