using HarmonyLib;
using UnityEngine;

namespace NoChestBlock.Patches {
    [HarmonyPatch]
    public class InventoryGuiPatch {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update)), HarmonyPrefix]
        public static void InventoryGuiUpdatePatch(InventoryGui __instance) {
            if (__instance.m_currentContainer) {
                __instance.m_currentContainer.CheckForChanges();
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnTakeAll)), HarmonyPrefix]
        public static bool InventoryGuiTakeAllPatch(InventoryGui __instance) {
            if (Player.m_localPlayer.IsTeleporting() || !__instance.m_currentContainer) {
                return false;
            }

            if (__instance.m_currentContainer.IsOwner()) {
                return true;
            }

            ContainerHandler.TakeAll(__instance.m_currentContainer);
            return false;
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnRightClickItem)), HarmonyPrefix]
        public static bool InventoryGuiOnRightClickItemPatch(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item) {
            if (item == null || !Player.m_localPlayer) {
                return true;
            }

            if (grid.GetInventory() == Player.m_localPlayer.GetInventory()) {
                return true;
            }

            if (!__instance.m_currentContainer || __instance.m_currentContainer.IsOwner()) {
                return true;
            }

            if (InventoryHandler.blockConsume) {
                return false;
            }

            if (Player.m_localPlayer.CanConsumeItem(item)) {
                InventoryHandler.blockConsume = true;
                RequestConsume request = new RequestConsume(item);

                Timer.Start(request);
                __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemConsume", request.WriteToPackage());
            }

            return false;
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
                __instance.m_containerName.text =
                    Localization.instance.Localize(__instance.m_currentContainer.GetInventory().GetName());
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

                if (!__instance.m_currentContainer.IsOwner() && !(grid.GetInventory() == localPlayer.m_inventory && __instance.m_dragInventory == localPlayer.m_inventory)) {

                    if (grid.GetInventory() == __instance.m_dragInventory) {
                        RequestMove request = new RequestMove(__instance.m_dragItem.m_gridPos, pos, __instance.m_dragAmount);

                        Timer.Start(request);
                        __instance.m_currentContainer.m_nview.InvokeRPC("RequestItemMove", request.WriteToPackage());
                    } else if (grid.m_inventory == __instance.m_currentContainer.GetInventory()) {
                        RequestAdd request = new RequestAdd(__instance.m_dragItem.m_gridPos, pos,
                                                            __instance.m_dragAmount, __instance.m_dragItem, true);
                        ContainerHandler.AddItemToChest(request, localPlayer.GetInventory(), __instance.m_currentContainer);
                    } else {
                        ItemDrop.ItemData prevItem = grid.GetInventory().GetItemAt(pos.x, pos.y);

                        RequestRemove request = new RequestRemove(__instance.m_dragItem.m_gridPos, pos,
                                                                  __instance.m_dragAmount, prevItem);

                        ContainerHandler.RemoveItemFromChest(request, __instance.m_currentContainer);
                    }

                    return false;
                }

                Log.LogInfo("Move inside own inventory");
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
                                if (localPlayer.GetInventory().CanAddItem(item)) {
                                    Vector2i targetSlot =
                                        InventoryHelper.FindEmptySlot(localPlayer.GetInventory(), InventoryHandler.blockedSlots);

                                    if (targetSlot.x != -1 && targetSlot.y != -1) {
                                        RequestRemove request = new RequestRemove(pos, targetSlot, item
                                                                                      .m_stack, null);

                                        ContainerHandler.RemoveItemFromChest(request, __instance.m_currentContainer);
                                    }
                                }
                            } else {
                                Vector2i targetPos = __instance.m_currentContainer.GetInventory().FindEmptySlot(true);
                                RequestAdd request = new RequestAdd(pos, targetPos, item.m_stack, item, false);

                                ContainerHandler.AddItemToChest(request, localPlayer.GetInventory(), __instance.m_currentContainer);
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
    }
}
