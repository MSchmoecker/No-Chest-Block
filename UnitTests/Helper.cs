using UnityEngine;

namespace UnitTests {
    public class Helper {
        public static ItemDrop.ItemData CreateItem(string name, int amount, int maxAmount) {
            return new ItemDrop.ItemData {
                m_shared = new ItemDrop.ItemData.SharedData {
                    m_name = name,
                    m_maxStackSize = maxAmount
                },
                m_stack = amount
            };
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
