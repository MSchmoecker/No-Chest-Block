using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace NoChestBlock.Patches.Compatibility {
    public class QuickStackPatch {
        [HarmonyPatch("QuickStack.QuickStackPlugin, QuickStack", "StackToMany"), HarmonyPrefix]
        public static bool StackToManyPatch(Player player, List<Container> containers) {
            long playerID = player.GetPlayerID();
            int moved = 0;

            foreach (Container container in containers) {
                if (container.m_checkGuardStone && !PrivateArea.CheckAccess(container.transform.position) || !container.CheckAccess(playerID)) {
                    continue;
                }

                moved = CallStackItems(player, container, moved);
            }

            ReportResult(player, moved);
            return false;
        }

        [HarmonyPatch("QuickStack.QuickStackPlugin, QuickStack", "DoQuickStack"), HarmonyPrefix]
        public static bool DoQuickStackPatch(Player player) {
            if (player.IsTeleporting()) {
                return false;
            }

            Container container = InventoryGui.instance.m_currentContainer;

            if (container && container.m_nview.IsValid()) {
                int moved = CallStackItems(player, container, 0);
                ReportResult(player, moved);
                return false;
            }

            List<Container> nearbyContainers = FindNearbyContainers(player.transform.position);
            if (nearbyContainers.Count != 0) {
                StackToManyPatch(player, nearbyContainers);
            }

            return false;
        }

        private static int CallStackItems(Player player, Container container, int moved) {
            long playerID = player.GetPlayerID();
            Inventory playerInventory = player.GetInventory();

            if (container.IsOwner()) {
                moved += StackItems(playerInventory, container.GetInventory(), playerID);
            } else {
                void StackItemsNoReturn(Inventory from, Inventory to) {
                    StackItems(@from, to, playerID);
                }

                List<ItemDrop.ItemData> movable = InventoryHelper.GetAllMoveableItems(playerInventory, container.GetInventory(), StackItemsNoReturn);
                moved += movable.Count;

                foreach (ItemDrop.ItemData item in movable) {
                    ContainerHandler.AddItemToChest(item.m_gridPos, new Vector2i(-1, -1), item.m_stack, true, playerInventory, container);
                }
            }

            return moved;
        }

        private static void ReportResult(Player player, int moved) {
            ReflectionHelper.InvokeStaticMethod<object>("QuickStack.QuickStackPlugin, QuickStack", "reportResult", player, moved);
        }

        private static int StackItems(Inventory from, Inventory to, long playerID) {
            return ReflectionHelper.InvokeStaticMethod<int>("QuickStack.QuickStackPlugin, QuickStack", "StackItems", playerID, from, to);
        }

        private static List<Container> FindNearbyContainers(Vector3 point) {
            return ReflectionHelper.InvokeStaticMethod<List<Container>>("QuickStack.QuickStackPlugin, QuickStack", "FindNearbyContainers", point);
        }
    }
}
