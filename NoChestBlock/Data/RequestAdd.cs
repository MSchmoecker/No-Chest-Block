namespace NoChestBlock {
    public class RequestAdd : IPackage {
        public readonly Vector2i fromInventory;
        public readonly Vector2i toContainer;
        public readonly int dragAmount;
        public readonly ItemDrop.ItemData dragItem;
        public readonly string inventoryName;
        public readonly bool allowSwitch;

        public RequestAdd(Vector2i fromInventory, Vector2i toContainer, int dragAmount, ItemDrop.ItemData dragItem, string inventoryName, bool allowSwitch) {
            this.fromInventory = fromInventory;
            this.toContainer = toContainer;
            this.dragAmount = dragAmount;
            this.dragItem = dragItem;
            this.inventoryName = inventoryName;
            this.allowSwitch = allowSwitch;
        }

        public RequestAdd(ZPackage package) {
            fromInventory = package.ReadVector2i();
            toContainer = package.ReadVector2i();
            dragAmount = package.ReadInt();
            inventoryName = package.ReadString();
            dragItem = InventoryHelper.LoadItemFromPackage(package);
            allowSwitch = package.ReadBool();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(fromInventory);
            package.Write(toContainer);
            package.Write(dragAmount);
            package.Write(inventoryName);
            InventoryHelper.WriteItemToPackage(dragItem, package);
            package.Write(allowSwitch);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestItemAdd:");
            Log.LogDebug($"  fromInventory: {fromInventory}");
            Log.LogDebug($"  toContainer: {toContainer}");
            Log.LogDebug($"  dragAmount: {dragAmount}");
            Log.LogDebug($"  inventoryName: {inventoryName}");
            Log.LogDebug($"  allowSwitch: {allowSwitch}");
            Log.LogDebug($"  dragItem: {dragItem != null}");
            InventoryHelper.PrintItem(dragItem);
        }
    }
}
