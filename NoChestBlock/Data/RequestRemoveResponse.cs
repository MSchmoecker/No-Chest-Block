namespace NoChestBlock {
    public class RequestRemoveResponse : IPackage {
        public bool success;
        public int amount;
        public bool hasSwitched;
        public Vector2i inventoryPos;
        public ItemDrop.ItemData responseItem;

        public RequestRemoveResponse(bool success, int amount, bool hasSwitched, Vector2i inventoryPos, ItemDrop.ItemData responseItem) {
            this.success = success;
            this.amount = amount;
            this.hasSwitched = hasSwitched;
            this.inventoryPos = inventoryPos;
            this.responseItem = responseItem;
        }

        public RequestRemoveResponse(ZPackage package) {
            ReadFromPackage(package);
        }

        public RequestRemoveResponse() {
            success = false;
            amount = 0;
            hasSwitched = false;
            inventoryPos = new Vector2i(-1, -1);
            responseItem = null;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(success);
            package.Write(amount);
            package.Write(hasSwitched);
            package.Write(inventoryPos);
            package.Write(responseItem != null);

            if (responseItem != null) {
                InventoryHelper.WriteItemToPackage(responseItem, package);
            }

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            success = package.ReadBool();
            amount = package.ReadInt();
            hasSwitched = package.ReadBool();
            inventoryPos = package.ReadVector2i();
            bool hasResponseItem = package.ReadBool();
            responseItem = hasResponseItem ? InventoryHelper.LoadItemFromPackage(package) : null;
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestRemoveResponse:");
            Log.LogInfo($"  success: {success}");
            Log.LogInfo($"  amount: {amount}");
            Log.LogInfo($"  hasSwitched: {hasSwitched}");
            Log.LogInfo($"  inventoryPos: {inventoryPos}");
            Log.LogInfo($"  responseItem: {responseItem != null}");
        }
    }
}
