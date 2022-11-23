using System.Collections.Generic;

namespace MultiUserChest {
    public class RequestTakeAll : IPackage {
        public readonly List<ItemDrop.ItemData> items = new List<ItemDrop.ItemData>();

        public RequestTakeAll(List<ItemDrop.ItemData> items) {
            this.items = items;
        }

        public RequestTakeAll(ZPackage package) {
            int count = package.ReadInt();

            for (int i = 0; i < count; i++) {
                items.Add(InventoryHelper.LoadItemFromPackage(package));
            }
        }

        public RequestTakeAll() {
            items = new List<ItemDrop.ItemData>();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();

            package.Write(items.Count);
            foreach (ItemDrop.ItemData item in items) {
                InventoryHelper.WriteItemToPackage(item, package);
            }

            return package;
        }

        public void PrintDebug() {
#if FULL_DEBUG
            Log.LogDebug($"TakeAll:");
            Log.LogDebug($"  items: {items.Count}");

            for (int i = 0; i < items.Count; i++) {
                ItemDrop.ItemData item = items[i];

                if (item == null) {
                    Log.LogDebug($"    {i} item is null");
                } else {
                    Log.LogDebug($"    {i} {item.m_shared?.m_name}");
                    Log.LogDebug($"    {i} {item.m_stack}");
                }
            }
#endif
        }
    }
}
