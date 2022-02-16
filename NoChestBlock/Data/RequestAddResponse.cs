namespace NoChestBlock {
    public class RequestAddResponse : IPackage {
        public bool success;
        public Vector2i inventoryPos;
        public int amount;
        public string inventoryName;
        public ItemDrop.ItemData switchItem;

        public RequestAddResponse(bool success, Vector2i inventoryPos, int amount, string inventoryName, ItemDrop.ItemData switchItem) {
            this.success = success;
            this.inventoryPos = inventoryPos;
            this.amount = amount;
            this.inventoryName = inventoryName;
            this.switchItem = switchItem;
        }

        public RequestAddResponse(ZPackage package) {
            ReadFromPackage(package);
        }

        public RequestAddResponse() {
            success = false;
            inventoryPos = new Vector2i(-1, -1);
            amount = 0;
            inventoryName = "";
            switchItem = null;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(inventoryPos);
            package.Write(success);
            package.Write(amount);
            package.Write(inventoryName);
            package.Write(switchItem != null);

            if (switchItem != null) {
                InventoryHelper.WriteItemToPackage(switchItem, package);
            }

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            inventoryPos = package.ReadVector2i();
            success = package.ReadBool();
            amount = package.ReadInt();
            inventoryName = package.ReadString();
            bool hasSwitchItem = package.ReadBool();
            if (hasSwitchItem) {
                switchItem = InventoryHelper.LoadItemFromPackage(package);
            }
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestAddResponse:");
            Log.LogDebug($"  inventoryPos: {inventoryPos}");
            Log.LogDebug($"  success: {success}");
            Log.LogDebug($"  amount: {amount}");
            Log.LogDebug($"  inventoryName: {inventoryName}");
            Log.LogDebug($"  switchItem: {switchItem != null}");
            InventoryHelper.PrintItem(switchItem);
        }
    }
}
