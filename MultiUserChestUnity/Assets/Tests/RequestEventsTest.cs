using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using MultiUserChest;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnitTests {
    [TestFixture]
    public class RequestEventsTest : ItemTestBase {
        private Inventory playerInv;
        private Inventory containerInv;
        private Inventory groundInv;

        private Container container;
        private ZDOID playerId;

        private List<IRequest> sent;
        private List<(IRequest request, IResponse response)> applied;

        [OneTimeSetUp]
        public void EnsureThreadingHelper() {
            // A refused request is cleaned up later through BepInEx's ThreadingHelper, which the chainloader creates in game.
            if (!ThreadingHelper.Instance) {
                typeof(ThreadingHelper).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
            }
        }

        [SetUp]
        public void Setup() {
            playerInv = new Inventory("player", null, 5, 5);
            containerInv = new Inventory("container", null, 5, 5);
            groundInv = new Inventory("ground", null, 5, 5);

            container = Helper.CreateContainer(containerInv);
            playerId = Helper.CreatePlayerIdToInventory(playerInv);

            // Earlier fixtures may leave routed RPCs queued; their answers would be counted here.
            ZNetSimulate.routedRpcs.Clear();
            sent = new List<IRequest>();
            applied = new List<(IRequest, IResponse)>();
            RequestEvents.RequestSent += OnRequestSent;
            RequestEvents.ResponseApplied += OnResponseApplied;

            Patches.DropPatch.OnDrop += AddItemToGround;
            InventoryBlock.Get(playerInv).ReleaseBlockedSlots();
            InventoryBlock.Get(containerInv).ReleaseBlockedSlots();
        }

        [TearDown]
        public void TearDown() {
            RequestEvents.RequestSent -= OnRequestSent;
            RequestEvents.ResponseApplied -= OnResponseApplied;
            Patches.DropPatch.OnDrop -= AddItemToGround;
            Helper.inventories.Clear();
            ZNetSimulate.routedRpcs.Clear();
        }

        private void OnRequestSent(IRequest request) {
            sent.Add(request);
        }

        private void OnResponseApplied(IRequest request, IResponse response) {
            applied.Add((request, response));
        }

        private void AddItemToGround(ItemDrop.ItemData item, int amount) {
            if (item != null) {
                ItemDrop.ItemData clone = item.Clone();
                clone.m_stack = amount;
                groundInv.AddItem(clone);
            }
        }

        [Test]
        public void AddToChest_RaisesSentThenApplied() {
            playerInv.CreateItem("itemA", 5, 2, 2);

            RequestChestAdd request = container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5);

            Assert.AreEqual(1, sent.Count);
            Assert.AreSame(request, sent[0]);
            Assert.AreEqual(0, applied.Count);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            Assert.AreEqual(1, applied.Count);
            Assert.AreSame(request, applied[0].request);
            Assert.AreEqual(request.RequestID, applied[0].response.SourceID);
            Assert.IsInstanceOf<RequestChestAddResponse>(applied[0].response);
            Assert.IsTrue(applied[0].response.Success);
            Assert.AreEqual(5, applied[0].response.Amount);
            TestForItems(containerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
        }

        [Test]
        public void AddToChest_AlreadyFull_SendsNothing() {
            containerInv = new Inventory("container", null, 1, 1);
            container = Helper.CreateContainer(containerInv);
            playerInv.CreateItem("itemA", 5, 2, 2);
            containerInv.CreateItem("itemB", 5, 0, 0);

            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(-1, -1), playerId, 5);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            Assert.AreEqual(0, sent.Count);
            Assert.AreEqual(0, applied.Count);
            TestForItems(playerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
        }

        [Test]
        public void AddToChest_SameItemAddedByOwnerMeanwhile_ReportsRequestAmountOnly() {
            containerInv = new Inventory("container", null, 1, 1);
            container = Helper.CreateContainer(containerInv);
            playerInv.CreateItem("itemA", 5, 2, 2);

            container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(-1, -1), playerId, 5);

            // The owner fills the only slot before the request arrives, so the request is refused
            // even though the chest now holds more itemA than before.
            containerInv.CreateItem("itemA", 20, 0, 0);
            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            Assert.AreEqual(1, applied.Count);
            Assert.AreEqual(0, applied[0].response.Amount);
            TestForItems(playerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            TestForItems(containerInv, new TestItem("itemA", 20, new Vector2i(0, 0)));
        }

        [Test]
        public void RemoveFromChest_RaisesSentThenApplied() {
            containerInv.CreateItem("itemB", 5, 2, 2);

            RequestChestRemove request = container.RemoveItemFromChest(containerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5);

            Assert.AreEqual(1, sent.Count);
            Assert.AreSame(request, sent[0]);

            container.m_nview.ClaimOwnership();
            ZNetSimulate.HandleAllRoutedRpcs();

            Assert.AreEqual(1, applied.Count);
            Assert.AreSame(request, applied[0].request);
            Assert.IsInstanceOf<RequestChestRemoveResponse>(applied[0].response);
            Assert.IsTrue(applied[0].response.Success);
            Assert.AreEqual(5, applied[0].response.Amount);
            TestForItems(playerInv, new TestItem("itemB", 5, new Vector2i(2, 2)));
        }

        [Test]
        public void InvalidRequest_RaisesNothing() {
            playerInv.CreateItem("itemA", 5, 2, 2);
            ItemDrop.ItemData notInInventory = Helper.CreateItem("itemA", 5);

            container.AddItemToChest(notInInventory, playerInv, new Vector2i(2, 2), playerId, 5);

            Assert.AreEqual(0, sent.Count);
        }

        [Test]
        public void ThrowingHandler_DoesNotBreakTransfer() {
            Action<IRequest> throwOnSent = _ => throw new InvalidOperationException("test");
            Action<IRequest, IResponse> throwOnApplied = (_, __) => throw new InvalidOperationException("test");
            RequestEvents.RequestSent += throwOnSent;
            RequestEvents.ResponseApplied += throwOnApplied;

            try {
                LogAssert.Expect(LogType.Error, new Regex("RequestEvents: RequestSent handler threw"));
                LogAssert.Expect(LogType.Error, new Regex("RequestEvents: ResponseApplied handler threw"));
                playerInv.CreateItem("itemA", 5, 2, 2);

                container.AddItemToChest(playerInv.GetItemAt(2, 2), playerInv, new Vector2i(2, 2), playerId, 5);

                container.m_nview.ClaimOwnership();
                ZNetSimulate.HandleAllRoutedRpcs();

                Assert.AreEqual(1, sent.Count);
                Assert.AreEqual(1, applied.Count);
                TestForItems(playerInv);
                TestForItems(containerInv, new TestItem("itemA", 5, new Vector2i(2, 2)));
            } finally {
                RequestEvents.RequestSent -= throwOnSent;
                RequestEvents.ResponseApplied -= throwOnApplied;
            }
        }
    }
}
