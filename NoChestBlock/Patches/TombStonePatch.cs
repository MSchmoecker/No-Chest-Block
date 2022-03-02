using System.Linq;
using HarmonyLib;

namespace NoChestBlock.Patches {
    [HarmonyPatch]
    public static class TombStonePatch {
        [HarmonyPatch(typeof(TombStone), nameof(TombStone.EasyFitInInventory)), HarmonyPostfix]
        public static void EasyFitInInventoryPatch(TombStone __instance, ref bool __result, Player player) {
            if (!__result) {
                return;
            }

            int moveAbleCount = InventoryHelper.GetAllMoveableItems(__instance.m_container.GetInventory(), player.GetInventory()).Sum(i => i.m_stack);
            int containerCount = __instance.m_container.GetInventory().NrOfItemsIncludingStacks();

            if (moveAbleCount != containerCount) {
                __result = false;
            }
        }
    }
}
