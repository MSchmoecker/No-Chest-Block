namespace ChestFix {
    public interface IPackage {
        ZPackage WriteToPackage();
        void ReadFromPackage(ZPackage package);
        void PrintDebug();
    }
}
