namespace NoChestBlock {
    public class RequestAddResponse : IPackage {
        public bool success;
        public Vector2i inventoryPos;
        public int amount;
        public ItemDrop.ItemData switchItem;

        public RequestAddResponse(bool success, Vector2i inventoryPos, int amount, ItemDrop.ItemData switchItem) {
            this.success = success;
            this.inventoryPos = inventoryPos;
            this.amount = amount;
            this.switchItem = switchItem;
        }

        public RequestAddResponse(ZPackage package) {
            ReadFromPackage(package);
        }

        public RequestAddResponse() {
            success = false;
            inventoryPos = new Vector2i(-1, -1);
            amount = 0;
            switchItem = null;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(inventoryPos);
            package.Write(success);
            package.Write(amount);
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
            bool hasSwitchItem = package.ReadBool();
            if (hasSwitchItem) {
                switchItem = InventoryHelper.LoadItemFromPackage(package);
            }
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestAddResponse:");
            Log.LogInfo($"  inventoryPos: {inventoryPos}");
            Log.LogInfo($"  success: {success}");
            Log.LogInfo($"  amount: {amount}");
            Log.LogInfo($"  switchItem: {switchItem != null}");
            InventoryHelper.PrintItem(switchItem);
        }
    }
}
