using HarmonyLib;

namespace NoChestBlock.Patches.Compatibility {
    [HarmonyPatch]
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

            ContainerHandler.AddItemToChest(item.m_gridPos, new Vector2i(-1, -1), item.m_stack, false, playerInventory, container);
            return false;
        }
    }
}
