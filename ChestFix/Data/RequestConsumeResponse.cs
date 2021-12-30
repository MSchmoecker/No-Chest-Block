namespace ChestFix {
    public class RequestConsumeResponse : IPackage {
        public ItemDrop.ItemData item;

        public RequestConsumeResponse(ZPackage package) {
            ReadFromPackage(package);
        }

        public RequestConsumeResponse(ItemDrop.ItemData item) {
            this.item = item;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(item != null);
            if (item != null) {
                InventoryHelper.WriteItemToPackage(item, package);
            }

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            bool hasItem = package.ReadBool();
            item = hasItem ? InventoryHelper.LoadItemFromPackage(package, new Vector2i(0, 0)) : null;
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestConsumeResponse:");
            Log.LogInfo($"  item: {item != null}");
        }
    }
}
