using System;
using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class RPCHandlerTest : ItemTestBase {
        private static Container container;

        [SetUp]
        public void SetUp() {
            container = Helper.CreateContainer();
            container.m_nview.ClaimOwnership();

            ZNetSimulate.routedRpcs.Clear();
        }

        [TearDown]
        public void TearDown() {
            ZNetSimulate.routedRpcs.Clear();
        }

        private static RequestChestAdd MakeRequest(Vector2i target, int itemAmount) {
            ItemDrop.ItemData item = Helper.CreateItem("my item", itemAmount);
            item.m_gridPos = new Vector2i(2, 2);
            return new RequestChestAdd(target, itemAmount, item, new Inventory("source", null, 1, 1), new Inventory("target", null, 1, 1));
        }

        private static void Invoke(Action<long, ZDOID, ZPackage> action, ZNetView nview, IPackage request) {
            ZPackage package = request.WriteToPackage();
            package.SetPos(0);
            action(0L, nview ? nview.m_zdo.m_uid : ZDOID.None, package);
        }

        [Test]
        public void RPC_AddItem() {
            RequestChestAdd request = MakeRequest(new Vector2i(1, 2), 5);
            request.RequestID = 5;

            Invoke(ContainerRPCHandler.RPC_RequestItemAdd, container.m_nview, request);
            RequestChestAddResponse response = ZNetSimulate.GetRoutedRpc(RPCRoute.RequestAddResponseRPC, package => new RequestChestAddResponse(package));

            TestResponse(response, true, 5);
            TestForItem(response.switchItem, null);
            TestForItems(container.m_inventory, new TestItem("my item", 5, new Vector2i(1, 2)));
            Assert.AreEqual(5, response.SourceID, "RequestChestAddResponse SourceID");
            Assert.AreEqual(new Vector2i(2, 2), response.inventoryPos, "RequestChestAddResponse inventoryPos");
        }

        [Test]
        public void RPC_AddItem_NotContainerOwner() {
            container.m_nview.m_zdo.SetOwner(-1);
            RequestChestAdd request = MakeRequest(new Vector2i(0, 0), 5);
            request.RequestID = 5;

            Invoke(ContainerRPCHandler.RPC_RequestItemAdd, container.m_nview, request);
            RequestChestAddResponse response = ZNetSimulate.GetRoutedRpc(RPCRoute.RequestAddResponseRPC, package => new RequestChestAddResponse(package));

            TestResponse(response, false, 0);
            TestForItem(response.switchItem, new TestItem("my item", 5, new Vector2i(2, 2)));
            TestForItems(container.m_inventory);
            Assert.AreEqual(5, response.SourceID, "RequestChestAddResponse SourceID");
            Assert.AreEqual(new Vector2i(2, 2), response.inventoryPos, "RequestChestAddResponse inventoryPos");
        }

        [Test]
        public void RPC_RemoveItem() {
            container.m_inventory.AddItem(Helper.CreateItem("my item", 5), 5, 1, 2);
            RequestChestRemove request = new RequestChestRemove(new Vector2i(1, 2), new Vector2i(0, 0), 5, Helper.CreateItem("my item 2", 5, 1, 2), null, null);
            request.RequestID = 5;

            Invoke(ContainerRPCHandler.RPC_RequestItemRemove, container.m_nview, request);
            RequestChestRemoveResponse response = ZNetSimulate.GetRoutedRpc(RPCRoute.RequestRemoveResponseRPC, package => new RequestChestRemoveResponse(package));

            TestResponse(response, true, 5);
            TestForItem(response.responseItem, new TestItem("my item", 5, new Vector2i(1, 2)));
            TestForItems(container.m_inventory, new TestItem("my item 2", 5, new Vector2i(1, 2)));
            Assert.AreEqual(5, response.SourceID, "RequestChestRemoveResponse SourceID");
        }

        [Test]
        public void RPC_RemoveItem_NotContainerOwner() {
            container.m_nview.m_zdo.SetOwner(-1);
            container.m_inventory.AddItem(Helper.CreateItem("my item", 5), 5, 1, 2);
            RequestChestRemove request = new RequestChestRemove(new Vector2i(1, 2), new Vector2i(0, 0), 5, Helper.CreateItem("my item 2", 5, 1, 2), null, null);
            request.RequestID = 5;

            Invoke(ContainerRPCHandler.RPC_RequestItemRemove, container.m_nview, request);
            RequestChestRemoveResponse response = ZNetSimulate.GetRoutedRpc(RPCRoute.RequestRemoveResponseRPC, package => new RequestChestRemoveResponse(package));

            TestResponse(response, false, 0);
            TestForItem(response.responseItem, new TestItem("my item 2", 5, new Vector2i(1, 2)));
            TestForItems(container.m_inventory, new TestItem("my item", 5, new Vector2i(1, 2)));
            Assert.AreEqual(5, response.SourceID, "RequestChestRemoveResponse SourceID");
        }

        [Test]
        public void RPC_MoveItem() {
            container.m_inventory.AddItem(Helper.CreateItem("my item", 5), 5, 1, 2);
            RequestMove request = new RequestMove(container.m_inventory.GetItemAt(1, 2), new Vector2i(0, 1), 5, container.m_inventory);
            request.RequestID = 5;

            Invoke(ContainerRPCHandler.RPC_RequestItemMove, container.m_nview, request);
            RequestMoveResponse response = ZNetSimulate.GetRoutedRpc(RPCRoute.RequestMoveResponseRPC, package => new RequestMoveResponse(package));

            TestResponse(response, true, 5);
            TestForItems(container.m_inventory, new TestItem("my item", 5, new Vector2i(0, 1)));
            Assert.AreEqual(5, response.SourceID, "RequestMoveResponse SourceID");
        }

        [Test]
        public void RPC_MoveItem_NotContainerOwner() {
            container.m_nview.m_zdo.SetOwner(-1);
            container.m_inventory.AddItem(Helper.CreateItem("my item", 5), 5, 1, 2);
            RequestMove request = new RequestMove(container.m_inventory.GetItemAt(1, 2), new Vector2i(0, 0), 5, container.m_inventory);
            request.RequestID = 5;

            Invoke(ContainerRPCHandler.RPC_RequestItemMove, container.m_nview, request);
            RequestMoveResponse response = ZNetSimulate.GetRoutedRpc(RPCRoute.RequestMoveResponseRPC, package => new RequestMoveResponse(package));

            TestResponse(response, false, 0);
            TestForItems(container.m_inventory, new TestItem("my item", 5, new Vector2i(1, 2)));
            Assert.AreEqual(5, response.SourceID, "RequestMoveResponse SourceID");
        }
    }
}
