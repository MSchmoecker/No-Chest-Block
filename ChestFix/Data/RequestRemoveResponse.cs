namespace ChestFix {
    public class RequestRemoveResponse : IPackage<RequestRemoveResponse> {
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

        public RequestRemoveResponse ReadFromPackage(ZPackage package) {
            success = package.ReadBool();
            amount = package.ReadInt();
            hasSwitched = package.ReadBool();
            inventoryPos = package.ReadVector2i();
            bool hasResponseItem = package.ReadBool();
            responseItem = hasResponseItem ? InventoryHelper.LoadItemFromPackage(package, inventoryPos) : null;
            return this;
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestRemoveResponse:");
            Log.LogInfo($"\tsuccess: {success}");
            Log.LogInfo($"\tamount: {amount}");
            Log.LogInfo($"\thasSwitched: {hasSwitched}");
            Log.LogInfo($"\tinventoryPos: {inventoryPos}");
            Log.LogInfo($"\tresponseItem: {responseItem != null}");
        }
    }
}
