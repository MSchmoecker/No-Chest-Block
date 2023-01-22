using UnityEngine;

namespace UnitTests {
    public static class Helper {
        public static ItemDrop.ItemData CreateItem(string name, int amount) {
            return new ItemDrop.ItemData {
                m_shared = new ItemDrop.ItemData.SharedData {
                    m_name = name,
                    m_maxStackSize = 20
                },
                m_stack = amount
            };
        }

        public static void CreateItem(this Inventory inventory, string itemName, int amount, int x, int y) {
            inventory.AddItem(CreateItem(itemName, amount), amount, x, y);
        }

        public static Container CreateContainer(Inventory containerInventory = null) {
            // cannot instantiate a GameObject correctly, do it the dirty way
            Container container = new Container();
            container.m_inventory = containerInventory ?? new Inventory("inventory", null, 4, 5);
            container.m_nview = new ZNetView();
            return container;
        }
    }
}
