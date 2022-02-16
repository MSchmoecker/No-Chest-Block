namespace NoChestBlock {
    public class RequestRemove : IPackage {
        public Vector2i fromContainer;
        public Vector2i toInventory;
        public int dragAmount;
        public string inventoryName;
        public ItemDrop.ItemData switchItem;

        public RequestRemove(Vector2i fromContainer, Vector2i toInventory, int dragAmount, string inventoryName, ItemDrop.ItemData switchItem) {
            this.fromContainer = fromContainer;
            this.toInventory = toInventory;
            this.dragAmount = dragAmount;
            this.inventoryName = inventoryName;
            this.switchItem = switchItem;
        }

        public RequestRemove(ZPackage package) {
            ReadFromPackage(package);
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(fromContainer);
            package.Write(toInventory);
            package.Write(dragAmount);
            package.Write(inventoryName);
            package.Write(switchItem != null);

            if (switchItem != null) {
                InventoryHelper.WriteItemToPackage(switchItem, package);
            }

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            fromContainer = package.ReadVector2i();
            toInventory = package.ReadVector2i();
            dragAmount = package.ReadInt();
            inventoryName = package.ReadString();
            bool hasSwitchItem = package.ReadBool();
            switchItem = hasSwitchItem ? InventoryHelper.LoadItemFromPackage(package) : null;
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
