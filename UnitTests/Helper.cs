using UnityEngine;

namespace UnitTests {
    public static class Helper {
        public static ItemDrop.ItemData CreateItem(string name, int amount, int maxAmount) {
            return new ItemDrop.ItemData {
                m_shared = new ItemDrop.ItemData.SharedData {
                    m_name = name,
                    m_maxStackSize = maxAmount
                },
                m_stack = amount
            };
        }

        public static void CreateItem(this Inventory inventory, string itemName, int amount, int x, int y) {
            inventory.AddItem(CreateItem(itemName, amount, 20), amount, x, y);
        }

        public static Container CreateContainer() {
            // cannot instantiate a GameObject correctly, do it the dirty way
            Container container = new Container();
            container.m_inventory = new Inventory("inventory", null, 4, 5);
            container.m_nview = new ZNetView();
            // container.m_nview.
            return container;
        }
    }
}
