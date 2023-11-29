using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerAddTest : ItemTestBase {
        private Inventory container;

        [SetUp]
        public void SetUp() {
            container = new Inventory("inventory", null, 4, 5);
        }

        private RequestChestAddResponse GetResponse(RequestChestAdd request) {
            return container.RequestItemAdd(request);
        }

        private static RequestChestAdd MakeRequest(int itemAmount = 5, int dragAmount = 5) {
            ItemDrop.ItemData item = Helper.CreateItem("my item", itemAmount);
            item.m_gridPos = new Vector2i(2, 2);
            return new RequestChestAdd(new Vector2i(3, 3), dragAmount, item, new Inventory("source", null, 1, 1), new Inventory("target", null, 1, 1));
        }

        private static RequestChestAdd MakeRequest(Vector2i target, int itemAmount = 5) {
            ItemDrop.ItemData item = Helper.CreateItem("my item", itemAmount);
            item.m_gridPos = new Vector2i(2, 2);
            return new RequestChestAdd(target, itemAmount, item, new Inventory("source", null, 1, 1), new Inventory("target", null, 1, 1));
        }

        private static RequestChestAdd MakeRequest(Vector2i target, int itemAmount, int dragAmount) {
            ItemDrop.ItemData item = Helper.CreateItem("my item", itemAmount);
            item.m_gridPos = new Vector2i(2, 2);
            return new RequestChestAdd(target, dragAmount, item, new Inventory("source", null, 1, 1), new Inventory("target", null, 1, 1));
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotExactAmountAsInventory() {
            RequestChestAdd request = MakeRequest();
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 5, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotNotAllowSwitch() {
            RequestChestAdd request = MakeRequest();
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 5, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotMoreAmountAsInventory() {
            RequestChestAdd request = MakeRequest(3);
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, true, 3);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 3, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotFewerAmountAsInventory() {
            RequestChestAdd request = MakeRequest(5, 3);
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, true, 3);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 3, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemSlotDragExact() {
            container.CreateItem("my item A", 5, 3, 3);

            RequestChestAdd request = MakeRequest();
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, new TestItem("my item A", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item", 5, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_RequestItemAdd_ToDifferentItemSlot_DragTooFew() {
            container.CreateItem("my item A", 5, 3, 3);

            RequestChestAdd request = MakeRequest(5, 3);
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, false, 0);
            TestForItem(response.switchItem, new TestItem("my item", 3, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item A", 5, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_RequestItemAdd_ToDifferentItem_NotAllowSwitch() {
            container.CreateItem("my item A", 5, 3, 3);

            RequestChestAdd request = MakeRequest(6);
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, false, 0);
            TestForItem(response.switchItem, new TestItem("my item", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item A", 5, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_RequestItemAddToSameItemSlotDragCanStack() {
            container.CreateItem("my item", 5, 3, 3);

            RequestChestAdd request = MakeRequest();
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 10, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_RequestItemAddToSameItemSlotDragCanStackNotAll() {
            container.CreateItem("my item", 19, 3, 3);

            RequestChestAdd request = MakeRequest();
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, true, 1);
            TestForItem(response.switchItem, new TestItem("my item", 4, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item", 20, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_RequestItemFullSlot() {
            container.CreateItem("my item", 20, 3, 3);

            RequestChestAdd request = MakeRequest();
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, false, 0);
            TestForItem(response.switchItem, new TestItem("my item", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item", 20, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_RequestItemSwitched() {
            container.CreateItem("my item A", 20, 3, 3);

            RequestChestAdd request = MakeRequest();
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, new TestItem("my item A", 20, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item", 5, new Vector2i(3, 3)));
        }

        [Test]
        public void RPC_AddItemFast_NoSpecialSlot_Full() {
            RequestChestAdd request = MakeRequest(new Vector2i(-1, -1));
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 5, new Vector2i(0, 0)));
        }

        [Test]
        public void RPC_AddItemFast_LastSlot_OverStack() {
            container = new Inventory("inv", null, 1, 1);
            container.CreateItem("my item", 15, 0, 0);

            RequestChestAdd request = MakeRequest(new Vector2i(-1, -1), 15);
            RequestChestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, new TestItem("my item", 10, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item", 20, new Vector2i(0, 0)));
        }

        [Test]
        public void RPC_AddItem_FailsByPatch() {
            container = new Inventory("inv", null, 1, 1);
            container.CreateItem("my item", 2, 0, 0);

            RequestChestAdd request = MakeRequest(new Vector2i(0, 0), 4, 3);
            Patches.PreventAddItem.Enable();
            RequestChestAddResponse response = GetResponse(request);
            Patches.PreventAddItem.Disable();

            TestResponse(response, false, 0);
            TestForItem(response.switchItem, new TestItem("my item", 3, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item", 2, new Vector2i(0, 0)));
        }
    }
}
