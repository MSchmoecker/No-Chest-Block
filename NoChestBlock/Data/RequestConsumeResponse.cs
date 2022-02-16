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

            InventoryHelper.WriteItemToPackage(item, package);

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            item = InventoryHelper.LoadItemFromPackage(package);
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestConsumeResponse:");
            Log.LogDebug($"  item: {item != null}");
            InventoryHelper.PrintItem(item);
        }
    }
}
