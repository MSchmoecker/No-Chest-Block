namespace MultiUserChest {
    public static class InventoryIgnore {
        public static bool IgnoreInventory(this InventoryOwner owner) {
            if (OdinShip.IsOdinShipInstalled() && owner is ContainerInventoryOwner containerOwner && containerOwner.Container.IsOdinShipContainer()) {
                return true;
            }

            if (owner == null || !owner.ZNetView || owner.ZNetView.GetZDO() == null) {
                return false;
            }

            return owner.ZNetView.GetZDO().GetBool("MUC_Ignore");
        }

        public static bool IgnoreInventory(this Container container) {
            return IgnoreInventory(InventoryOwner.GetOwner(container.GetInventory()));
        }
    }
}
