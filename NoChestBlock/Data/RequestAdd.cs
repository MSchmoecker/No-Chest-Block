namespace NoChestBlock {
    public class RequestAdd : IPackage {
        public Vector2i fromInventory;
        public Vector2i toContainer;
        public int dragAmount;
        public ItemDrop.ItemData dragItem;
        public bool allowSwitch;

        public RequestAdd(Vector2i fromInventory, Vector2i toContainer, int dragAmount, ItemDrop.ItemData dragItem, bool allowSwitch) {
            this.fromInventory = fromInventory;
            this.toContainer = toContainer;
            this.dragAmount = dragAmount;
            this.dragItem = dragItem;
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
            InventoryHelper.WriteItemToPackage(dragItem, package);
            package.Write(allowSwitch);

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            fromInventory = package.ReadVector2i();
            toContainer = package.ReadVector2i();
            dragAmount = package.ReadInt();
            dragItem = InventoryHelper.LoadItemFromPackage(package);
            allowSwitch = package.ReadBool();
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestItemAdd:");
            Log.LogInfo($"  fromInventory: {fromInventory}");
            Log.LogInfo($"  toContainer: {toContainer}");
            Log.LogInfo($"  dragAmount: {dragAmount}");
            Log.LogInfo($"  dragItem: {dragItem != null}");
            Log.LogInfo($"  allowSwitch: {allowSwitch}");
        }
    }
}
