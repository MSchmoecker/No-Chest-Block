using System.Linq;
using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class InventoryPreviewTest : ItemTestBase {
        private Inventory player;
        private Inventory chest;

        [SetUp]
        public void Setup() {
            player = new Inventory("player", null, 2, 2);
            chest = new Inventory("chest", null, 2, 2);
        }

        [Test]
        public void NoChanges() {
            bool hasChanges = InventoryPreview.GetChanges(player, out SlotPreview preview);

            Assert.IsFalse(hasChanges);
            Assert.IsNull(preview);
        }

        [Test]
        public void AddRequest_EmptySlot() {
            player.CreateItem("item A", 5, 0, 0);
            RequestChestAdd request = new RequestChestAdd(new Vector2i(0, 1), 5, player.GetItemAt(0, 0), player, chest);
            InventoryPreview.AddPackage(request);

            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);
            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsFalse(hasPlayerChanges, "Player should not have changes");

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item A", 5, new Vector2i(0, 1))
            });
        }

        [Test]
        public void AddRequest_Switch() {
            player.CreateItem("item A", 5, 0, 0);
            chest.CreateItem("item B", 5, 0, 1);
            RequestChestAdd request = new RequestChestAdd(new Vector2i(0, 1), 5, player.GetItemAt(0, 0), player, chest);
            InventoryPreview.AddPackage(request);
            player.RemoveItem(player.GetItemAt(0, 0));

            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);
            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasPlayerChanges, "Player should have changes");
            TestSlotPreview(player, playerPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 0),
                item = new TestItem("item B", 5, new Vector2i(0, 0))
            });

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item A", 5, new Vector2i(0, 1))
            });
        }

        [Test]
        public void AddRequest_Stack() {
            player.CreateItem("item A", 5, 0, 0);
            chest.CreateItem("item A", 5, 0, 1);
            RequestChestAdd request = new RequestChestAdd(new Vector2i(0, 1), 3, player.GetItemAt(0, 0), player, chest);
            InventoryPreview.AddPackage(request);
            player.RemoveItem(player.GetItemAt(0, 0), 3);

            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);
            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsFalse(hasPlayerChanges, "Player should have changes");

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item A", 8, new Vector2i(0, 1))
            });
        }

        [Test]
        public void AddRequest_FastMove() {
            player.CreateItem("item A", 5, 0, 0);

            RequestChestAdd request = new RequestChestAdd(new Vector2i(-1, -1), 5, player.GetItemAt(0, 0), player, chest);
            InventoryPreview.AddPackage(request);
            player.RemoveItem(player.GetItemAt(0, 0));

            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);
            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsFalse(hasPlayerChanges, "Player should not have changes");

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 0),
                item = new TestItem("item A", 5, new Vector2i(0, 0))
            });
        }

        [Test]
        public void RemoveRequest_EmptySlot() {
            chest.CreateItem("item A", 5, 0, 1);
            RequestChestRemove request = new RequestChestRemove(new Vector2i(0, 1), new Vector2i(1, 0), 5, null, chest, player);
            InventoryPreview.AddPackage(request);

            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);
            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasPlayerChanges, "Player should have changes");
            TestSlotPreview(player, playerPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(1, 0),
                item = new TestItem("item A", 5, new Vector2i(1, 0))
            });

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = null
            });
        }

        [Test]
        public void RemoveRequest_Switch() {
            player.CreateItem("item A", 5, 0, 0);
            chest.CreateItem("item B", 5, 0, 1);

            RequestChestRemove request = new RequestChestRemove(new Vector2i(0, 1), new Vector2i(1, 0), 5, player.GetItemAt(0, 0), chest, player);
            InventoryPreview.AddPackage(request);
            player.RemoveItem(player.GetItemAt(0, 0));

            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);
            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasPlayerChanges, "Player should have changes");
            TestSlotPreview(player, playerPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(1, 0),
                item = new TestItem("item B", 5, new Vector2i(1, 0))
            });

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item A", 5, new Vector2i(0, 1))
            });
        }

        [Test]
        public void RemoveRequest_Stack() {
            player.CreateItem("item A", 5, 0, 0);
            chest.CreateItem("item A", 5, 0, 1);
            RequestChestRemove request = new RequestChestRemove(new Vector2i(0, 1), new Vector2i(0, 0), 3, null, chest, player);
            InventoryPreview.AddPackage(request);

            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);
            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasPlayerChanges, "Player should have changes");
            TestSlotPreview(player, playerPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 0),
                item = new TestItem("item A", 8, new Vector2i(0, 0))
            });

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item A", 2, new Vector2i(0, 1))
            });
        }

        [Test]
        public void RemoveRequest_Stack_Twice() {
            chest.CreateItem("item A", 10, 0, 1);

            RequestChestRemove request = new RequestChestRemove(new Vector2i(0, 1), new Vector2i(0, 0), 3, null, chest, player);
            InventoryPreview.AddPackage(request);

            request = new RequestChestRemove(new Vector2i(0, 1), new Vector2i(0, 0), 3, null, chest, player);
            InventoryPreview.AddPackage(request);

            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);
            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasPlayerChanges, "Player should have changes");
            TestSlotPreview(player, playerPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 0),
                item = new TestItem("item A", 6, new Vector2i(0, 0))
            });

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item A", 4, new Vector2i(0, 1))
            });
        }

        [Test]
        public void RemoveRequest_FastMove_NoStack() {
            player.CreateItem("item B", 5, 0, 0);
            chest.CreateItem("item A", 5, 0, 0);

            RequestChestRemove request = new RequestChestRemove(new Vector2i(0, 0), new Vector2i(-1, -1), 5, null, chest, player);
            InventoryPreview.AddPackage(request);

            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);
            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasPlayerChanges, "Player should have changes");
            TestSlotPreview(player, playerPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(1, 0),
                item = new TestItem("item A", 5, new Vector2i(1, 0))
            });

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 0),
                item = null
            });
        }

        [Test]
        public void RemoveRequest_FastMove_Stack() {
            player.CreateItem("item A", 5, 0, 1);
            chest.CreateItem("item A", 5, 1, 0);

            RequestChestRemove request = new RequestChestRemove(new Vector2i(1, 0), new Vector2i(-1, -1), 5, null, chest, player);
            InventoryPreview.AddPackage(request);

            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);
            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasPlayerChanges, "Player should have changes");
            TestSlotPreview(player, playerPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item A", 10, new Vector2i(0, 1))
            });

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(1, 0),
                item = null
            });
        }

        [Test]
        public void RequestRemove_FastMove_Split_Twice() {
            chest.CreateItem("item A", 10, 0, 0);

            RequestChestRemove request = new RequestChestRemove(new Vector2i(0, 0), new Vector2i(-1, -1), 5, null, chest, player);
            InventoryPreview.AddPackage(request);

            request = new RequestChestRemove(new Vector2i(0, 0), new Vector2i(-1, -1), 3, null, chest, player);
            InventoryPreview.AddPackage(request);

            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);
            bool hasPlayerChanges = InventoryPreview.GetChanges(player, out SlotPreview playerPreview);

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 0),
                item = new TestItem("item A", 2, new Vector2i(0, 0))
            });

            Assert.IsTrue(hasPlayerChanges, "Player should have changes");
            TestSlotPreview(player, playerPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 0),
                item = new TestItem("item A", 8, new Vector2i(0, 0))
            });
        }

        [Test]
        public void MoveRequest_EmptySlot() {
            chest.CreateItem("item A", 5, 0, 1);

            RequestMove request = new RequestMove(chest.GetItemAt(0, 1), new Vector2i(1, 0), 5, chest);
            InventoryPreview.AddPackage(request);

            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = null
            }, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(1, 0),
                item = new TestItem("item A", 5, new Vector2i(1, 0))
            });
        }

        [Test]
        public void MoveRequest_EmptySlot_Twice() {
            chest.CreateItem("item A", 5, 0, 1);

            RequestMove request = new RequestMove(chest.GetItemAt(0, 1), new Vector2i(1, 0), 5, chest);
            InventoryPreview.AddPackage(request);
            request = new RequestMove(chest.GetItemAt(0, 1), new Vector2i(1, 0), 5, chest);
            InventoryPreview.AddPackage(request);

            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = null
            }, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(1, 0),
                item = new TestItem("item A", 5, new Vector2i(1, 0))
            });
        }

        [Test]
        public void MoveRequest_Stack_Twice() {
            chest.CreateItem("item A", 5, 0, 1);

            RequestMove request = new RequestMove(chest.GetItemAt(0, 1), new Vector2i(1, 0), 3, chest);
            InventoryPreview.AddPackage(request);

            request = new RequestMove(chest.GetItemAt(0, 1), new Vector2i(1, 0), 1, chest);
            InventoryPreview.AddPackage(request);

            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item A", 1, new Vector2i(0, 1))
            }, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(1, 0),
                item = new TestItem("item A", 4, new Vector2i(1, 0))
            });
        }

        [Test]
        public void MoveRequest_Stack() {
            chest.CreateItem("item A", 5, 0, 1);
            chest.CreateItem("item A", 5, 1, 0);

            RequestMove request = new RequestMove(chest.GetItemAt(0, 1), new Vector2i(1, 0), 3, chest);
            InventoryPreview.AddPackage(request);

            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item A", 2, new Vector2i(0, 1))
            }, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(1, 0),
                item = new TestItem("item A", 8, new Vector2i(1, 0))
            });
        }

        [Test]
        public void MoveRequest_Stack_Overflow() {
            chest.CreateItem("item A", 15, 0, 1);
            chest.CreateItem("item A", 15, 1, 0);

            RequestMove request = new RequestMove(chest.GetItemAt(0, 1), new Vector2i(1, 0), 10, chest);
            InventoryPreview.AddPackage(request);

            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);

            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item A", 10, new Vector2i(0, 1))
            }, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(1, 0),
                item = new TestItem("item A", 20, new Vector2i(1, 0))
            });
        }

        [Test]
        public void MoveRequest_Switch() {
            chest.CreateItem("item A", 3, 0, 1);
            chest.CreateItem("item B", 5, 1, 0);

            RequestMove request = new RequestMove(chest.GetItemAt(0, 1), new Vector2i(1, 0), 3, chest);
            InventoryPreview.AddPackage(request);
            request = new RequestMove(chest.GetItemAt(1, 0), new Vector2i(0, 1), 5, chest);
            InventoryPreview.AddPackage(request);

            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);
            Assert.IsTrue(hasChestChanges, "Chest should have changes");
            TestSlotPreview(chest, chestPreview, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(0, 1),
                item = new TestItem("item B", 5, new Vector2i(0, 1))
            }, new ExpectedSlotPreview {
                hasChanges = true,
                pos = new Vector2i(1, 0),
                item = new TestItem("item A", 3, new Vector2i(1, 0))
            });
        }

        [Test]
        public void MoveRequest_NoSwitchAllowed() {
            chest.CreateItem("item A", 5, 0, 1);
            chest.CreateItem("item B", 5, 1, 0);

            RequestMove request = new RequestMove(chest.GetItemAt(0, 1), new Vector2i(1, 0), 3, chest);
            InventoryPreview.AddPackage(request);

            bool hasChestChanges = InventoryPreview.GetChanges(chest, out SlotPreview chestPreview);
            Assert.False(hasChestChanges, "Chest should not have changes");
        }

        private void TestSlotPreview(Inventory inventory, SlotPreview preview, params ExpectedSlotPreview[] expected) {
            for (int x = 0; x < inventory.m_width; x++) {
                for (int y = 0; y < inventory.m_height; y++) {
                    ExpectedSlotPreview expectedSlotPreview = expected.FirstOrDefault(e => e.pos == new Vector2i(x, y));
                    bool hasChanged = preview.GetSlot(new Vector2i(x, y), out ItemDrop.ItemData previewItem);

                    if (expectedSlotPreview == null) {
                        Assert.IsFalse(hasChanged, $"{inventory.m_name} Slot {x}, {y} should not have changes");
                    } else {
                        Assert.AreEqual(expectedSlotPreview.hasChanges, hasChanged, $"{inventory.m_name} Slot {x}, {y} should have changes");
                        TestForItem(previewItem, expectedSlotPreview.item);
                    }
                }
            }
        }

        private class ExpectedSlotPreview {
            public bool hasChanges;
            public Vector2i pos;
            public TestItem? item;
        }
    }
}
