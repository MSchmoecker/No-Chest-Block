using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ContainerDropTest {
        private Inventory container;

        [SetUp]
        public void SetUp() {
            container = new Inventory("inventory", null, 4, 5);
        }

        private RequestDropResponse GetDropResponse(RequestDrop request) {
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = container.RequestDrop(0L, data);
            response.SetPos(0);

            return new RequestDropResponse(response);
        }


        [Test]
        public void RPC_RequestDropItem_FullStack() {
            container.AddItem(Helper.CreateItem("my item", 5, 20), 5, 2, 2);

            RequestDrop request = new RequestDrop(new Vector2i(2, 2), 5);
            RequestDropResponse response = GetDropResponse(request);

            Assert.NotNull(response.responseItem);
            Assert.AreEqual(5, response.responseItem.m_stack);
            Assert.AreEqual("my item", response.responseItem.m_shared.m_name);

            Assert.AreEqual(0, container.m_inventory.Count);
        }
        
        [Test]
        public void RPC_RequestDropItem_SpkitStack() {
            container.AddItem(Helper.CreateItem("my item", 5, 20), 5, 2, 2);

            RequestDrop request = new RequestDrop(new Vector2i(2, 2), 3);
            RequestDropResponse response = GetDropResponse(request);

            Assert.NotNull(response.responseItem);
            Assert.AreEqual(3, response.responseItem.m_stack);
            Assert.AreEqual("my item", response.responseItem.m_shared.m_name);

            Assert.AreEqual(1, container.m_inventory.Count);
            Assert.AreEqual(2, container.m_inventory[0].m_stack);
            Assert.AreEqual("my item", container.m_inventory[0].m_shared.m_name);
        }
    }
}
