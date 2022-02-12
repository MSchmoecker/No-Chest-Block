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

        private ZPackage GetAddResponse(RequestAdd request) {
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = container.RequestItemAdd(0L, data);
            response.SetPos(0);
            return response;
        }

        private ZPackage GetRemoveResponse(RequestRemove request) {
            ZPackage data = request.WriteToPackage();
            data.SetPos(0);

            ZPackage response = container.RequestItemRemove(0L, data);
            response.SetPos(0);
            return response;
        }

        [Test]
        public void AddToChest_SlotOccupied_DifferentItem_SplitMove() {
            player.AddItem(Helper.CreateItem("itemA", 5, 20), 5, 2, 2);
            container.AddItem(Helper.CreateItem("itemB", 5, 20), 5, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(new Vector2i(2, 2), new Vector2i(2, 2), 3, true, player, null);
            ZPackage response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, 0L, response);

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
            ZPackage response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, 0L, response);

            Assert.AreEqual("itemB", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemA", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_DifferentItem_SplitMove() {
            player.AddItem(Helper.CreateItem("itemA", 5, 20), 5, 2, 2);
            container.AddItem(Helper.CreateItem("itemB", 5, 20), 5, 2, 2);

            RequestRemove request = new RequestRemove(new Vector2i(2, 2), new Vector2i(2, 2), 3, player.GetItemAt(2, 2));
            ContainerHandler.RemoveItemFromChest(request, null);
            ZPackage response = GetRemoveResponse(request);
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
            ZPackage response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, 0L, response);

            Assert.IsNull(player.GetItemAt(2, 2));
            Assert.AreEqual("itemA", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_DifferentItem_FullMove() {
            player.AddItem(Helper.CreateItem("itemA", 5, 20), 5, 2, 2);
            container.AddItem(Helper.CreateItem("itemB", 5, 20), 5, 2, 2);

            RequestRemove request = new RequestRemove(new Vector2i(2, 2), new Vector2i(2, 2), 5, player.GetItemAt(2, 2));
            ContainerHandler.RemoveItemFromChest(request, null);
            ZPackage response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            Assert.AreEqual("itemB", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemA", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void RemoveFromChest_SlotEmpty_FullMove() {
            container.AddItem(Helper.CreateItem("itemB", 5, 20), 5, 2, 2);

            RequestRemove request = new RequestRemove(new Vector2i(2, 2), new Vector2i(2, 2), 5, player.GetItemAt(2, 2));
            ContainerHandler.RemoveItemFromChest(request, null);
            ZPackage response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            Assert.AreEqual("itemB", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.IsNull(container.GetItemAt(2, 2));
        }
    }
}
