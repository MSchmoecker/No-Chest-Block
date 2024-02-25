namespace MultiUserChest {
    public class RequestConsumeResponse : IPackage, IResponse {
        public int SourceID { get; set; }
        public bool Success { get; set; }
        public int Amount { get; set; }

        public readonly ItemDrop.ItemData item;

        public RequestConsumeResponse(ZPackage package) {
            item = InventoryHelper.LoadItemFromPackage(package);
            Success = package.ReadBool();
            Amount = package.ReadInt();
        }

        public RequestConsumeResponse(ItemDrop.ItemData item, bool success, int amount) {
            this.item = item.ClampStackSize();
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

#if DEBUG
        public void PrintDebug() {
            Log.LogDebug($"RequestConsumeResponse:");
            Log.LogDebug($"  id: {SourceID}");
            Log.LogDebug($"  success: {Success}");
            Log.LogDebug($"  amount: {Amount}");
            InventoryHelper.PrintItem(nameof(item), item);
        }
#endif
    }
}
