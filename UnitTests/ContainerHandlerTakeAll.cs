using System.Collections.Generic;
using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerTakeAll : ItemTestBase {
        private Inventory container;

        [SetUp]
        public void SetUp() {
            container = new Inventory("inventory", null, 4, 5);
        }

        [Test]
        public void TakeAll_Success() {
            container.CreateItem("my item", 10, 2, 3);

            List<ItemDrop.ItemData> wantedItems = new List<ItemDrop.ItemData>() {
                Helper.CreateItem("my item", 10, 2, 3)
            };

            RequestTakeAll request = new RequestTakeAll(wantedItems);
            RequestTakeAll response = container.RequestTakeAllItems(request);

            TestForItems(response.items, new TestItem[] {
                new TestItem("my item", 10, new Vector2i(2, 3)),
            });

            TestForItems(container);
        }

        [Test]
        public void TakeAll_TooMuchRequested() {
            container.CreateItem("my item", 5, 2, 3);

            List<ItemDrop.ItemData> wantedItems = new List<ItemDrop.ItemData>() {
                Helper.CreateItem("my item", 10, 2, 3),
            };

            RequestTakeAll request = new RequestTakeAll(wantedItems);
            RequestTakeAll response = container.RequestTakeAllItems(request);

            TestForItems(response.items, new TestItem[] {
                new TestItem("my item", 5, new Vector2i(2, 3)),
            });

            TestForItems(container);
        }

        [Test]
        public void TakeAll_LessRequested() {
            container.CreateItem("my item", 5, 2, 3);

            List<ItemDrop.ItemData> wantedItems = new List<ItemDrop.ItemData>() {
                Helper.CreateItem("my item", 3, 2, 3),
            };

            RequestTakeAll request = new RequestTakeAll(wantedItems);
            RequestTakeAll response = container.RequestTakeAllItems(request);

            TestForItems(response.items, new TestItem[] {
                new TestItem("my item", 3, new Vector2i(2, 3)),
            });

            TestForItems(container, new TestItem[] {
                new TestItem("my item", 2, new Vector2i(2, 3)),
            });
        }

        [Test]
        public void TakeAll_MultipleItems() {
            container.CreateItem("my item A", 5, 2, 3);
            container.CreateItem("my item B", 10, 2, 4);

            List<ItemDrop.ItemData> wantedItems = new List<ItemDrop.ItemData>() {
                Helper.CreateItem("my item A", 5, 2, 3),
                Helper.CreateItem("my item B", 10, 2, 4),
            };

            RequestTakeAll request = new RequestTakeAll(wantedItems);
            RequestTakeAll response = container.RequestTakeAllItems(request);

            TestForItems(response.items, new TestItem[] {
                new TestItem("my item A", 5, new Vector2i(2, 3)),
                new TestItem("my item B", 10, new Vector2i(2, 4)),
            });

            TestForItems(container);
        }
    }
}
