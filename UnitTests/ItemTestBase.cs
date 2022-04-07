using NoChestBlock;
using NUnit.Framework;

namespace UnitTests {
    public class ItemTestBase {
        public struct TestItem {
            public string name;
            public int stack;

            public TestItem(string name, int stack) {
                this.name = name;
                this.stack = stack;
            }
        }

        public static void TestForItems(Inventory target, params TestItem[] items) {
            Assert.AreEqual(items.Length, target.m_inventory.Count);

            for (int i = 0; i < items.Length; i++) {
                TestForItem(target.m_inventory[i], items[i]);
            }
        }

        public static void TestForItem(ItemDrop.ItemData target, TestItem? item) {
            if (item == null) {
                Assert.Null(target);
            } else {
                Assert.NotNull(target);
                Assert.AreEqual(item.Value.name, target.m_shared.m_name);
                Assert.AreEqual(item.Value.stack, target.m_stack);
            }
        }

        public static void TestResponse(IResponse response, bool success, int amount) {
            Assert.AreEqual(success, response.Success);
            Assert.AreEqual(amount, response.Amount);
        }
    }
}
