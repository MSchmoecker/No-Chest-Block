namespace NoChestBlock {
    public class RequestRemove : IPackage {
        public Vector2i fromContainer;
        public Vector2i toInventory;
        public int dragAmount;
        public ItemDrop.ItemData switchItem;

        public RequestRemove(Vector2i fromContainer, Vector2i toInventory, int dragAmount, ItemDrop.ItemData switchItem) {
            this.fromContainer = fromContainer;
            this.toInventory = toInventory;
            this.dragAmount = dragAmount;
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
            bool hasSwitchItem = package.ReadBool();
            switchItem = hasSwitchItem ? InventoryHelper.LoadItemFromPackage(package) : null;
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestItemRemove:");
            Log.LogInfo($"  fromContainer: {fromContainer}");
            Log.LogInfo($"  toInventory: {toInventory}");
            Log.LogInfo($"  dragAmount: {dragAmount}");
            Log.LogInfo($"  switchItem: {switchItem != null}");
            InventoryHelper.PrintItem(switchItem);
        }
    }
}
