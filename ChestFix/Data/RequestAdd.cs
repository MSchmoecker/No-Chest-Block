namespace ChestFix {
    public class RequestItemAdd : IPackage<RequestItemAdd> {
        public Vector2i fromInventory;
        public Vector2i toContainer;
        public int dragAmount;
        public ItemDrop.ItemData dragItem;
        public bool allowSwitch;

        public RequestItemAdd(Vector2i fromInventory, Vector2i toContainer, int dragAmount, ItemDrop.ItemData dragItem, bool allowSwitch) {
            this.fromInventory = fromInventory;
            this.toContainer = toContainer;
            this.dragAmount = dragAmount;
            this.dragItem = dragItem;
            this.allowSwitch = allowSwitch;
        }

        public RequestItemAdd(ZPackage package) {
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

        public RequestItemAdd ReadFromPackage(ZPackage package) {
            fromInventory = package.ReadVector2i();
            toContainer = package.ReadVector2i();
            dragAmount = package.ReadInt();
            dragItem = InventoryHelper.LoadItemFromPackage(package, toContainer);
            allowSwitch = package.ReadBool();
            return this;
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestItemAdd:");
            Log.LogInfo($"\tfromInventory: {fromInventory}");
            Log.LogInfo($"\ttoContainer: {toContainer}");
            Log.LogInfo($"\tdragAmount: {dragAmount}");
            Log.LogInfo($"\tdragItem: {dragItem != null}");
            Log.LogInfo($"\tallowSwitch: {allowSwitch}");
        }
    }
}
