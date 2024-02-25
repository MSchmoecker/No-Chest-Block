namespace MultiUserChest {
    public interface IResponse : IPackage {
        int SourceID { get; set; }
        bool Success { get; set; }
        int Amount { get; set; }
    }
}
