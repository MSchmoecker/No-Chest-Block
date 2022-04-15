using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ConsumeTest : ItemTestBase {
        private Inventory container;

        [SetUp]
        public void SetUp() {
            container = new Inventory("inventory", null, 4, 5);
        }

        private RequestConsumeResponse GetConsumeResponse(RequestConsume request) {
            return container.RequestItemConsume(request);
        }

        [Test]
        public void Consume() {
            container.CreateItem("my item", 10, 2, 3);

            RequestConsume request = new RequestConsume(container.GetItemAt(2, 3));
            RequestConsumeResponse response = GetConsumeResponse(request);
            TestResponse(response, true, 1);

            TestForItem(response.item, new TestItem("my item", 1, new Vector2i(2, 3)));
            TestForItems(container, new TestItem("my item", 9, new Vector2i(2, 3)));
        }

        [Test]
        public void Consume_LastItem() {
            container.CreateItem("my item", 1, 2, 3);

            RequestConsume request = new RequestConsume(container.GetItemAt(2, 3));
            RequestConsumeResponse response = GetConsumeResponse(request);
            TestResponse(response, true, 1);

            TestForItem(response.item, new TestItem("my item", 1, new Vector2i(2, 3)));
            TestForItems(container);
        }

        [Test]
        public void Consume_NotExistingItem() {
            ItemDrop.ItemData itemData = Helper.CreateItem("my item", 5);

            RequestConsume request = new RequestConsume(itemData);
            RequestConsumeResponse response = GetConsumeResponse(request);
            TestResponse(response, false, 0);

            TestForItem(response.item, null);
            TestForItems(container);
        }
    }
}
