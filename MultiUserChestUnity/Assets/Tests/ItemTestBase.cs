using System.Collections.Generic;
using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    public class ItemTestBase {
        public struct TestItem {
            public string name;
            public int stack;
            public Vector2i pos;

            public TestItem(string name, int stack, Vector2i pos) {
                this.name = name;
                this.stack = stack;
                this.pos = pos;
            }
        }

        public static void TestForItems(List<ItemDrop.ItemData> targetItems, params TestItem[] items) {
            Assert.AreEqual(items.Length, targetItems.Count, "Different count of items in inventory");

            for (int i = 0; i < items.Length; i++) {
                TestForItem(targetItems[i], items[i]);
            }
        }

        public static void TestForItems(Inventory target, params TestItem[] items) {
            TestForItems(target.m_inventory, items);
        }

        public static void TestForItem(ItemDrop.ItemData target, TestItem? item) {
            if (item == null) {
                Assert.Null(target);
            } else {
                Assert.NotNull(target, $"item {item.Value.name} should not be null");
                Assert.AreEqual(item.Value.name, target.m_shared.m_name, "Different items");
                Assert.AreEqual(item.Value.stack, target.m_stack, "Different stacks");
                Assert.AreEqual(item.Value.pos, target.m_gridPos, "Different gridPos");
            }
        }

        public static void TestResponse(IResponse response, bool success, int amount) {
            Assert.AreEqual(success, response.Success, "Response success");
            Assert.AreEqual(amount, response.Amount, "Response amount");
        }

        public static void TestAddRequest(RequestChestAdd request, RequestChestAdd original, TestItem dragItem, Vector2i toPos, bool allowSwitch, Inventory sourceInventory, Inventory targetInventory) {
            TestForItem(request.dragItem, dragItem);
            Assert.AreEqual(toPos, request.toPos, "RequestChestAdd toPos");
            Assert.AreEqual(allowSwitch, request.allowSwitch, "RequestChestAdd allowSwitch");
            Assert.AreEqual(sourceInventory, original.SourceInventory, "RequestChestAdd sourceInventory");
            Assert.AreEqual(targetInventory, original.TargetInventory, "RequestChestAdd targetInventory");
        }

        public static void TestRemoveRequest(RequestChestRemove request, RequestChestRemove original, TestItem? switchItem, Vector2i fromPos, Vector2i toPos, int amount, Inventory sourceInventory, Inventory targetInventory) {
            TestForItem(request.switchItem, switchItem);
            Assert.AreEqual(request.fromPos, fromPos, "RequestChestRemove fromPos");
            Assert.AreEqual(request.toPos, toPos, "RequestChestRemove toPos");
            Assert.AreEqual(request.dragAmount, amount, "RequestChestRemove dragAmount");
            Assert.AreEqual(sourceInventory, original.SourceInventory, "RequestChestRemove sourceInventory");
            Assert.AreEqual(targetInventory, original.TargetInventory, "RequestChestRemove targetInventory");
        }

        public static void TestMoveRequest(RequestMove request, RequestMove original, Vector2i fromPos, Vector2i toPos, int amount, string itemName, Inventory inventory) {
            Assert.AreEqual(request.fromPos, fromPos);
            Assert.AreEqual(request.toPos, toPos);
            Assert.AreEqual(request.dragAmount, amount);
            Assert.AreEqual(request.itemHash, itemName.GetStableHashCode());
            Assert.AreEqual(inventory, original.SourceInventory, "RequestMove sourceInventory");
            Assert.AreEqual(inventory, original.TargetInventory, "RequestMove targetInventory");

        }
    }
}
