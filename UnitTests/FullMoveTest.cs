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
            return container.RequestItemAdd(request);
        }

        private RequestRemoveResponse GetRemoveResponse(RequestRemove request) {
            return container.RequestItemRemove(request);
        }

        [Test]
        public void AddToChest_SlotOccupied_DifferentItem_SplitMove() {
            player.CreateItem("itemA", 5, 2, 2);
            container.CreateItem("itemB", 5, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(null, player, ZDOID.None, new Vector2i(2, 2), new Vector2i(2, 2), 3, true);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.AreEqual("itemA", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemB", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void AddToChest_SlotOccupied_DifferentItem_FullMove() {
            player.CreateItem("itemA", 5, 2, 2);
            container.CreateItem("itemB", 5, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(null, player, ZDOID.None,new Vector2i(2, 2), new Vector2i(2, 2), 5, true);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.AreEqual("itemB", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemA", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void AddToChest_Full_FastMove() {
            container = new Inventory("container", null, 1, 1);
            player.CreateItem("itemA", 5, 2, 2);
            container.CreateItem("itemB", 5, 0, 0);

            RequestAdd request = ContainerHandler.AddItemToChest(null, player, ZDOID.None,new Vector2i(2, 2), new Vector2i(-1, -1), 5, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.AreEqual("itemA", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemB", container.GetItemAt(0, 0).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(0, 0).m_stack);
        }

        [Test]
        public void AddToChest_SplitStack_EnoughSpaceAtFirstSlot_FastMove() {
            // 25 itemA
            player.CreateItem("itemA", 10, 3, 3);
            container.CreateItem("itemA", 15, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(null, player, ZDOID.None,new Vector2i(3, 3), new Vector2i(-1, -1), 5, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.AreEqual("itemA", player.GetItemAt(3, 3).m_shared.m_name);
            Assert.AreEqual(2, container.m_inventory.Count);
            Assert.AreEqual("itemA", container.m_inventory[0].m_shared.m_name);
            Assert.AreEqual("itemA", container.m_inventory[1].m_shared.m_name);

            // 25 itemA
            Assert.AreEqual(5, player.GetItemAt(3, 3).m_stack);
            Assert.AreEqual(15, container.m_inventory[0].m_stack);
            Assert.AreEqual(5, container.m_inventory[1].m_stack);
        }

        [Test]
        public void AddToChest_SplitStack_NotEnoughSpaceAtFirstSlot_All_FastMove() {
            // 25 itemA
            player.CreateItem("itemA", 10, 3, 3);
            container.CreateItem("itemA", 15, 0, 0);

            RequestAdd request = ContainerHandler.AddItemToChest(null, player, ZDOID.None,new Vector2i(3, 3), new Vector2i(-1, -1), 10, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.AreEqual(0, player.m_inventory.Count);
            Assert.AreEqual(2, container.m_inventory.Count);
            Assert.AreEqual("itemA", container.m_inventory[0].m_shared.m_name);
            Assert.AreEqual("itemA", container.m_inventory[1].m_shared.m_name);

            // 25 itemA
            Assert.AreEqual(20, container.m_inventory[0].m_stack);
            Assert.AreEqual(5, container.m_inventory[1].m_stack);
        }

        [Test]
        public void AddToChest_SplitStack_NotEnoughSpaceAtFirstSlot_FastMove() {
            // 25 itemA
            player.CreateItem("itemA", 10, 3, 3);
            container.CreateItem("itemA", 15, 0, 0);

            RequestAdd request = ContainerHandler.AddItemToChest(null, player, ZDOID.None,new Vector2i(3, 3), new Vector2i(-1, -1), 9, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.AreEqual("itemA", player.GetItemAt(3, 3).m_shared.m_name);
            Assert.AreEqual(2, container.m_inventory.Count);
            Assert.AreEqual("itemA", container.m_inventory[0].m_shared.m_name);
            Assert.AreEqual("itemA", container.m_inventory[1].m_shared.m_name);

            // 25 itemA
            Assert.AreEqual(1, player.GetItemAt(3, 3).m_stack);
            Assert.AreEqual(20, container.m_inventory[0].m_stack);
            Assert.AreEqual(4, container.m_inventory[1].m_stack);
        }

        [Test]
        public void AddToChest_SlotOccupied_SameItem_CannotStackAll() {
            player.CreateItem("itemA", 10, 2, 2);
            container.CreateItem("itemA", 15, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(null, player, ZDOID.None,new Vector2i(2, 2), new Vector2i(2, 2), 10, true);
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
            player.CreateItem("itemA", 5, 2, 2);
            container.CreateItem("itemB", 5, 2, 2);

            RequestRemove request = new RequestRemove(new Vector2i(2, 2), new Vector2i(2, 2), 3, "inv".GetStableHashCode(), player.GetItemAt(2, 2), ZDOID.None);
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
            player.CreateItem("itemA", 5, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(null, player, ZDOID.None,new Vector2i(2, 2), new Vector2i(2, 2), 5, true);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.IsNull(player.GetItemAt(2, 2));
            Assert.AreEqual("itemA", container.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, container.GetItemAt(2, 2).m_stack);
        }

        [Test]
        public void AddToChest_LastSlot_OverStack_FastMove() {
            container = new Inventory("inv", null, 1, 1);
            container.CreateItem("itemA", 15, 0, 0);
            player.CreateItem("itemA", 10, 2, 2);

            RequestAdd request = ContainerHandler.AddItemToChest(null, player, ZDOID.None, new Vector2i(2, 2), new Vector2i(-1, -1), 10, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            Assert.NotNull(player.GetItemAt(2, 2));
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.AreEqual("itemA", container.GetItemAt(0, 0).m_shared.m_name);
            Assert.AreEqual(20, container.GetItemAt(0, 0).m_stack);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_DifferentItem_FullMove() {
            player.CreateItem("itemA", 5, 2, 2);
            container.CreateItem("itemB", 5, 2, 2);

            RequestRemove request = new RequestRemove(new Vector2i(2, 2), new Vector2i(2, 2), 5, "inv".GetStableHashCode(), player.GetItemAt(2, 2), ZDOID.None);
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
            container.CreateItem("itemB", 5, 2, 2);

            RequestRemove request = new RequestRemove(new Vector2i(2, 2), new Vector2i(2, 2), 5, "inv".GetStableHashCode(), player.GetItemAt(2, 2), ZDOID.None);
            ContainerHandler.RemoveItemFromChest(request, null);
            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            Assert.AreEqual("itemB", player.GetItemAt(2, 2).m_shared.m_name);
            Assert.AreEqual(5, player.GetItemAt(2, 2).m_stack);
            Assert.IsNull(container.GetItemAt(2, 2));
        }
    }
}
