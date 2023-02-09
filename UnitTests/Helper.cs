using System.Collections.Generic;
using MultiUserChest.Patches;
using UnityEngine;

namespace UnitTests {
    public static class Helper {
        private static ZDOMan zdoMan;
        public static Dictionary<ZDOID, Inventory> inventories = new Dictionary<ZDOID, Inventory>();

        public static ItemDrop.ItemData CreateItem(string name, int amount, int x = 0, int y = 0) {
            return new ItemDrop.ItemData {
                m_shared = new ItemDrop.ItemData.SharedData {
                    m_name = name,
                    m_maxStackSize = 20,
                },
                m_stack = amount,
                m_gridPos = new Vector2i(x, y),
            };
        }

        public static ItemDrop.ItemData CreateItem(this Inventory inventory, string itemName, int amount, int x, int y) {
            inventory.AddItem(CreateItem(itemName, amount), amount, x, y);
            return inventory.GetItemAt(x, y);
        }

        static Helper() {
            // cannot instantiate a GameObject correctly, do it the dirty way
            ZNet.m_instance = new ZNet();
            ZoneSystem.m_instance = new ZoneSystem();
            ZNetScene.m_instance = new ZNetScene();

            zdoMan = new ZDOMan(512);
            zdoMan.m_objectsByID = new Dictionary<ZDOID, ZDO>();
        }

        public static Container CreateContainer(Inventory containerInventory = null) {
            // cannot instantiate a GameObject correctly, do it the dirty way
            Container container = new Container();
            container.m_inventory = containerInventory ?? new Inventory("inventory", null, 4, 5);
            container.m_nview = new ZNetView();
            container.m_nview.m_zdo = zdoMan.CreateNewZDO(new Vector3());
            container.RegisterRPCs();

            inventories.Add(container.m_nview.m_zdo.m_uid, container.m_inventory);

            return container;
        }

        public static ZDOID CreatePlayerIdToInventory(Inventory playerInventory) {
            ZDO zdo = zdoMan.CreateNewZDO(new Vector3());

            inventories.Add(zdo.m_uid, playerInventory);

            return zdo.m_uid;
        }
    }
}
