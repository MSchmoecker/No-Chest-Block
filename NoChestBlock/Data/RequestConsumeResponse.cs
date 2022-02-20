namespace NoChestBlock {
    public class RequestConsumeResponse : IPackage {
        public readonly ItemDrop.ItemData item;

        public RequestConsumeResponse(ZPackage package) {
            item = InventoryHelper.LoadItemFromPackage(package);
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

        public void PrintDebug() {
            Log.LogDebug($"RequestConsumeResponse:");
            Log.LogDebug($"  item: {item != null}");
            InventoryHelper.PrintItem(item);
        }
    }
}
