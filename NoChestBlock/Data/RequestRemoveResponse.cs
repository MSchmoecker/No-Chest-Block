namespace NoChestBlock {
    public class RequestRemoveResponse : IPackage {
        public bool success;
        public int amount;
        public bool hasSwitched;
        public Vector2i inventoryPos;
        public string inventoryName;
        public ItemDrop.ItemData responseItem;

        public RequestRemoveResponse(bool success, int amount, bool hasSwitched, Vector2i inventoryPos, string inventoryName, ItemDrop.ItemData responseItem) {
            this.success = success;
            this.amount = amount;
            this.hasSwitched = hasSwitched;
            this.inventoryPos = inventoryPos;
            this.inventoryName = inventoryName;
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
            inventoryName = "";
            responseItem = null;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(success);
            package.Write(amount);
            package.Write(hasSwitched);
            package.Write(inventoryPos);
            package.Write(inventoryName);
            InventoryHelper.WriteItemToPackage(responseItem, package);

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            success = package.ReadBool();
            amount = package.ReadInt();
            hasSwitched = package.ReadBool();
            inventoryPos = package.ReadVector2i();
            inventoryName = package.ReadString();
            responseItem = InventoryHelper.LoadItemFromPackage(package);
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestRemoveResponse:");
            Log.LogDebug($"  success: {success}");
            Log.LogDebug($"  amount: {amount}");
            Log.LogDebug($"  hasSwitched: {hasSwitched}");
            Log.LogDebug($"  inventoryPos: {inventoryPos}");
            Log.LogDebug($"  inventoryName: {inventoryName}");
            Log.LogDebug($"  responseItem: {responseItem != null}");
            InventoryHelper.PrintItem(responseItem);
        }
    }
}
