using System.Collections.Generic;

namespace NoChestBlock {
    public class RequestTakeAll : IPackage {
        public List<ItemDrop.ItemData> items = new List<ItemDrop.ItemData>();

        public RequestTakeAll(List<ItemDrop.ItemData> items) {
            this.items = items;
        }

        public RequestTakeAll(ZPackage package) {
            ReadFromPackage(package);
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(items.Count);
            foreach (ItemDrop.ItemData item in items) {
                InventoryHelper.WriteItemToPackage(item, package);
            }

            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            int count = package.ReadInt();

            for (int i = 0; i < count; i++) {
                items.Add(InventoryHelper.LoadItemFromPackage(package));
            }
        }

        public void PrintDebug() {
            Log.LogInfo($"TakeAll:");
            Log.LogInfo($"  items: {items.Count}");
        }
    }
}
