namespace NoChestBlock {
    public interface IPackage {
        ZPackage WriteToPackage();
        void ReadFromPackage(ZPackage package);
        void PrintDebug();
    }
}
