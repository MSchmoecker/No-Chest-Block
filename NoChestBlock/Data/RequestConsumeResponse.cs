namespace NoChestBlock {
    public class RequestConsumeResponse : IPackage, IResponse {
        public bool Success { get; }
        public int Amount { get; }
        public readonly ItemDrop.ItemData item;

        public RequestConsumeResponse(ZPackage package) {
            item = InventoryHelper.LoadItemFromPackage(package);
            Success = package.ReadBool();
            Amount = package.ReadInt();
        }

        public RequestConsumeResponse(ItemDrop.ItemData item, bool success, int amount) {
            this.item = item;
            Success = success;
            Amount = amount;
        }

        public RequestConsumeResponse() {
            item = null;
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            InventoryHelper.WriteItemToPackage(item, package);
            package.Write(Success);
            package.Write(Amount);

            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestConsumeResponse:");
            Log.LogDebug($"  item: {item != null}");
            InventoryHelper.PrintItem(item);
        }
    }
}
