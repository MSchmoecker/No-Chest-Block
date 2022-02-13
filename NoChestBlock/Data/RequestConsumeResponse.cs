namespace NoChestBlock {
    public class RequestConsumeResponse : IPackage {
        public ItemDrop.ItemData item;

        public RequestConsumeResponse(ZPackage package) {
            ReadFromPackage(package);
        }

        public RequestConsumeResponse(ItemDrop.ItemData item) {
            this.item = item;
        }

        public RequestConsumeResponse() {
            item = null;
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
            item = hasItem ? InventoryHelper.LoadItemFromPackage(package) : null;
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestConsumeResponse:");
            Log.LogInfo($"  item: {item != null}");
            InventoryHelper.PrintItem(item);
        }
    }
}
