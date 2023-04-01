namespace MultiUserChest {
    public interface IResponse {
        int SourceID { get; }
        bool Success { get; }
        int Amount { get; }
    }
}
