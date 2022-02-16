namespace NoChestBlock {
    public class RequestDropResponse : IPackage {
        public ItemDrop.ItemData responseItem;

        public RequestDropResponse(ItemDrop.ItemData responseItem) {
            this.responseItem = responseItem;
        }

        public RequestDropResponse(ZPackage package) {
            ReadFromPackage(package);
        }

        public RequestDropResponse() {
            responseItem = null;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            InventoryHelper.WriteItemToPackage(responseItem, package);

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            responseItem = InventoryHelper.LoadItemFromPackage(package);
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestDropResponse:");
            Log.LogDebug($"  switchItem: {responseItem != null}");
            InventoryHelper.PrintItem(responseItem);
        }
    }
}
