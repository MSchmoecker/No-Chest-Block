using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace MultiUserChest.Patches {
    [HarmonyPatch]
    public static class InventoryGuiPatch {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update)), HarmonyPostfix]
        public static void InventoryGuiUpdatePatch(InventoryGui __instance) {
            if (__instance.m_currentContainer && __instance.m_currentContainer.m_nview && __instance.m_currentContainer.m_nview.IsValid()) {
                __instance.m_currentContainer.CheckForChanges();
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnRightClickItem)), HarmonyPrefix]
        public static bool InventoryGuiOnRightClickItemPatch(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item) {
            Player player = Player.m_localPlayer;

            if (item == null || !player) {
                return true;
            }

            if (grid.GetInventory() == player.GetInventory()) {
                return true;
            }

            if (!__instance.m_currentContainer || __instance.m_currentContainer.IsOwner()) {
                return true;
            }

            if (InventoryBlock.Get(player.GetInventory()).BlockConsume) {
                return false;
            }

            if (player.CanConsumeItem(item)) {
                InventoryBlock.Get(player.GetInventory()).BlockConsume = true;
                RequestConsume request = new RequestConsume(item);
#if DEBUG
                Timer.Start(request);
#endif
                __instance.m_currentContainer.m_nview.InvokeRPC(ContainerPatch.ItemConsumeRPC, request.WriteToPackage());
            }

            return false;
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateContainer)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ChangeOwnerCheck(IEnumerable<CodeInstruction> instructions) {
            // any player can potentially open a container, thus the IsOwner() statement need to be changed
            return new CodeMatcher(instructions)
                   .MatchForward(true,
                                 new CodeMatch(OpCodes.Ldarg_0),
                                 new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "m_currentContainer"),
                                 new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "IsOwner"))
                   .RemoveInstructions(1)
                   .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InventoryGuiPatch), nameof(CanOpenContainer))))
                   .InstructionEnumeration();
        }

        public static bool CanOpenContainer(Container container) {
            if (container.IsOdinShipContainer()) {
                // do not change behavior for OdinShip containers
                return container.IsOwner();
            }

            return true;
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnDropOutside)), HarmonyPrefix]
        public static bool InventoryGuiOnDropOutsidePatch(InventoryGui __instance) {
            Player player = Player.m_localPlayer;

            if (!__instance.m_dragGo) {
                return false;
            }

            bool isOwnerOfContainer = __instance.m_currentContainer && __instance.m_currentContainer.IsOwner();
            bool isPlayerInventory = player.GetInventory().GetInventories().Contains(__instance.m_dragInventory);

            if (isOwnerOfContainer || isPlayerInventory) {
                Log.LogDebug("Drop item from own inventory");
                return true;
            }

            RequestDrop request = new RequestDrop(__instance.m_dragItem.m_gridPos, __instance.m_dragAmount, player.GetZDOID());
#if DEBUG
            Timer.Start(request);
#endif
            __instance.m_currentContainer.m_nview.InvokeRPC(ContainerPatch.ItemDropRPC, request.WriteToPackage());
            __instance.SetupDragItem(null, null, 1);
            return false;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.Load)), HarmonyPostfix]
        public static void InventorySelectSameItemAfterLoad(Inventory __instance) {
            if (!InventoryGui.instance || InventoryGui.instance.m_dragItem == null) {
                return;
            }

            if (!InventoryGui.instance.m_currentContainer || InventoryGui.instance.m_currentContainer.GetInventory() != __instance) {
                return;
            }

            if (InventoryGui.instance.m_dragInventory == null || InventoryGui.instance.m_dragInventory != __instance) {
                return;
            }

            ItemDrop.ItemData dragItem = InventoryGui.instance.m_dragItem;
            ItemDrop.ItemData newItem = __instance.GetItemAt(dragItem.m_gridPos.x, dragItem.m_gridPos.y);

            if (newItem == null) {
                InventoryGui.instance.SetupDragItem(null, null, 1);
                return;
            }

            int amount = Mathf.Min(newItem.m_stack, InventoryGui.instance.m_dragAmount);
            InventoryGui.instance.m_dragAmount = amount;
            InventoryGui.instance.m_dragItem = newItem;
        }

        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateInventory)), HarmonyPostfix]
        public static void InventoryGridUpdateInventoryPatch(InventoryGrid __instance) {
            if (!InventoryPreview.GetChanges(__instance.m_inventory, out Dictionary<Vector2i, SlotPreview> preview)) {
                return;
            }

            foreach (InventoryGrid.Element element in __instance.m_elements) {
                if (!preview.TryGetValue(element.m_pos, out SlotPreview diff)) {
                    continue;
                }

                int previousStackSize = __instance.GetInventory().GetItemAt(element.m_pos.x, element.m_pos.y)?.m_stack ?? 0;
                int stackSize = previousStackSize + diff.amountDiff;
                int maxStackSize = diff.item?.m_shared?.m_maxStackSize ?? 0;

                element.m_icon.enabled = stackSize > 0;
                element.m_icon.sprite = diff.item?.GetIcon();

                element.m_amount.enabled = stackSize > 0 && maxStackSize > 1;
                element.m_amount.text = $"{stackSize}/{maxStackSize}";
            }
        }
    }
}
