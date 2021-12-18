using System;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace ChestFix.Patches {
    [HarmonyPatch]
    public class ContainerPatch {
        [HarmonyPatch(typeof(Container), nameof(Container.IsInUse)), HarmonyPostfix]
        public static void IsInUsePatch(ref bool __result, ref bool ___m_inUse) {
            __result = false;
            ___m_inUse = false;
        }

        [HarmonyPatch(typeof(Container), nameof(Container.Awake)), HarmonyPostfix]
        public static void ContainerAwakePatch(Container __instance) {
            __instance.m_nview.Register<ZPackage>("RequestItemMove",
                                                  (sender, package) => ContainerHandler.RPC_RequestItemMove(__instance, sender, package));
            __instance.m_nview.Register<ZPackage>("RequestItemAdd",
                                                  (sender, package) => ContainerHandler.RPC_RequestItemAdd(__instance, sender, package));
            __instance.m_nview.Register<ZPackage>("RequestItemRemove",
                                                  (sender, package) => ContainerHandler.RPC_RequestItemRemove(__instance, sender, package));
            __instance.m_nview.Register<bool>("RequestItemMoveResponse", (l, b) => { InventoryGui.instance.SetupDragItem(null, null, 0); });
            __instance.m_nview.Register<bool>("RequestItemAddResponse", RPC_RequestItemAddResponse);

            __instance.m_nview.Register<bool>("RequestItemRemoveResponse", RPC_RequestItemRemoveResponse);
        }

        private static void RPC_RequestItemRemoveResponse(long sender, bool success) {
            if (success) {
                lastGrid.m_inventory.AddItem(InventoryGui.instance.m_dragItem);
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);
        }

        private static void RPC_RequestItemAddResponse(long sender, bool success) {
            if (success) {
                InventoryGui.instance.m_dragInventory.RemoveItem(InventoryGui.instance.m_dragItem);
            }

            InventoryGui.instance.SetupDragItem(null, null, 0);
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update)), HarmonyPrefix]
        public static void InventoryGuiUpdatePatch(InventoryGui __instance) {
            if (__instance.m_currentContainer) {
                __instance.m_currentContainer.CheckForChanges();
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateContainer)), HarmonyPrefix]
        public static bool UpdateContainerPatch(InventoryGui __instance, Player player) {
            if (!__instance.m_animator.GetBool("visible")) {
                return false;
            }

            if ((bool)__instance.m_currentContainer /*&& __instance.m_currentContainer.IsOwner()*/) {
                __instance.m_currentContainer.SetInUse(inUse: true);
                __instance.m_container.gameObject.SetActive(value: true);
                __instance.m_containerGrid.UpdateInventory(__instance.m_currentContainer.GetInventory(), null,
                                                           __instance.m_dragItem);
                __instance.m_containerName.text = Localization.instance.Localize(__instance.m_currentContainer.GetInventory().GetName());
                if (__instance.m_firstContainerUpdate) {
                    __instance.m_containerGrid.ResetView();
                    __instance.m_firstContainerUpdate = false;
                }

                if (Vector3.Distance(__instance.m_currentContainer.transform.position, player.transform.position) >
                    __instance.m_autoCloseDistance) {
                    __instance.CloseContainer();
                }
            } else {
                __instance.m_container.gameObject.SetActive(value: false);
                if (__instance.m_dragInventory != null && __instance.m_dragInventory != Player.m_localPlayer.GetInventory()) {
                    __instance.SetupDragItem(null, null, 1);
                }
            }

            return false;
        }

        /*[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem)), HarmonyPrefix]
        public static void OnSelectedItemPatch(InventoryGui __instance) {
            if (!__instance.m_currentContainer.IsOwner()) {
                Logger.LogInfo("Claimed ownership for inventory!");
                long owner = __instance.m_currentContainer.m_nview.GetZDO().m_owner;
                __instance.m_currentContainer.m_nview.ClaimOwnership();
                ZDOMan.instance.ForceSendZDO(owner, __instance.m_currentContainer.m_nview.GetZDO().m_uid);

            }
        }*/

        /*[HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.DropItem)), HarmonyPrefix]
        public static void OnSelectedItemPatch(InventoryGrid __instance) {
            if (!__instance.m_inventory.IsOwner()) {
                __instance.dr
                Logger.LogInfo("Claimed ownership for inventory!");
                long owner = __instance.m_currentContainer.m_nview.GetZDO().m_owner;
                __instance.m_currentContainer.m_nview.ClaimOwnership();
                ZDOMan.instance.ForceSendZDO(owner, __instance.m_currentContainer.m_nview.GetZDO().m_uid);
            }
        }*/

        private static InventoryGrid lastGrid;

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem)), HarmonyPrefix]
        public static void OnSelectedItemPatch(InventoryGrid grid) {
            lastGrid = grid;
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem)), HarmonyPrefix]
        public static bool OnSelectedItemPatch(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos,
            InventoryGrid.Modifier mod) {
            Player localPlayer = Player.m_localPlayer;
            if (localPlayer.IsTeleporting()) {
                return false;
            }

            if ((bool)__instance.m_dragGo) {
                __instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);
                bool flag = localPlayer.IsItemEquiped(__instance.m_dragItem);
                bool flag2 = item != null && localPlayer.IsItemEquiped(item);
                Vector2i gridPos = __instance.m_dragItem.m_gridPos;
                if ((__instance.m_dragItem.m_shared.m_questItem || (item != null && item.m_shared.m_questItem)) &&
                    __instance.m_dragInventory != grid.GetInventory()) {
                    return false;
                }

                if (!__instance.m_dragInventory.ContainsItem(__instance.m_dragItem)) {
                    __instance.SetupDragItem(null, null, 1);
                    return false;
                }

                localPlayer.RemoveFromEquipQueue(item);
                localPlayer.RemoveFromEquipQueue(__instance.m_dragItem);
                localPlayer.UnequipItem(__instance.m_dragItem, triggerEquipEffects: false);
                localPlayer.UnequipItem(item, triggerEquipEffects: false);

                if (!__instance.m_currentContainer.IsOwner() &&
                    (grid.GetInventory() != localPlayer.m_inventory || __instance.m_dragInventory != localPlayer.m_inventory)) {
                    // Logger.LogInfo($"from container {__instance.m_currentContainer.m_nview.GetZDO().m_uid}");
                    // Logger.LogInfo($"m_dragItem pos {__instance.m_dragItem.m_gridPos}");
                    // Logger.LogInfo($"pos {pos}");
                    // Logger.LogInfo($"m_dragAmount {__instance.m_dragAmount}");

                    if (grid.GetInventory() == __instance.m_dragInventory) {
                        ZPackage data = new ZPackage();
                        data.Write(__instance.m_dragItem.m_gridPos);
                        data.Write(pos);
                        data.Write(__instance.m_dragAmount);

                        Logger.LogInfo("RequestItemMove");
                        __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemMove", data);
                    } else if (__instance.m_dragInventory != __instance.m_currentContainer.GetInventory()) {
                        ZPackage data = new ZPackage();
                        data.Write(localPlayer.GetPlayerID());
                        data.Write(__instance.m_dragItem.m_gridPos);
                        data.Write(pos);
                        data.Write(__instance.m_dragAmount);
                        WriteItemToPackage(__instance.m_dragItem, data);

                        Logger.LogInfo("RequestItemAdd");
                        __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemAdd", data);
                    } else {
                        ZPackage data = new ZPackage();
                        data.Write(localPlayer.GetPlayerID());
                        data.Write(__instance.m_dragItem.m_gridPos);
                        data.Write(pos);
                        data.Write(__instance.m_dragAmount);
                        WriteItemToPackage(__instance.m_dragItem, data);

                        Logger.LogInfo("RequestItemRemove");
                        __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemRemove", data);
                    }

                    return false;
                }

                bool num = grid.DropItem(__instance.m_dragInventory, __instance.m_dragItem, __instance.m_dragAmount, pos);
                if (__instance.m_dragItem.m_stack < __instance.m_dragAmount) {
                    __instance.m_dragAmount = __instance.m_dragItem.m_stack;
                }

                if (flag) {
                    ItemDrop.ItemData itemAt = grid.GetInventory().GetItemAt(pos.x, pos.y);
                    if (itemAt != null) {
                        localPlayer.EquipItem(itemAt, triggerEquipEffects: false);
                    }

                    if (localPlayer.GetInventory().ContainsItem(__instance.m_dragItem)) {
                        localPlayer.EquipItem(__instance.m_dragItem, triggerEquipEffects: false);
                    }
                }

                if (flag2) {
                    ItemDrop.ItemData itemAt2 = __instance.m_dragInventory.GetItemAt(gridPos.x, gridPos.y);
                    if (itemAt2 != null) {
                        localPlayer.EquipItem(itemAt2, triggerEquipEffects: false);
                    }

                    if (localPlayer.GetInventory().ContainsItem(item)) {
                        localPlayer.EquipItem(item, triggerEquipEffects: false);
                    }
                }

                if (num) {
                    __instance.SetupDragItem(null, null, 1);
                    __instance.UpdateCraftingPanel();
                }
            } else {
                if (item == null) {
                    return false;
                }

                switch (mod) {
                    case InventoryGrid.Modifier.Move:
                        if (item.m_shared.m_questItem) {
                            return false;
                        }

                        if (__instance.m_currentContainer != null) {
                            localPlayer.RemoveFromEquipQueue(item);
                            localPlayer.UnequipItem(item);

                            if (grid.GetInventory() == __instance.m_currentContainer.GetInventory()) {
                                localPlayer.GetInventory().MoveItemToThis(grid.GetInventory(), item);
                            } else {
                                __instance.m_currentContainer.GetInventory().MoveItemToThis(localPlayer.GetInventory(), item);
                            }

                            __instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);
                        } else if (Player.m_localPlayer.DropItem(localPlayer.GetInventory(), item, item.m_stack)) {
                            __instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);
                        }

                        return false;
                    case InventoryGrid.Modifier.Split:
                        if (item.m_stack > 1) {
                            __instance.ShowSplitDialog(item, grid.GetInventory());
                            return false;
                        }

                        break;
                }

                __instance.SetupDragItem(item, grid.GetInventory(), item.m_stack);
            }

            return false;
        }

        private static void WriteItemToPackage(ItemDrop.ItemData itemData, ZPackage pkg) {
            if (itemData.m_dropPrefab == null) {
                ZLog.Log("Item missing prefab " + itemData.m_shared.m_name);
                pkg.Write("");
            } else
                pkg.Write(itemData.m_dropPrefab.name);

            pkg.Write(itemData.m_stack);
            pkg.Write(itemData.m_durability);
            pkg.Write(itemData.m_equiped);
            pkg.Write(itemData.m_quality);
            pkg.Write(itemData.m_variant);
            pkg.Write(itemData.m_crafterID);
            pkg.Write(itemData.m_crafterName);
        }
    }
}
