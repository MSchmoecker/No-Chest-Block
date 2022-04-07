using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerAddTest : ItemTestBase {
        private Inventory container;

        [SetUp]
        public void SetUp() {
            container = new Inventory("inventory", null, 4, 5);
        }

        private RequestAddResponse GetResponse(RequestAdd request) {
            return container.RequestItemAdd(request);
        }

        private static RequestAdd MakeRequest(bool allowSwitch, int itemAmount = 5, int dragAmount = 5) {
            ItemDrop.ItemData item = Helper.CreateItem("my item", itemAmount);
            return new RequestAdd(new Vector2i(2, 2), new Vector2i(3, 3), dragAmount, item, "inv", allowSwitch, ZDOID.None);
        }

        private static RequestAdd MakeRequest(bool allowSwitch, Vector2i target, int itemAmount = 5) {
            ItemDrop.ItemData item = Helper.CreateItem("my item", itemAmount);
            return new RequestAdd(new Vector2i(2, 2), target, itemAmount, item, "inv", allowSwitch, ZDOID.None);
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotExactAmountAsInventory() {
            RequestAdd request = MakeRequest(true);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 5));
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotNotAllowSwitch() {
            RequestAdd request = MakeRequest(false);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 5));
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotMoreAmountAsInventory() {
            RequestAdd request = MakeRequest(true, 3);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, true, 3);
            TestForItem(response.switchItem, new TestItem("my item", 2));
            TestForItems(container, new TestItem("my item", 3));
        }

        [Test]
        public void RPC_RequestItemAddToEmptySlotFewerAmountAsInventory() {
            RequestAdd request = MakeRequest(true, 5, 3);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, true, 3);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 3));
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemSlotDragExact() {
            container.CreateItem("my item A", 5, 3, 3);

            RequestAdd request = MakeRequest(true);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, new TestItem("my item A", 5));
            TestForItems(container, new TestItem("my item", 5));
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemSlotDragTooFew() {
            container.CreateItem("my item A", 5, 3, 3);

            RequestAdd request = MakeRequest(true, 5, 3);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, false, 0);
            TestForItem(response.switchItem, new TestItem("my item", 3));
            TestForItems(container, new TestItem("my item A", 5));
        }

        [Test]
        public void RPC_RequestItemAddToDifferentItemNotAllowSwitch() {
            container.CreateItem("my item A", 5, 3, 3);

            RequestAdd request = MakeRequest(false, 6);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, false, 0);
            TestForItem(response.switchItem, new TestItem("my item", 5));
            TestForItems(container, new TestItem("my item A", 5));
        }

        [Test]
        public void RPC_RequestItemAddToSameItemSlotDragCanStack() {
            container.CreateItem("my item", 5, 3, 3);

            RequestAdd request = MakeRequest(true);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 10));
        }

        [Test]
        public void RPC_RequestItemAddToSameItemSlotDragCanStackNotAll() {
            container.CreateItem("my item", 19, 3, 3);

            RequestAdd request = MakeRequest(true);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, true, 1);
            TestForItem(response.switchItem, new TestItem("my item", 4));
            TestForItems(container, new TestItem("my item", 20));
        }

        [Test]
        public void RPC_RequestItemFullSlot() {
            container.CreateItem("my item", 20, 3, 3);

            RequestAdd request = MakeRequest(true);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, false, 0);
            TestForItem(response.switchItem, new TestItem("my item", 5));
            TestForItems(container, new TestItem("my item", 20));
        }

        [Test]
        public void RPC_RequestItemSwitched() {
            container.CreateItem("my item A", 20, 3, 3);

            RequestAdd request = MakeRequest(true);

            RequestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, new TestItem("my item A", 20));
            TestForItems(container, new TestItem("my item", 5));
        }

        [Test]
        public void RPC_AddItemFast_NoSpecialSlot_Full() {
            RequestAdd request = MakeRequest(true, new Vector2i(-1, -1));
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, null);
            TestForItems(container, new TestItem("my item", 5));
        }

        [Test]
        public void RPC_AddItemFast_LastSlot_OverStack() {
            container = new Inventory("inv", null, 1, 1);
            container.CreateItem("my item", 15, 0, 0);

            RequestAdd request = MakeRequest(false, new Vector2i(-1, -1), 15);
            RequestAddResponse response = GetResponse(request);

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, new TestItem("my item", 10));
            TestForItems(container, new TestItem("my item", 20));
        }
    }
}
