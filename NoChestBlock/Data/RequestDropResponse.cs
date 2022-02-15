namespace NoChestBlock {
    public class RequestDropResponse : IPackage {
        public ItemDrop.ItemData responseItem;

        public RequestDropResponse(ItemDrop.ItemData responseItem) {
            this.responseItem = responseItem;
        }

        public RequestDropResponse(ZPackage package) {
            ReadFromPackage(package);
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(responseItem != null);

            if (responseItem != null) {
                InventoryHelper.WriteItemToPackage(responseItem, package);
            }

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            bool hasSwitchItem = package.ReadBool();
            responseItem = hasSwitchItem ? InventoryHelper.LoadItemFromPackage(package) : null;
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestDropResponse:");
            Log.LogInfo($"  switchItem: {responseItem != null}");
            InventoryHelper.PrintItem(responseItem);
        }
    }
}
