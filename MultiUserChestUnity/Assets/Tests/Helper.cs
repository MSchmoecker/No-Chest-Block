using System.Collections.Generic;
using MultiUserChest;
using MultiUserChest.Patches;
using UnityEngine;

namespace UnitTests {
    public static class Helper {
        private static ZNet zNet;
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
            GameObject zNetParent = new GameObject("ZNet");
            zNetParent.SetActive(false);

            zNet = zNetParent.AddComponent<ZNet>();
            zNetParent.AddComponent<ZoneSystem>();
            zNetParent.AddComponent<ZNetScene>();

            zNet.m_passwordDialog = (RectTransform)new GameObject("PasswordDialog", typeof(RectTransform)).transform;
            zNet.m_connectingDialog = (RectTransform)new GameObject("PasswordDialog", typeof(RectTransform)).transform;

            zNetParent.SetActive(true);
        }

        public static Container CreateContainer(Inventory containerInventory = null) {
            GameObject containerParent = new GameObject("Container");

            containerParent.AddComponent<ZNetView>();

            Container container = containerParent.AddComponent<Container>();
            container.m_nview.m_zdo.SetOwner(-1);
            container.m_inventory = containerInventory ?? new Inventory("inventory", null, 4, 5);
            container.m_inventory.m_onChanged += container.OnContainerChanged;
            container.RegisterRPCs();

            inventories.Add(container.m_nview.m_zdo.m_uid, container.m_inventory);
            Log.LogInfo("Created container with id: " + container.m_nview.m_zdo.m_uid);

            return container;
        }

        public static ZDOID CreatePlayerIdToInventory(Inventory playerInventory) {
            ZDO zdo = zNet.m_zdoMan.CreateNewZDO(new Vector3());

            inventories.Add(zdo.m_uid, playerInventory);

            return zdo.m_uid;
        }
    }
}