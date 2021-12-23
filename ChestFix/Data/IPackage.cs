namespace ChestFix {
    public interface IPackage<T> {
        ZPackage WriteToPackage();
        T ReadFromPackage(ZPackage package);
    }
}
