using MultiUserChest;
using MultiUserChest.Patches;
using NUnit.Framework;
using UnityEngine;

namespace UnitTests {
    public class InterceptAddItemTest : ItemTestBase {
        private Container playerContainer;
        private Container chestContainer;

        private Inventory player;
        private Inventory chest;

        [SetUp]
        public void Setup() {
            playerContainer = Helper.CreateContainer();
            chestContainer = Helper.CreateContainer();
            player = playerContainer.GetInventory();
            chest = chestContainer.GetInventory();
            playerContainer.m_nview.ClaimOwnership();
            chestContainer.m_nview.GetZDO().SetOwner(-1);
        }

        [Test]
        public void NoFromOwner() {
            player = new Inventory("tmp", null, 4, 5);

            player.CreateItem("item A", 5, 1, 1);

            bool intercepted = InventoryPatch.InterceptAddItem(chest, player.GetItemAt(1, 1), 5, new Vector2i(2, 2), out bool successfulAdded);

            Assert.False(intercepted);
            Assert.False(successfulAdded);

            TestForItems(player, new TestItem("item A", 5, new Vector2i(1, 1)));
            TestForItems(chest);
        }

        [Test]
        public void NoToOwner() {
            chest = new Inventory("tmp", null, 4, 5);

            player.CreateItem("item A", 5, 1, 1);

            bool intercepted = InventoryPatch.InterceptAddItem(chest, player.GetItemAt(1, 1), 5, new Vector2i(2, 2), out bool successfulAdded);

            Assert.False(intercepted);
            Assert.False(successfulAdded);

            TestForItems(player, new TestItem("item A", 5, new Vector2i(1, 1)));
            TestForItems(chest);
        }

        [Test]
        public void NoIntercept_IsOwner() {
            playerContainer.m_nview.ClaimOwnership();
            chestContainer.m_nview.ClaimOwnership();

            player.CreateItem("item A", 5, 1, 1);

            bool intercepted = InventoryPatch.InterceptAddItem(player, player.GetItemAt(1, 1), 5, new Vector2i(2, 2), out bool successfulAdded);

            Assert.False(intercepted);
            Assert.False(successfulAdded);

            TestForItems(player, new TestItem("item A", 5, new Vector2i(1, 1)));
            TestForItems(chest);
        }

        [Test]
        public void Intercept_AddItem() {
            player.CreateItem("item A", 5, 1, 1);

            bool intercepted = InventoryPatch.InterceptAddItem(chest, player.GetItemAt(1, 1), 5, new Vector2i(2, 2), out bool successfulAdded);

            Assert.True(intercepted, "intercepted");
            Assert.True(successfulAdded, "successfulAdded");

            RequestChestAdd request = GetAddRequest();
            RequestChestAdd original = PackageHandler.GetPackage<RequestChestAdd>(request.RequestID);

            TestAddRequest(request, original, new TestItem("item A", 5, new Vector2i(1, 1)), new Vector2i(2, 2), true, player, chest);
        }

        [Test]
        public void Intercept_AddItem_Switch() {
            player.CreateItem("item A", 5, 1, 1);
            chest.CreateItem("item B", 5, 2, 2);

            bool intercepted = InventoryPatch.InterceptAddItem(chest, player.GetItemAt(1, 1), 5, new Vector2i(2, 2), out bool successfulAdded);

            Assert.True(intercepted, "intercepted");
            Assert.True(successfulAdded, "successfulAdded");

            RequestChestAdd request = GetAddRequest();
            RequestChestAdd original = PackageHandler.GetPackage<RequestChestAdd>(request.RequestID);

            TestAddRequest(request, original, new TestItem("item A", 5, new Vector2i(1, 1)), new Vector2i(2, 2), true, player, chest);
        }

        [Test]
        public void Intercept_AddItem_Stack() {
            player.CreateItem("item A", 5, 1, 1);
            chest.CreateItem("item A", 5, 2, 2);

            bool intercepted = InventoryPatch.InterceptAddItem(chest, player.GetItemAt(1, 1), 5, new Vector2i(2, 2), out bool successfulAdded);

            Assert.True(intercepted, "intercepted");
            Assert.True(successfulAdded, "successfulAdded");

            RequestChestAdd request = GetAddRequest();
            RequestChestAdd original = PackageHandler.GetPackage<RequestChestAdd>(request.RequestID);

            TestAddRequest(request, original, new TestItem("item A", 5, new Vector2i(1, 1)), new Vector2i(2, 2), true, player, chest);
        }

        [Test]
        public void Intercept_RemoveItem() {
            chest.CreateItem("item A", 5, 1, 1);

            bool intercepted = InventoryPatch.InterceptAddItem(player, chest.GetItemAt(1, 1), 5, new Vector2i(1, 2), out bool successfulAdded);

            Assert.True(intercepted, "intercepted");
            Assert.True(successfulAdded, "successfulAdded");

            RequestChestRemove request = GetRemoveRequest();
            RequestChestRemove original = PackageHandler.GetPackage<RequestChestRemove>(request.RequestID);

            TestRemoveRequest(request, original, null, new Vector2i(1, 1), new Vector2i(1, 2), 5, chest, player);
        }

