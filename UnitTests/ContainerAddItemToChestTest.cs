using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ContainerAddItemToChestTest {
        private Inventory player;
        private Inventory container;
        private Container chest;

        [SetUp]
        public void Setup() {
            player = new Inventory("player", null, 2, 1);
            container = new Inventory("container", null, 2, 1);
            chest = Helper.CreateContainer();
            chest.m_inventory = container;
            InventoryBlock.Get(player).ReleaseBlockedSlots();
            InventoryBlock.Get(container).ReleaseBlockedSlots();
        }

        [Test]
        public void Possible_DragAmount_NegativeAmount() {
            player.CreateItem("my item", 20, 0, 0);

            int amount = ContainerHandler.PossibleDragAmount(chest, player.GetItemAt(0, 0), new Vector2i(0, 0), -1);

            Assert.AreEqual(20, amount);
        }

        [Test]
        public void Possible_DragAmount_TooMuchAmount() {
            player.CreateItem("my item", 10, 0, 0);

            int amount = ContainerHandler.PossibleDragAmount(chest, player.GetItemAt(0, 0), new Vector2i(0, 0), 15);

            Assert.AreEqual(10, amount);
        }

        [Test]
        public void Possible_DragAmount_SpecificSlot_Empty() {
            player.CreateItem("my item", 20, 0, 0);

            int amount = ContainerHandler.PossibleDragAmount(chest, player.GetItemAt(0, 0), new Vector2i(0, 0), 10);

            Assert.AreEqual(10, amount);
        }

        [Test]
        public void Possible_DragAmount_SpecificSlot_AllPossible() {
            player.CreateItem("my item", 20, 0, 0);
            container.CreateItem("my item", 5, 0, 0);

            int amount = ContainerHandler.PossibleDragAmount(chest, player.GetItemAt(0, 0), new Vector2i(0, 0), 10);

            Assert.AreEqual(10, amount);
        }

        [Test]
        public void Possible_DragAmount_SpecificSlot_NotAllPossible() {
            player.CreateItem("my item", 20, 0, 0);
            container.CreateItem("my item", 15, 0, 0);

            int amount = ContainerHandler.PossibleDragAmount(chest, player.GetItemAt(0, 0), new Vector2i(0, 0), 10);

            Assert.AreEqual(5, amount);
        }

        [Test]
        public void Possible_DragAmount_SpecificSlot_NoMovePossible() {
            player.CreateItem("my item", 20, 0, 0);
            container.CreateItem("my item", 20, 0, 0);

            int amount = ContainerHandler.PossibleDragAmount(chest, player.GetItemAt(0, 0), new Vector2i(0, 0), 10);

            Assert.AreEqual(0, amount);
        }

        [Test]
        public void Possible_DragAmount_AnySlot_EmptySlot() {
            player.CreateItem("my item", 20, 0, 0);
            container.CreateItem("my item", 15, 0, 0);

            int amount = ContainerHandler.PossibleDragAmount(chest, player.GetItemAt(0, 0), new Vector2i(-1, -1), 15);

            Assert.AreEqual(15, amount);
        }

        [Test]
        public void Possible_DragAmount_AnySlot_StackableSlots() {
            player.CreateItem("my item", 20, 0, 0);
            container.CreateItem("my item", 15, 0, 0);
            container.CreateItem("my item", 15, 1, 0);

            int amount = ContainerHandler.PossibleDragAmount(chest, player.GetItemAt(0, 0), new Vector2i(-1, -1), 10);

            Assert.AreEqual(10, amount);
        }

        [Test]
        public void Possible_DragAmount_AnySlot_StackableSlots_Overflow() {
            player.CreateItem("my item", 20, 0, 0);
            container.CreateItem("my item", 15, 0, 0);
            container.CreateItem("my item", 15, 1, 0);

            int amount = ContainerHandler.PossibleDragAmount(chest, player.GetItemAt(0, 0), new Vector2i(-1, -1), 15);
            Assert.AreEqual(10, amount);
        }
    }
}
