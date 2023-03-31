namespace MultiUserChest {
    public class SlotPreview {
        public ItemDrop.ItemData item;
        public int amountDiff;

        public SlotPreview(ItemDrop.ItemData item, int amountDiff) {
            this.item = item;
            this.amountDiff = amountDiff;
        }

        public override string ToString() {
            return $"({item?.m_shared?.m_name ?? "-"} {amountDiff})";
        }
    }
}
