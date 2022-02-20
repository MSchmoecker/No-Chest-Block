namespace NoChestBlock {
    public class RequestRemove : IPackage {
        public readonly Vector2i fromContainer;
        public readonly Vector2i toInventory;
        public readonly int dragAmount;
        public readonly string inventoryName;
        public readonly ItemDrop.ItemData switchItem;

        public RequestRemove(Vector2i fromContainer, Vector2i toInventory, int dragAmount, string inventoryName, ItemDrop.ItemData switchItem) {
            this.fromContainer = fromContainer;
            this.toInventory = toInventory;
            this.dragAmount = dragAmount;
            this.inventoryName = inventoryName;
            this.switchItem = switchItem;
        }

        public RequestRemove(ZPackage package) {
            fromContainer = package.ReadVector2i();
            toInventory = package.ReadVector2i();
            dragAmount = package.ReadInt();
            inventoryName = package.ReadString();
            switchItem = InventoryHelper.LoadItemFromPackage(package);
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(fromContainer);
            package.Write(toInventory);
            package.Write(dragAmount);
            package.Write(inventoryName);
            InventoryHelper.WriteItemToPackage(switchItem, package);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestItemRemove:");
            Log.LogDebug($"  fromContainer: {fromContainer}");
            Log.LogDebug($"  toInventory: {toInventory}");
            Log.LogDebug($"  dragAmount: {dragAmount}");
            Log.LogDebug($"  inventoryName: {inventoryName}");
            Log.LogDebug($"  switchItem: {switchItem != null}");
            InventoryHelper.PrintItem(switchItem);
        }
    }
}
