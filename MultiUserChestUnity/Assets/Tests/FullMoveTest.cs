using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class FullMoveTest : ItemTestBase {
        private Inventory playerInv;
        private Inventory containerInv;
        private Inventory groundInv;

        private Container container;
        private ZDOID playerId;

        [SetUp]
        public void Setup() {
            playerInv = new Inventory("player", null, 5, 5);
            containerInv = new Inventory("container", null, 5, 5);
            groundInv = new Inventory("ground", null, 5, 5);

            container = Helper.CreateContainer(containerInv);
            playerId = Helper.CreatePlayerIdToInventory(playerInv);

            Patches.DropPatch.OnDrop += AddItemToGround;
            InventoryBlock.Get(playerInv).ReleaseBlockedSlots();
            InventoryBlock.Get(containerInv).ReleaseBlockedSlots();
        }

        [TearDown]
        public void TearDown() {
            Patches.DropPatch.OnDrop -= AddItemToGround;
            Helper.inventories.Clear();
            ZNetSimulate.routedRpcs.Clear();
        }

        private void AddItemToGround(ItemDrop.ItemData item, int amount) {
            if (item != null) {
                ItemDrop.ItemData clone = item.Clone();
                clone.m_stack = amount;
                groundInv.AddItem(clone);
            }
        }

        private RequestChestAddResponse GetAddResponse(RequestChestAdd request) {
            return containerInv.RequestItemAdd(request);
        }

        private RequestChestRemoveResponse GetRemoveResponse(RequestChestRemove request) {
            return containerInv.RequestItemRemove(request);
        }

        [Test]
        public void AddToChest_SlotOccupied_DifferentItem_SplitMove() {
            playerInv.CreateItem("itemA", 5, 2, 2);
            containerInv.CreateItem("itemB", 5, 2, 2);

            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 3);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void AddToChest_SlotOccupied_DifferentItem_FullMove() {
            playerInv.CreateItem("itemA", 5, 2, 2);
            containerInv.CreateItem("itemB", 5, 2, 2);

            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void AddToChest_Full_FastMove() {
            containerInv = new Inventory("container", null, 1, 1);
            container = Helper.CreateContainer(containerInv);
            playerInv.CreateItem("itemA", 5, 2, 2);
            containerInv.CreateItem("itemB", 5, 0, 0);

            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(-1, -1), playerId, 5);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemB", 5, new Vector2i(0, 0)));
            TestForItems(groundInv);
        }

        [Test]
        public void AddToChest_SplitStack_EnoughSpaceAtFirstSlot_FastMove() {
            // 25 itemA
            playerInv.CreateItem("itemA", 10, 3, 3);
            containerInv.CreateItem("itemA", 15, 2, 2);

            container.AddItemToChest(playerInv.GetItemAt(3, 3), playerInv, new Vector2i(-1, -1), playerId, 5);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            // 25 itemA
            TestForItems(playerInv, new TestItem("itemA", 5, new Vector2i(3, 3)));
            TestForItems(containerInv, new TestItem("itemA", 20, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void AddToChest_SplitStack_NotEnoughSpaceAtFirstSlot_All_FastMove() {
            // 25 itemA
            playerInv.CreateItem("itemA", 10, 3, 3);
            containerInv.CreateItem("itemA", 15, 0, 0);

            container.AddItemToChest(playerInv.GetItemAt(3, 3), playerInv, new Vector2i(-1, -1), playerId, 10);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            // 25 itemA
            TestForItems(playerInv);
            TestForItems(containerInv, new[] {
                new TestItem("itemA", 20, new Vector2i(0, 0)),
                new TestItem("itemA", 5, new Vector2i(1, 0))
            });
            TestForItems(groundInv);
        }

        [Test]
        public void AddToChest_SplitStack_NotEnoughSpaceAtFirstSlot_FastMove() {
            // 25 itemA
            playerInv.CreateItem("itemA", 10, 3, 3);
            containerInv.CreateItem("itemA", 15, 0, 0);

            container.AddItemToChest(playerInv.GetItemAt(3, 3), playerInv, new Vector2i(-1, -1), playerId, 9);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            // 25 itemA
            TestForItems(playerInv, new TestItem("itemA", 1, new Vector2i(3, 3)));
            TestForItems(containerInv, new[] {
                new TestItem("itemA", 20, new Vector2i(0, 0)),
                new TestItem("itemA", 4, new Vector2i(1, 0))
            });
            TestForItems(groundInv);
        }

        [Test]
        public void AddToChest_SlotOccupied_SameItem_CannotStackAll() {
            playerInv.CreateItem("itemA", 10, 2, 2);
            containerInv.CreateItem("itemA", 15, 2, 2);

            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 10);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemA", 20, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_DifferentItem_SplitMove() {
            playerInv.CreateItem("itemA", 5, 2, 2);
            containerInv.CreateItem("itemB", 5, 2, 2);

            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 3, playerInv.GetItemAt(2, 2));

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_SameItem_SplitMove() {
            playerInv.CreateItem("itemA", 5, 2, 2);
            containerInv.CreateItem("itemA", 5, 2, 2);

            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 3, playerInv.GetItemAt(2, 2));

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemA", 8, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemA", 2, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_SameItem_SplitMove_Overflow() {
            playerInv.CreateItem("itemA", 15, 2, 2);
            containerInv.CreateItem("itemA", 15, 2, 2);

            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 7, playerInv.GetItemAt(2, 2));

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemA", 20, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemA", 10, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void AddToChest_SlotEmpty_FullMove() {
            playerInv.CreateItem("itemA", 5, 2, 2);

            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv);
            TestForItems(containerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void AddToChest_LastSlot_OverStack_FastMove() {
            containerInv = new Inventory("inv", null, 1, 1);
            containerInv.CreateItem("itemA", 15, 0, 0);
            playerInv.CreateItem("itemA", 10, 2, 2);
            container = Helper.CreateContainer(containerInv);

            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(-1, -1), playerId, 10);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemA", 20, new Vector2i(0, 0)));
            TestForItems(groundInv);
        }

        [Test]
        public void AddItemToChestHigherDragAmount() {
            playerInv.CreateItem("my item A", 5, 3, 3);

            container.AddItemToChest(playerInv.GetItemAt(3, 3), playerInv, new Vector2i(1, 1), playerId, 7);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv);
            TestForItems(containerInv, new TestItem("my item A", 5, new Vector2i(1, 1)));
            TestForItems(groundInv);
        }

        [Test]
        public void AddItemToChestDifferentSlotSameItems() {
            playerInv.CreateItem("my item A", 10, 2, 2);
            playerInv.CreateItem("my item A", 10, 3, 2);

            container.AddItemToChest(playerInv.GetItemAt(3, 2), playerInv, new Vector2i(1, 2), playerId, 3);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new[] {
                new TestItem("my item A", 10, new Vector2i(2, 2)),
                new TestItem("my item A", 7, new Vector2i(3, 2)),
            });
            TestForItems(containerInv, new[] {
                new TestItem("my item A", 3, new Vector2i(1, 2)),
            });
            TestForItems(groundInv);

            container.m_nview.m_zdo.SetOwner(-1);
            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(1, 2), playerId, 2);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new[] {
                new TestItem("my item A", 8, new Vector2i(2, 2)),
                new TestItem("my item A", 7, new Vector2i(3, 2)),
            });
            TestForItems(containerInv, new[] {
                new TestItem("my item A", 5, new Vector2i(1, 2)),
            });
            TestForItems(groundInv);
        }

        [Test]
        public void RemoveFromChest_SlotOccupied_DifferentItem_FullMove() {
            playerInv.CreateItem("itemA", 5, 2, 2);
            containerInv.CreateItem("itemB", 5, 2, 2);

            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5, playerInv.GetItemAt(2, 2));

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void RemoveFromChest_SlotEmpty_FullMove() {
            containerInv.CreateItem("itemB", 5, 2, 2);

            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5, playerInv.GetItemAt(2, 2));

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(containerInv);
            TestForItems(groundInv);
        }

        [Test]
        public void RemoveFromChest_SlotEmpty_SplitMove() {
            containerInv.CreateItem("itemB", 5, 2, 2);

            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 3, playerInv.GetItemAt(2, 2));

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemB", 3, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemB", 2, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void RemoveFromChest_ItemRemovedBetweenRequests() {
            playerInv.CreateItem("itemA", 4, 2, 2);
            containerInv.CreateItem("itemB", 5, 2, 2);

            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5, playerInv.GetItemAt(2, 2));

            TestForItems(playerInv);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemA", 4, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void RemoveFromChest_ItemAddedBetweenRequest_DifferentItem() {
            containerInv.CreateItem("itemB", 5, 2, 2);

            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleRoutedRpcs(1);

            playerInv.CreateItem("itemA", 5, 2, 2);
            ZNetSimulate.HandleRoutedRpcs(1);

            TestForItems(playerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(containerInv);
            TestForItems(groundInv, new TestItem("itemB", 5, new Vector2i(0, 0)));
        }

        [Test]
        public void RemoveFromChest_ItemAddedBetweenRequest_SameItem() {
            containerInv.CreateItem("itemA", 5, 2, 2);

            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleRoutedRpcs(1);

            playerInv.CreateItem("itemA", 5, 2, 2);
            ZNetSimulate.HandleRoutedRpcs(1);

            TestForItems(playerInv, new TestItem("itemA", 10, new Vector2i(2, 2)));
            TestForItems(containerInv);
            TestForItems(groundInv);
        }

        [Test]
        public void RemoveFromChest_ItemAddedBetweenRequest_SameItem_Overflow() {
            containerInv.CreateItem("itemA", 5, 2, 2);

            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5);
            playerInv.CreateItem("itemA", 19, 2, 2);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemA", 20, new Vector2i(2, 2)));
            TestForItems(containerInv);
            TestForItems(groundInv, new TestItem("itemA", 4, new Vector2i(0, 0)));
        }

        [Test]
        public void AddToChest_InventoryChangedAfterRequestConstructed() {
            playerInv.CreateItem("itemA", 5, 2, 2);

            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5);
            containerInv.CreateItem("itemB", 5, 2, 2);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemB", 5, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void AddAndRemove_Switch() {
            playerInv.CreateItem("itemA", 4, 2, 2);
            containerInv.CreateItem("itemB", 4, 2, 2);

            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 4);
            container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 4);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv, new TestItem("itemB", 4, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemA", 4, new Vector2i(2, 2)));
            TestForItems(groundInv);
        }

        [Test]
        public void AddItemToChest_NotItemOfInventory() {
            ItemDrop.ItemData item = Helper.CreateItem("item", 4, 2, 2);
            containerInv.CreateItem("item", 4, 2, 2);

            container.AddItemToChest(item, playerInv, new Vector2i(2, 2), playerId, 4);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            TestForItems(playerInv);
            TestForItems(containerInv, new TestItem("item", 4, new Vector2i(2, 2)));
        }
    }
}