        [Test]
        public void Intercept_RemoveItem_Switch() {
            player.CreateItem("item B", 5, 1, 2);
            chest.CreateItem("item A", 5, 1, 1);

            bool intercepted = InventoryPatch.InterceptAddItem(player, chest.GetItemAt(1, 1), 5, new Vector2i(1, 2), out bool successfulAdded);

            Assert.True(intercepted, "intercepted");
            Assert.True(successfulAdded, "successfulAdded");

            RequestChestRemove request = GetRemoveRequest();
            RequestChestRemove original = PackageHandler.GetPackage<RequestChestRemove>(request.RequestID);

            TestRemoveRequest(request, original, new TestItem("item B", 5, new Vector2i(1, 2)), new Vector2i(1, 1), new Vector2i(1, 2), 5, chest, player);
        }

        [Test]
        public void Intercept_RemoveItem_Stack() {
            player.CreateItem("item A", 5, 1, 2);
            chest.CreateItem("item A", 5, 1, 1);

            bool intercepted = InventoryPatch.InterceptAddItem(player, chest.GetItemAt(1, 1), 5, new Vector2i(1, 2), out bool successfulAdded);

            Assert.True(intercepted, "intercepted");
            Assert.True(successfulAdded, "successfulAdded");

            RequestChestRemove request = GetRemoveRequest();
            RequestChestRemove original = PackageHandler.GetPackage<RequestChestRemove>(request.RequestID);

            TestRemoveRequest(request, original, null, new Vector2i(1, 1), new Vector2i(1, 2), 5, chest, player);
        }

        [Test]
        public void Intercept_MoveItem() {
            chest.CreateItem("item A", 5, 1, 1);

            bool intercepted = InventoryPatch.InterceptAddItem(chest, chest.GetItemAt(1, 1), 5, new Vector2i(1, 2), out bool successfulAdded);

            Assert.True(intercepted, "intercepted");
            Assert.True(successfulAdded, "successfulAdded");

            RequestMove request = GetMoveRequest();
            RequestMove original = PackageHandler.GetPackage<RequestMove>(request.RequestID);

            TestMoveRequest(request, original, new Vector2i(1, 1), new Vector2i(1, 2), 5, "item A", chest);
        }

        [Test]
        public void Intercept_MoveItem_Switch() {
            chest.CreateItem("item A", 5, 1, 1);
            chest.CreateItem("item B", 5, 1, 2);

            bool intercepted = InventoryPatch.InterceptAddItem(chest, chest.GetItemAt(1, 1), 5, new Vector2i(1, 2), out bool successfulAdded);

            Assert.True(intercepted, "intercepted");
            Assert.True(successfulAdded, "successfulAdded");

            RequestMove request = GetMoveRequest();
            RequestMove original = PackageHandler.GetPackage<RequestMove>(request.RequestID);

            TestMoveRequest(request, original, new Vector2i(1, 1), new Vector2i(1, 2), 5, "item A", chest);
        }

        [Test]
        public void Intercept_MoveItem_Stack() {
            chest.CreateItem("item A", 5, 1, 1);
            chest.CreateItem("item A", 5, 1, 2);

            bool intercepted = InventoryPatch.InterceptAddItem(chest, chest.GetItemAt(1, 1), 5, new Vector2i(1, 2), out bool successfulAdded);

            Assert.True(intercepted, "intercepted");
            Assert.True(successfulAdded, "successfulAdded");

            RequestMove request = GetMoveRequest();
            RequestMove original = PackageHandler.GetPackage<RequestMove>(request.RequestID);

            TestMoveRequest(request, original, new Vector2i(1, 1), new Vector2i(1, 2), 5, "item A", chest);
        }

        private RequestChestAdd GetAddRequest() {
            ZNetSimulate.RoutedNetViewRpc rpc = ZNetSimulate.routedRpcs.Dequeue();
            Assert.AreEqual(ContainerPatch.ItemAddRPC, rpc.method);
            ZPackage package = (ZPackage)rpc.parameters[0];
            package.SetPos(0);
            return new RequestChestAdd(package);
        }

        private RequestChestRemove GetRemoveRequest() {
            ZNetSimulate.RoutedNetViewRpc rpc = ZNetSimulate.routedRpcs.Dequeue();
            Assert.AreEqual(ContainerPatch.ItemRemoveRPC, rpc.method);
            ZPackage package = (ZPackage)rpc.parameters[0];
            package.SetPos(0);
            return new RequestChestRemove(package);
        }

        private RequestMove GetMoveRequest() {
            ZNetSimulate.RoutedNetViewRpc rpc = ZNetSimulate.routedRpcs.Dequeue();
            Assert.AreEqual(ContainerPatch.ItemMoveRPC, rpc.method);
            ZPackage package = (ZPackage)rpc.parameters[0];
            package.SetPos(0);
            return new RequestMove(package);
        }
    }
}
