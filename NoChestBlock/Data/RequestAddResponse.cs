namespace NoChestBlock {
    public class RequestAddResponse : IPackage {
        public readonly bool success;
        public readonly int amount;
        public readonly Vector2i inventoryPos;
        public readonly int inventoryHash;
        public readonly ItemDrop.ItemData switchItem;
        public readonly ZDOID sender;

        public RequestAddResponse(bool success, Vector2i inventoryPos, int amount, int inventoryHash, ItemDrop.ItemData switchItem, ZDOID sender) {
            this.success = success;
            this.inventoryPos = inventoryPos;
            this.amount = amount;
            this.inventoryHash = inventoryHash;
            this.switchItem = switchItem;
            this.sender = sender;
        }

        public RequestAddResponse(ZPackage package) {
            inventoryPos = package.ReadVector2i();
            success = package.ReadBool();
            amount = package.ReadInt();
            inventoryHash = package.ReadInt();
            switchItem = InventoryHelper.LoadItemFromPackage(package);
            sender = package.ReadZDOID();
        }

        public RequestAddResponse() {
            success = false;
            inventoryPos = new Vector2i(-1, -1);
            amount = 0;
            inventoryHash = 0;
            switchItem = null;
            sender = ZDOID.None;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(inventoryPos);
            package.Write(success);
            package.Write(amount);
            package.Write(inventoryHash);
            InventoryHelper.WriteItemToPackage(switchItem, package);
            package.Write(sender);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestAddResponse:");
            Log.LogDebug($"  inventoryPos: {inventoryPos}");
            Log.LogDebug($"  success: {success}");
            Log.LogDebug($"  amount: {amount}");
            Log.LogDebug($"  inventoryHash: {inventoryHash}");
            Log.LogDebug($"  switchItem: {switchItem != null}");
            Log.LogDebug($"  sender: {sender}");
            InventoryHelper.PrintItem(switchItem);
        }
    }
}
