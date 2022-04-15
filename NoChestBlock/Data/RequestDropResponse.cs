namespace NoChestBlock {
    public class RequestDropResponse : IPackage {
        public readonly ItemDrop.ItemData responseItem;
        public readonly ZDOID sender;

        public RequestDropResponse(ItemDrop.ItemData responseItem, ZDOID sender) {
            this.responseItem = responseItem;
            this.sender = sender;
        }

        public RequestDropResponse(ZPackage package) {
            responseItem = InventoryHelper.LoadItemFromPackage(package);
            sender = package.ReadZDOID();
        }

        public RequestDropResponse() {
            responseItem = null;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            InventoryHelper.WriteItemToPackage(responseItem, package);
            package.Write(sender);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestDropResponse:");
            Log.LogDebug($"  switchItem: {responseItem != null}");
            InventoryHelper.PrintItem(responseItem);
        }
    }
}
