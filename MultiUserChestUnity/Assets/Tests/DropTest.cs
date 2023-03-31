using System.Collections;
using System.Collections.Generic;
using MultiUserChest;
using NUnit.Framework;
using UnitTests;
using UnityEngine;

namespace UnitTests {
    [TestFixture]
    public class DropTest : ItemTestBase {
        private Inventory inventory;

        [SetUp]
        public void SetUp() {
            inventory = new Inventory("inventory", null, 4, 5);
        }

        [Test]
        public void RequestDropAll() {
            inventory.CreateItem("item", 5, 2, 3);

            RequestDrop request = new RequestDrop(new Vector2i(2, 3), 5, ZDOID.None);
            RequestDropResponse response = inventory.RequestDrop(request);

            TestResponse(response, true, 5);
            TestForItem(response.responseItem, new TestItem("item", 5, new Vector2i(2, 3)));
            TestForItems(inventory);
        }

        [Test]
        public void RequestDropSome() {
            inventory.CreateItem("item", 5, 2, 3);

            RequestDrop request = new RequestDrop(new Vector2i(2, 3), 3, ZDOID.None);
            RequestDropResponse response = inventory.RequestDrop(request);

            TestResponse(response, true, 3);
            TestForItem(response.responseItem, new TestItem("item", 3, new Vector2i(2, 3)));
            TestForItems(inventory, new TestItem("item", 2, new Vector2i(2, 3)));
        }

        [Test]
        public void RequestDropNone() {
            inventory.CreateItem("item", 5, 2, 3);

            RequestDrop request = new RequestDrop(new Vector2i(2, 3), 0, ZDOID.None);
            RequestDropResponse response = inventory.RequestDrop(request);

            TestResponse(response, false, 0);
            TestForItem(response.responseItem, null);
            TestForItems(inventory, new TestItem("item", 5, new Vector2i(2, 3)));
        }

        [Test]
        public void DropNoneExisting() {
            RequestDrop request = new RequestDrop(new Vector2i(2, 3), 5, ZDOID.None);
            RequestDropResponse response = inventory.RequestDrop(request);

            TestResponse(response, false, 0);
            TestForItem(response.responseItem, null);
            TestForItems(inventory);
        }
    }
}
