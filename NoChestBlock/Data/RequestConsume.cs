namespace NoChestBlock {
    public class RequestConsume : IPackage {
        public readonly int itemPosX;
        public readonly int itemPosY;

        public RequestConsume(ItemDrop.ItemData item) {
            itemPosX = item.m_gridPos.x;
            itemPosY = item.m_gridPos.y;
        }

        public RequestConsume(ZPackage package) {
            itemPosX = package.ReadInt();
            itemPosY = package.ReadInt();
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(itemPosX);
            package.Write(itemPosY);
            return package;
        }

        public void PrintDebug() {
            Log.LogDebug($"RequestConsume:");
            Log.LogDebug($"  itemPosX: {itemPosX}");
            Log.LogDebug($"  itemPosY: {itemPosY}");
        }
    }
}
