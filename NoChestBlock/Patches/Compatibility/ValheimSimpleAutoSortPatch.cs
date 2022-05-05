using HarmonyLib;

namespace NoChestBlock.Patches.Compatibility {
    public static class ValheimSimpleAutoSortPatch {
        [HarmonyPatch("ValheimAutoSort.ValheimAutoSort, ValheimAutoSort", "MoveItems"), HarmonyPrefix]
        public static bool MoveItemPatch(Container container, Inventory playerInventory, ItemDrop.ItemData item, ref bool __result) {
            if (container.IsOwner()) {
                Log.LogDebug("ValheimAutoSort MoveItems in own inventory");
                return true;
            }

            __result = container.GetInventory().CanAddItem(item);

            if (!__result) {
                return false;
            }

            container.AddItemToChest(item, playerInventory, new Vector2i(-1, -1), Player.m_localPlayer.GetZDOID());
            return false;
        }
    }
}
