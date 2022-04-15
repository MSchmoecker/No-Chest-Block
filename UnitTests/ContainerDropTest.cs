using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ContainerDropTest : ItemTestBase {
        private Inventory container;

        [SetUp]
        public void SetUp() {
            container = new Inventory("inventory", null, 4, 5);
        }

        private RequestDropResponse GetDropResponse(RequestDrop request) {
            return container.RequestDrop(request);
        }

        [Test]
        public void RPC_RequestDropItem_FullStack() {
            container.CreateItem("my item", 5, 2, 2);

            RequestDrop request = new RequestDrop(new Vector2i(2, 2), 5, ZDOID.None);
            RequestDropResponse response = GetDropResponse(request);

            TestForItem(response.responseItem, new TestItem("my item", 5, new Vector2i(2, 2)));
            TestForItems(container);
        }

        [Test]
        public void RPC_RequestDropItem_SplitStack() {
            container.CreateItem("my item", 5, 2, 2);

            RequestDrop request = new RequestDrop(new Vector2i(2, 2), 3, ZDOID.None);
            RequestDropResponse response = GetDropResponse(request);

            TestForItem(response.responseItem, new TestItem("my item", 3, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("my item", 2, new Vector2i(2, 2)));
        }
    }
}
