using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace NoChestBlock.Patches {
    [HarmonyPatch]
    public class InventoryGuiOnSelectedItemFastMovePatch {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem)), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OnSelectedItemFastMovePatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
            Label label = generator.DefineLabel();

            return new CodeMatcher(instructions, generator)
                   .MatchForward(false,
                                 new CodeMatch(i => CodeMatcherExtensions.IsVirtCall(i, nameof(Humanoid), nameof(Humanoid.UnequipItem))),
                                 new CodeMatch(OpCodes.Ldarg_1),
                                 new CodeMatch(i => CodeMatcherExtensions.IsVirtCall(i, nameof(InventoryGrid), nameof(InventoryGrid.GetInventory))))
                   .Advance(1)
                   .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1),
                                     new CodeInstruction(OpCodes.Ldarg_2),
                                     new CodeInstruction(OpCodes.Ldarg_3),
                                     new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InventoryGuiOnSelectedItemFastMovePatch), nameof(FastMoveItem))),
                                     new CodeInstruction(OpCodes.Brtrue, label),
                                     new CodeInstruction(OpCodes.Ret))
                   .AddLabels(new[] { label })
                   .InstructionEnumeration();
        }

        private static bool FastMoveItem(InventoryGrid grid, ItemDrop.ItemData item, Vector2i fromPos) {
            Log.LogInfo("FastMoveItem");
            InventoryGui gui = InventoryGui.instance;
            Player player = Player.m_localPlayer;

            bool isOwnerOfContainer = gui.m_currentContainer && gui.m_currentContainer.IsOwner();

            if (isOwnerOfContainer) {
                Log.LogInfo("FastMoveItem in own inventory");
                return true;
            }

            if (grid.GetInventory() == gui.m_currentContainer.GetInventory()) {
                if (!player.GetInventory().CanAddItem(item)) {
                    return false;
                }

                Vector2i targetSlot = InventoryHelper.FindEmptySlot(player.GetInventory(), InventoryHandler.blockedSlots);

                if (targetSlot.x == -1 || targetSlot.y == -1) {
                    return false;
                }

                RequestRemove request = new RequestRemove(fromPos, targetSlot, item.m_stack, player.GetInventory().m_name, null);
                ContainerHandler.RemoveItemFromChest(request, gui.m_currentContainer);
            } else {
                Vector2i targetPos = gui.m_currentContainer.GetInventory().FindEmptySlot(true);
                ContainerHandler.AddItemToChest(fromPos, targetPos, item.m_stack, false, grid.m_inventory, gui.m_currentContainer);
            }

            gui.m_moveItemEffects.Create(gui.transform.position, Quaternion.identity);
            Log.LogDebug("FastMoveItem in other inventory");
            return false;
        }
    }
}
