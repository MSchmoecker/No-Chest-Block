namespace NoChestBlock {
    public class RequestRemoveResponse : IPackage {
        public readonly bool success;
        public readonly int amount;
        public readonly Vector2i inventoryPos;
        public readonly int inventoryHash;
        public readonly ItemDrop.ItemData responseItem;
        public readonly ZDOID sender;
        public readonly bool hasSwitched;

        public RequestRemoveResponse(bool success, int amount, bool hasSwitched, Vector2i inventoryPos, int inventoryHash, ItemDrop.ItemData responseItem, ZDOID sender) {
            this.success = success;
            this.amount = amount;
            this.hasSwitched = hasSwitched;
            this.inventoryPos = inventoryPos;
            this.inventoryHash = inventoryHash;
            this.responseItem = responseItem;
            this.sender = sender;
        }

        public RequestRemoveResponse(ZPackage package) {
            success = package.ReadBool();
            amount = package.ReadInt();
            hasSwitched = package.ReadBool();
            inventoryPos = package.ReadVector2i();
            inventoryHash = package.ReadInt();
            responseItem = InventoryHelper.LoadItemFromPackage(package);
            sender = package.ReadZDOID();
        }

        public RequestRemoveResponse() {
            success = false;
            amount = 0;
            hasSwitched = false;
            inventoryPos = new Vector2i(-1, -1);
            inventoryHash = 0;
            responseItem = null;
            sender = ZDOID.None;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(success);
            package.Write(amount);
            package.Write(hasSwitched);
            package.Write(inventoryPos);
            package.Write(inventoryHash);
            InventoryHelper.WriteItemToPackage(responseItem, package);
            package.Write(sender);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestRemoveResponse:");
            Log.LogDebug($"  success: {success}");
            Log.LogDebug($"  amount: {amount}");
            Log.LogDebug($"  hasSwitched: {hasSwitched}");
            Log.LogDebug($"  inventoryPos: {inventoryPos}");
            Log.LogDebug($"  inventoryName: {inventoryHash}");
            Log.LogDebug($"  responseItem: {responseItem != null}");
            Log.LogDebug($"  sender: {sender}");
            InventoryHelper.PrintItem(responseItem);
        }
    }
}
