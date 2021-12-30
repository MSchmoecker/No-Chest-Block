namespace ChestFix {
    public class RequestConsume : IPackage {
        public int itemPosX;
        public int itemPosY;

        public RequestConsume(ItemDrop.ItemData item) {
            itemPosX = item.m_gridPos.x;
            itemPosY = item.m_gridPos.y;
        }

        public RequestConsume(ZPackage package) {
            ReadFromPackage(package);
        }

        public ZPackage WriteToPackage() {
            ZPackage package = new ZPackage();
            package.Write(itemPosX);
            package.Write(itemPosY);
            return package;
        }

        public void ReadFromPackage(ZPackage package) {
            itemPosX = package.ReadInt();
            itemPosY = package.ReadInt();
        }

        public void PrintDebug() {
            Log.LogInfo($"RequestConsume:");
            Log.LogInfo($"  itemPosX: {itemPosX}");
            Log.LogInfo($"  itemPosY: {itemPosY}");
        }
    }
}
