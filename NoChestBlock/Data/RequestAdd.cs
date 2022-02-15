namespace NoChestBlock {
    public class RequestAdd : IPackage {
        public Vector2i fromInventory;
        public Vector2i toContainer;
        public int dragAmount;
        public ItemDrop.ItemData dragItem;
        public string inventoryName;
        public bool allowSwitch;

        public RequestAdd(Vector2i fromInventory, Vector2i toContainer, int dragAmount, ItemDrop.ItemData dragItem, string inventoryName, bool allowSwitch) {
            this.fromInventory = fromInventory;
            this.toContainer = toContainer;
            this.dragAmount = dragAmount;
            this.dragItem = dragItem;
            this.inventoryName = inventoryName;
            this.allowSwitch = allowSwitch;
        }

        public RequestAdd(ZPackage package) {
            ReadFromPackage(package);
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

        public void ReadFromPackage(ZPackage package) {
            fromInventory = package.ReadVector2i();
            toContainer = package.ReadVector2i();
            dragAmount = package.ReadInt();
            inventoryName = package.ReadString();
            dragItem = InventoryHelper.LoadItemFromPackage(package);
            allowSwitch = package.ReadBool();
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestItemAdd:");
            Log.LogInfo($"  fromInventory: {fromInventory}");
            Log.LogInfo($"  toContainer: {toContainer}");
            Log.LogInfo($"  dragAmount: {dragAmount}");
            Log.LogInfo($"  inventoryName: {inventoryName}");
            Log.LogInfo($"  allowSwitch: {allowSwitch}");
            Log.LogInfo($"  dragItem: {dragItem != null}");
            InventoryHelper.PrintItem(dragItem);
        }
    }
}
