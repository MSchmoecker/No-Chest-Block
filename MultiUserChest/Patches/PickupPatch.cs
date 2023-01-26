using HarmonyLib;

namespace MultiUserChest.Patches {
    /// <summary>
    ///     block complete pickup if any slot is blocked
    ///     slots are only blocked a limited amount of time and changed inventories (EAQS) don't have to be taken into account this way
    /// </summary>
    [HarmonyPatch]
    public static class PickupPatch {
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.Pickup)), HarmonyPrefix]
        public static bool HaveEmptySlotPrefix(Humanoid __instance, ref bool __result) {
            if (InventoryBlock.Get(__instance.GetInventory()).IsAnySlotBlocked()) {
#if DEBUG
                Log.LogDebug("Pickup blocked because of blocked slot");
#endif
                __result = false;
                return false;
            }

            return true;
        }
    }
}
