using System.Collections.Generic;

namespace NoChestBlock {
    public class ContainerHandler {
        public static void TakeAll(Container container) {
            Inventory playerInventory = Player.m_localPlayer.GetInventory();
            List<ItemDrop.ItemData> wanted = InventoryHelper.GetAllMoveableItems(container.GetInventory(), playerInventory);

            InventoryHandler.blockAllSlots = true;

            RequestTakeAll request = new RequestTakeAll(wanted);
            Timer.Start(request);

            if (container != null && container.m_nview) {
                container.m_nview.InvokeRPC("RequestTakeAllItems", request.WriteToPackage());
            }
        }

        public static RequestAdd AddItemToChest(Container container, Inventory inventory, ZDOID sender, Vector2i @from, Vector2i to, int dragAmount, bool allowSwitch) {
            ItemDrop.ItemData item = inventory.GetItemAt(from.x, from.y).Clone();
            RequestAdd request = new RequestAdd(to, dragAmount, item, inventory.m_name, allowSwitch, sender);

            inventory.RemoveItem(inventory.GetItemAt(from.x, from.y), dragAmount);
            InventoryHandler.BlockSlot(from);

            if (container != null && container.m_nview) {
                Timer.Start(request);
                container.m_nview.InvokeRPC("RequestItemAdd", request.WriteToPackage());
            }

            return request;
        }

        public static void RemoveItemFromChest(RequestRemove request, Container container) {
            InventoryHandler.BlockSlot(request.toPos);

            if (container != null && container.m_nview) {
                Timer.Start(request);
                container.m_nview.InvokeRPC("RequestItemRemove", request.WriteToPackage());
            }
        }
    }
}
