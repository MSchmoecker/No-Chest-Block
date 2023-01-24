using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerMoveTest : ItemTestBase {
        private Inventory container;

        [SetUp]
        public void SetUp() {
            container = new Inventory("inventory", null, 4, 5);
        }

        [Test]
        public void MoveItem_EmptySlot() {
            container.CreateItem("item A", 10, 2, 3);

            RequestMove request = new RequestMove(new Vector2i(2, 3), new Vector2i(3, 3), 10);
            bool response = container.RequestItemMove(request);

            Assert.True(response);
            TestForItems(container, new TestItem("item A", 10, new Vector2i(3, 3)));
        }

        [Test]
        public void MoveItem_SameSlot() {
            container.CreateItem("item A", 10, 2, 3);

            RequestMove request = new RequestMove(new Vector2i(2, 3), new Vector2i(2, 3), 10);
            bool response = container.RequestItemMove(request);

            Assert.True(response);
            TestForItems(container, new TestItem("item A", 10, new Vector2i(2, 3)));
        }

        [Test]
        public void MoveItem_SameSlot_Stacked() {
            container.CreateItem("item A", 10, 2, 3);
            container.CreateItem("item A", 10, 3, 3);

            RequestMove request = new RequestMove(new Vector2i(2, 3), new Vector2i(3, 3), 10);
            bool response = container.RequestItemMove(request);

            Assert.True(response);
            TestForItems(container, new TestItem("item A", 20, new Vector2i(3, 3)));
        }

        [Test]
        public void MoveItem_SameSlot_Stacked_Overflow() {
            container.CreateItem("item A", 10, 2, 3);
            container.CreateItem("item A", 15, 3, 3);

            RequestMove request = new RequestMove(new Vector2i(2, 3), new Vector2i(3, 3), 10);
            bool response = container.RequestItemMove(request);

            Assert.True(response);
            TestForItems(container, new TestItem[] {
                new TestItem("item A", 5, new Vector2i(2, 3)),
                new TestItem("item A", 20, new Vector2i(3, 3))
            });
        }

        [Test]
        public void MoveItem_NoItem() {
            RequestMove request = new RequestMove(new Vector2i(2, 3), new Vector2i(3, 3), 10);
            bool response = container.RequestItemMove(request);

            Assert.False(response);
            TestForItems(container);
        }

        [Test]
        public void MoveItem_Switch() {
            container.CreateItem("item A", 10, 2, 3);
            container.CreateItem("item B", 10, 3, 3);

            RequestMove request = new RequestMove(new Vector2i(2, 3), new Vector2i(3, 3), 10);
            bool response = container.RequestItemMove(request);

            Assert.True(response);
            TestForItems(container, new TestItem[] {
                new TestItem("item B", 10, new Vector2i(2, 3)),
                new TestItem("item A", 10, new Vector2i(3, 3))
            });
        }

        [Test]
        public void MoveItem_Switch_NotPossible() {
            container.CreateItem("item A", 10, 2, 3);
            container.CreateItem("item B", 10, 3, 3);

            RequestMove request = new RequestMove(new Vector2i(2, 3), new Vector2i(3, 3), 5);
            bool response = container.RequestItemMove(request);

            Assert.False(response);
            TestForItems(container, new TestItem[] {
                new TestItem("item A", 10, new Vector2i(2, 3)),
                new TestItem("item B", 10, new Vector2i(3, 3))
            });
        }
    }
}
