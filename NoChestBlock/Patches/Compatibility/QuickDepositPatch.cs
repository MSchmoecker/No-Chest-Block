using System.Collections.Generic;
using HarmonyLib;

namespace NoChestBlock.Patches.Compatibility {
    public static class QuickDepositPatch {
        [HarmonyPatch("MaGic.QuickDeposit, QuickDeposit", "DepositAll"), HarmonyPrefix]
        public static bool DepositAllPatch() {
            Container container = InventoryGui.instance.m_currentContainer;

            if (!container) {
                return true;
            }

            if (container.IsOwner()) {
                Log.LogDebug("DepositAll in own inventory");
                return true;
            }

            Inventory playerInventory = Player.m_localPlayer.GetInventory();
            Inventory containerInventory = container.GetInventory();

            List<ItemDrop.ItemData> movable = InventoryHelper.GetAllMoveableItems(playerInventory, containerInventory, MoveAllStackableItems);

            foreach (ItemDrop.ItemData item in movable) {
                ContainerHandler.AddItemToChest(container, playerInventory, Player.m_localPlayer.GetZDOID(), item.m_gridPos, new Vector2i(-1, -1), item.m_stack, true);
            }

            return false;
        }

        public static void MoveAllStackableItems(Inventory from, Inventory to) {
            List<ItemDrop.ItemData> fromItems = from.GetAllItems();

            for (int i = fromItems.Count - 1; i >= 0; i--) {
                if (fromItems[i].m_shared.m_maxStackSize <= 1) {
                    continue;
                }

                foreach (ItemDrop.ItemData item in to.GetAllItems()) {
                    if (item.m_shared.m_name == fromItems[i].m_shared.m_name) {
                        to.MoveItemToThis(from, fromItems[i]);
                        break;
                    }
                }
            }
        }
    }
}
