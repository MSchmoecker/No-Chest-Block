using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    public class CanAddItemTest : ItemTestBase {
        private Inventory player;
        private ItemDrop.ItemData item;

        [SetUp]
        public void Setup() {
            player = new Inventory("player", null, 1, 1);

            player.CreateItem("itemA", 5, 0, 0);
            item = Helper.CreateItem("itemA", 5);
            item.m_gridPos = Vector2i.zero;
        }

        [Test]
        public void CanAddItem_Blocked_SlotEmpty() {
            Helper.CreateContainer().AddItemToChest(player.GetItemAt(0, 0), player, new Vector2i(0, 0), ZDOID.None, 5, true);
            Assert.False(player.CanAddItem(item));
        }

        [Test]
        public void CanAddItem_Blocked_SlotNotEmpty() {
            Helper.CreateContainer().AddItemToChest(player.GetItemAt(0, 0), player, new Vector2i(0, 0), ZDOID.None, 3, true);
            Assert.False(player.CanAddItem(item));
        }

        [Test]
        public void AddItem_Blocked_Empty() {
            Helper.CreateContainer().AddItemToChest(player.GetItemAt(0, 0), player, new Vector2i(0, 0), ZDOID.None, 5, true);
            bool result = player.AddItem(item);
            Assert.False(result);
            Assert.AreEqual(5, item.m_stack);
            Assert.Null(player.GetItemAt(0, 0));
        }

        [Test]
        public void AddItem_Blocked_NotEmpty() {
            Helper.CreateContainer().AddItemToChest(player.GetItemAt(0, 0), player, new Vector2i(0, 0), ZDOID.None, 3, true);
            bool result = player.AddItem(item);
            Assert.False(result);
            Assert.AreEqual(5, item.m_stack);
            TestForItems(player, new TestItem("itemA", 2, new Vector2i(0, 0)));
        }
    }
}
