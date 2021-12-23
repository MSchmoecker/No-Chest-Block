namespace ChestFix {
    public class RequestAddResponse : IPackage<RequestAddResponse> {
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

        public RequestAddResponse ReadFromPackage(ZPackage package) {
            inventoryPos = package.ReadVector2i();
            success = package.ReadBool();
            amount = package.ReadInt();
            bool hasSwitchItem = package.ReadBool();
            if (hasSwitchItem) {
                switchItem = InventoryHelper.LoadItemFromPackage(package, inventoryPos);
            }

            return this;
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestAddResponse:");
            Log.LogInfo($"\tinventoryPos: {inventoryPos}");
            Log.LogInfo($"\tsuccess: {success}");
            Log.LogInfo($"\tamount: {amount}");
            Log.LogInfo($"\tswitchItem: {switchItem != null}");
        }
    }
}
