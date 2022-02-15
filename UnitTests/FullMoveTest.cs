using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class FullMoveTest {
        private Inventory player;
        private Inventory container;

        [SetUp]
        public void Setup() {
            player = new Inventory("player", null, 5, 5);
            container = new Inventory("container", null, 5, 5);
            InventoryHandler.blockedSlots.Clear();
        }

        private RequestAddResponse GetAddResponse(RequestAdd request) {
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = container.RequestItemAdd(0L, data);
            response.SetPos(0);
            return new RequestAddResponse(response);
        }

        private RequestRemoveResponse GetRemoveResponse(RequestRemove request) {
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = container.RequestItemRemove(0L, data);
            response.SetPos(0);
            return new RequestRemoveResponse(response);;
        }

        [Test]
        public void AddToChest_SlotOccupied_DifferentItem_SplitMove() {
            player.AddItem(Helper.CreateItem("itemA", 5, 20), 5, 2, 2);
            container.AddItem(Helper.CreateItem("itemB", 5, 20), 5, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(new Vector2i(2, 2), new Vector2i(2, 2), 3, true, player, null);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.AreEqual("itemA", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemB", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void AddToChest_SlotOccupied_DifferentItem_FullMove() {
            player.AddItem(Helper.CreateItem("itemA", 5, 20), 5, 2, 2);
            container.AddItem(Helper.CreateItem("itemB", 5, 20), 5, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(new Vector2i(2, 2), new Vector2i(2, 2), 5, true, player, null);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.AreEqual("itemB", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemA", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void AddToChest_Full_FastMove() {
            player.AddItem(Helper.CreateItem("itemA", 5, 20), 5, 2, 2);
            container = new Inventory("container", null, 1, 1);
            container.AddItem(Helper.CreateItem("itemB", 5, 20), 5, 0, 0);

            Vector2i targetPos = container.FindEmptySlot(true);
            RequestAdd request = ContainerHandler.AddItemToChest(new Vector2i(2, 2), targetPos, 5, false, player, null);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.AreEqual("itemA", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemB", container.GetItemAt(0, 0).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(0, 0).m_stack);
        }

        [Test]
        public void AddToChest_SlotOccupied_SameItem_CannotStackAll() {
            player.AddItem(Helper.CreateItem("itemA", 10, 20), 10, 2, 2);
            container.AddItem(Helper.CreateItem("itemA", 15, 20), 15, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(new Vector2i(2, 2), new Vector2i(2, 2), 10, true, player, null);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.NotNull(player.GetItemAt(2, 2));
            Assert.NotNull(container.GetItemAt(2, 2));

            Assert.AreEqual("itemA", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemA", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(20, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_DifferentItem_SplitMove() {
            player.AddItem(Helper.CreateItem("itemA", 5, 20), 5, 2, 2);
            container.AddItem(Helper.CreateItem("itemB", 5, 20), 5, 2, 2);

            RequestRemove request = new RequestRemove(new Vector2i(2, 2), new Vector2i(2, 2), 3, "inv", player.GetItemAt(2, 2));
            ContainerHandler.RemoveItemFromChest(request, null);
            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            Assert.AreEqual("itemA", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemB", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void AddToChest_SlotEmpty_FullMove() {
            player.AddItem(Helper.CreateItem("itemA", 5, 20), 5, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(new Vector2i(2, 2), new Vector2i(2, 2), 5, true, player, null);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.IsNull(player.GetItemAt(2, 2));
            Assert.AreEqual("itemA", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_DifferentItem_FullMove() {
            player.AddItem(Helper.CreateItem("itemA", 5, 20), 5, 2, 2);
            container.AddItem(Helper.CreateItem("itemB", 5, 20), 5, 2, 2);

            RequestRemove request = new RequestRemove(new Vector2i(2, 2), new Vector2i(2, 2), 5, "inv", player.GetItemAt(2, 2));
            ContainerHandler.RemoveItemFromChest(request, null);
            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            Assert.AreEqual("itemB", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemA", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void RemoveFromChest_SlotEmpty_FullMove() {
            container.AddItem(Helper.CreateItem("itemB", 5, 20), 5, 2, 2);

            RequestRemove request = new RequestRemove(new Vector2i(2, 2), new Vector2i(2, 2), 5, "inv", player.GetItemAt(2, 2));
            ContainerHandler.RemoveItemFromChest(request, null);
            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            Assert.AreEqual("itemB", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.IsNull(container.GetItemAt(2, 2));
        }
    }
}
