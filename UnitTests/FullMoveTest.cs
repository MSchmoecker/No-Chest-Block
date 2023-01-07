using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class FullMoveTest : ItemTestBase {
        private Inventory player;
        private Inventory container;
        private Inventory ground;

        [SetUp]
        public void Setup() {
            player = new Inventory("player", null, 5, 5);
            container = new Inventory("container", null, 5, 5);
            ground = new Inventory("ground", null, 5, 5);
            Patches.DropPatch.OnDrop += AddItemToGround;
            InventoryBlock.Get(player).ReleaseBlockedSlots();
            InventoryBlock.Get(container).ReleaseBlockedSlots();
        }

        [TearDown]
        public void TearDown() {
            Patches.DropPatch.OnDrop -= AddItemToGround;
        }

        private void AddItemToGround(ItemDrop.ItemData item, int amount) {
            if (item != null && amount > 0) {
                ItemDrop.ItemData clone = item.Clone();
                clone.m_stack = amount;
                ground.AddItem(clone);
            }
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

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 3, true);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            TestForItems(player, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void AddToChest_SlotOccupied_DifferentItem_FullMove() {
            player.CreateItem("itemA", 5, 2, 2);
            container.CreateItem("itemB", 5, 2, 2);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 5, true);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            TestForItems(player, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void AddToChest_Full_FastMove() {
            container = new Inventory("container", null, 1, 1);
            player.CreateItem("itemA", 5, 2, 2);
            container.CreateItem("itemB", 5, 0, 0);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(2, 2), player, new Vector2i(-1, -1), ZDOID.None, 5, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            TestForItems(player, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemB", 5, new Vector2i(0, 0)));
            TestForItems(ground);
        }

        [Test]
        public void AddToChest_SplitStack_EnoughSpaceAtFirstSlot_FastMove() {
            // 25 itemA
            player.CreateItem("itemA", 10, 3, 3);
            container.CreateItem("itemA", 15, 2, 2);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(3, 3), player, new Vector2i(-1, -1), ZDOID.None, 5, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            // 25 itemA
            TestForItems(player, new TestItem("itemA", 5, new Vector2i(3, 3)));
            TestForItems(container, new TestItem("itemA", 20, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void AddToChest_SplitStack_NotEnoughSpaceAtFirstSlot_All_FastMove() {
            // 25 itemA
            player.CreateItem("itemA", 10, 3, 3);
            container.CreateItem("itemA", 15, 0, 0);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(3, 3), player, new Vector2i(-1, -1), ZDOID.None, 10, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            // 25 itemA
            TestForItems(player);
            TestForItems(container, new[] {
                new TestItem("itemA", 20, new Vector2i(0, 0)),
                new TestItem("itemA", 5, new Vector2i(1, 0))
            });
            TestForItems(ground);
        }

        [Test]
        public void AddToChest_SplitStack_NotEnoughSpaceAtFirstSlot_FastMove() {
            // 25 itemA
            player.CreateItem("itemA", 10, 3, 3);
            container.CreateItem("itemA", 15, 0, 0);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(3, 3), player, new Vector2i(-1, -1), ZDOID.None, 9, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            // 25 itemA
            TestForItems(player, new TestItem("itemA", 1, new Vector2i(3, 3)));
            TestForItems(container, new[] {
                new TestItem("itemA", 20, new Vector2i(0, 0)),
                new TestItem("itemA", 4, new Vector2i(1, 0))
            });
            TestForItems(ground);
        }

        [Test]
        public void AddToChest_SlotOccupied_SameItem_CannotStackAll() {
            player.CreateItem("itemA", 10, 2, 2);
            container.CreateItem("itemA", 15, 2, 2);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 10, true);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            TestForItems(player, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemA", 20, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_DifferentItem_SplitMove() {
            player.CreateItem("itemA", 5, 2, 2);
            container.CreateItem("itemB", 5, 2, 2);

            RequestRemove request = Helper.CreateContainer().RemoveItemFromChest(container.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 3, player.GetItemAt(2, 2));
            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            TestForItems(player, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_SameItem_SplitMove() {
            player.CreateItem("itemA", 5, 2, 2);
            container.CreateItem("itemA", 5, 2, 2);

            RequestRemove request = Helper.CreateContainer().RemoveItemFromChest(container.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 3, player.GetItemAt(2, 2));
            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            TestForItems(player, new TestItem("itemA", 8, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemA", 2, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_SameItem_SplitMove_Overflow() {
            player.CreateItem("itemA", 15, 2, 2);
            container.CreateItem("itemA", 15, 2, 2);

            RequestRemove request = Helper.CreateContainer().RemoveItemFromChest(container.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 7, player.GetItemAt(2, 2));
            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            TestForItems(player, new TestItem("itemA", 20, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemA", 10, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void AddToChest_SlotEmpty_FullMove() {
            player.CreateItem("itemA", 5, 2, 2);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 5, true);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            TestForItems(player);
            TestForItems(container, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void AddToChest_LastSlot_OverStack_FastMove() {
            container = new Inventory("inv", null, 1, 1);
            container.CreateItem("itemA", 15, 0, 0);
            player.CreateItem("itemA", 10, 2, 2);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(2, 2), player, new Vector2i(-1, -1), ZDOID.None, 10, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            TestForItems(player, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemA", 20, new Vector2i(0, 0)));
            TestForItems(ground);
        }

        [Test]
        public void AddItemToChestHigherDragAmount() {
            player.CreateItem("my item A", 5, 3, 3);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(3, 3), player, new Vector2i(1, 1), ZDOID.None, 7, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            TestForItems(player);
            TestForItems(container, new TestItem("my item A", 5, new Vector2i(1, 1)));
            TestForItems(ground);
        }

        [Test]
        public void AddItemToChestDifferentSlotSameItems() {
            player.CreateItem("my item A", 10, 2, 2);
            player.CreateItem("my item A", 10, 3, 2);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(3, 2), player, new Vector2i(1, 2), ZDOID.None, 3, false);
            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            TestForItems(player, new[] {
                new TestItem("my item A", 10, new Vector2i(2, 2)),
                new TestItem("my item A", 7, new Vector2i(3, 2)),
            });
            TestForItems(container, new[] {
                new TestItem("my item A", 3, new Vector2i(1, 2)),
            });
            TestForItems(ground);

            request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(2, 2), player, new Vector2i(1, 2), ZDOID.None, 2, false);
            response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            TestForItems(player, new[] {
                new TestItem("my item A", 8, new Vector2i(2, 2)),
                new TestItem("my item A", 7, new Vector2i(3, 2)),
            });
            TestForItems(container, new[] {
                new TestItem("my item A", 5, new Vector2i(1, 2)),
            });
            TestForItems(ground);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_DifferentItem_FullMove() {
            player.CreateItem("itemA", 5, 2, 2);
            container.CreateItem("itemB", 5, 2, 2);

            RequestRemove request = Helper.CreateContainer().RemoveItemFromChest(container.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 5, player.GetItemAt(2, 2));
            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            TestForItems(player, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void RemoveFromChest_SlotEmpty_FullMove() {
            container.CreateItem("itemB", 5, 2, 2);

            RequestRemove request = Helper.CreateContainer().RemoveItemFromChest(container.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 5, player.GetItemAt(2, 2));
            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            TestForItems(player, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(container);
            TestForItems(ground);
        }
        
        [Test]
        public void RemoveFromChest_SlotEmpty_SplitMove() {
            container.CreateItem("itemB", 5, 2, 2);

            RequestRemove request = Helper.CreateContainer().RemoveItemFromChest(container.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 3, player.GetItemAt(2, 2));
            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            TestForItems(player, new TestItem("itemB", 3, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemB", 2, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void RemoveFromChest_ItemRemovedBetweenRequests() {
            player.CreateItem("itemA", 4, 2, 2);
            container.CreateItem("itemB", 5, 2, 2);

            RequestRemove request = Helper.CreateContainer().RemoveItemFromChest(container.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 5, player.GetItemAt(2, 2));

            TestForItems(player);

            RequestRemoveResponse response = GetRemoveResponse(request);
            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            TestForItems(player, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemA", 4, new Vector2i(2, 2)));
            TestForItems(ground);
        }

        [Test]
        public void RemoveFromChest_ItemAddedBetweenRequest_DifferentItem() {
            container.CreateItem("itemB", 5, 2, 2);

            RequestRemove request = Helper.CreateContainer().RemoveItemFromChest(container.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 5);
            RequestRemoveResponse response = GetRemoveResponse(request);

            player.CreateItem("itemA", 5, 2, 2);

            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            TestForItems(player, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(container);
            TestForItems(ground, new TestItem("itemB", 5, new Vector2i(0, 0)));
        }

        [Test]
        public void RemoveFromChest_ItemAddedBetweenRequest_SameItem() {
            container.CreateItem("itemA", 5, 2, 2);

            RequestRemove request = Helper.CreateContainer().RemoveItemFromChest(container.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 5);
            RequestRemoveResponse response = GetRemoveResponse(request);

            player.CreateItem("itemA", 5, 2, 2);

            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            TestForItems(player, new TestItem("itemA", 10, new Vector2i(2, 2)));
            TestForItems(container);
            TestForItems(ground);
        }

        [Test]
        public void RemoveFromChest_ItemAddedBetweenRequest_SameItem_Overflow() {
            container.CreateItem("itemA", 5, 2, 2);

            RequestRemove request = Helper.CreateContainer().RemoveItemFromChest(container.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 5);
            RequestRemoveResponse response = GetRemoveResponse(request);

            player.CreateItem("itemA", 19, 2, 2);

            InventoryHandler.RPC_RequestItemRemoveResponse(player, response);

            TestForItems(player, new TestItem("itemA", 20, new Vector2i(2, 2)));
            TestForItems(container);
            TestForItems(ground, new TestItem("itemA", 4, new Vector2i(0, 0)));
        }

        [Test]
        public void AddToChest_InventoryChangedAfterRequestConstructed() {
            player.CreateItem("itemA", 5, 2, 2);

            RequestAdd request = Helper.CreateContainer().AddItemToChest(player.GetItemAt(2, 2), player, new Vector2i(2, 2), ZDOID.None, 5, true);
            container.CreateItem("itemB", 5, 2, 2);

            RequestAddResponse response = GetAddResponse(request);
            InventoryHandler.RPC_RequestItemAddResponse(player, response);

            TestForItems(player, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(container, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(ground);
        }
    }
}
