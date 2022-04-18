using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoChestBlock {
    public class InventoryHandler {
        public static readonly List<Vector2i> blockedSlots = new List<Vector2i>();
        public static bool blockConsume;
        public static bool blockAllSlots;

        public static void RPC_RequestItemAddResponse(long sender, ZPackage package) {
            HandleRPC(new RequestAddResponse(package), p => GetInventory(p.sender, p.inventoryHash), RPC_RequestItemAddResponse);
        }

        public static void RPC_RequestTakeAllItemsResponse(Container container, long sender, ZPackage package) {
            HandleRPC(new RequestTakeAll(package), p => Player.m_localPlayer.GetInventory(), container, RPC_RequestTakeAllItemsResponse);
        }

        public static void RPC_RequestItemRemoveResponse(long sender, ZPackage package) {
            HandleRPC(new RequestRemoveResponse(package), p => GetInventory(p.sender, p.inventoryHash), RPC_RequestItemRemoveResponse);
        }

        public static void RPC_RequestDropResponse(long sender, ZPackage package) {
            HandleRPC(new RequestDropResponse(package), RPC_RequestDropResponse);
        }

        public static void RPC_RequestItemConsumeResponse(long sender, ZPackage package) {
            HandleRPC(new RequestConsumeResponse(package), RPC_RequestItemConsumeResponse);
        }

        public static void RPC_RequestItemMoveResponse(long sender, bool success) {
            Timer.Stop("RPC_RequestItemMoveResponse");
            InventoryGui.instance.SetupDragItem(null, null, 0);
#if FULL_DEBUG
            Log.LogDebug($"RequestItemMoveResponse:");
            Log.LogDebug($"\tsuccess: {success}");
#endif
        }

        private static void HandleRPC<T>(T package, Func<T, Inventory> inventory, Container container, Action<Inventory, Container, T> handle) where T : IPackage {
            CallRPC(handle.Method.Name, package, () => handle.Invoke(inventory.Invoke(package), container, package));
        }

        private static void HandleRPC<T>(T package, Func<T, Inventory> inventory, Action<Inventory, T> handle) where T : IPackage {
            CallRPC(handle.Method.Name, package, () => handle.Invoke(inventory.Invoke(package), package));
        }

        private static void HandleRPC<T>(T package, Action<T> handle) where T : IPackage {
            CallRPC(handle.Method.Name, package, () => handle.Invoke(package));
        }

        private static void CallRPC(string timerName, IPackage package, Action action) {
            Timer.Stop(timerName);
            package.PrintDebug();
            action?.Invoke();
        }

        public static void RPC_RequestItemRemoveResponse(Inventory inventory, RequestRemoveResponse response) {
            Vector2i inventoryPos = response.inventoryPos;
            ReleaseSlot(inventoryPos);

            if (response.Success) {
                if (response.hasSwitched) {
                    ItemDrop.ItemData atSlot = inventory.GetItemAt(inventoryPos.x, inventoryPos.y);
                    inventory.RemoveItem(atSlot);
                }

                if (inventoryPos.x >= 0 && inventoryPos.y >= 0) {
                    inventory.AddItemToInventory(response.responseItem, response.Amount, inventoryPos);
                } else {
                    Inventory tmp = new Inventory("tmp", null, 1, 1);
                    tmp.AddItem(response.responseItem);
                    inventory.MoveItemToThis(tmp, response.responseItem);
                }
            }

            UpdateGUIAfterPlayerMove(response.sender);
        }

        private static void RPC_RequestDropResponse(RequestDropResponse response) {
            DropItem(response.responseItem, response.responseItem.m_stack);
            UpdateGUIAfterPlayerMove(response.sender);
        }

        private static void UpdateGUIAfterPlayerMove(ZDOID sender) {
            if (InventoryGui.instance != null && sender == Player.m_localPlayer.GetZDOID()) {
                InventoryGui.instance.UpdateCraftingPanel();
            }
        }

        private static void DropItem(ItemDrop.ItemData responseItem, int amount) {
            Transform player = Player.m_localPlayer.transform;
            ItemDrop drop = ItemDrop.DropItem(responseItem, amount, player.position + player.forward + player.up, player.rotation);
            drop.OnPlayerDrop();
            drop.GetComponent<Rigidbody>().velocity = (player.forward + Vector3.up) * 5f;
            Player.m_localPlayer.m_zanim.SetTrigger("interact");
            Player.m_localPlayer.m_dropEffects.Create(player.position, Quaternion.identity);

            ItemDrop.ItemData dropData = drop.m_itemData;
            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$msg_dropped " + dropData.m_shared.m_name, dropData.m_stack, dropData.GetIcon());
        }

        public static void RPC_RequestItemAddResponse(Inventory inventory, RequestAddResponse response) {
            Vector2i inventoryPos = response.inventoryPos;
            ItemDrop.ItemData switchItem = response.switchItem;

            ReleaseSlot(inventoryPos);

            if (switchItem != null) {
                inventory.AddItemToInventory(switchItem, switchItem.m_stack, inventoryPos);
            }

            UpdateGUIAfterPlayerMove(response.sender);
        }

        public static void RPC_RequestItemConsumeResponse(RequestConsumeResponse response) {
            Player player = Player.m_localPlayer;
            blockConsume = false;

            if (response.item == null) {
                return;
            }

            if (!player.CanConsumeItem(response.item)) {
                return;
            }

            if (response.item.m_shared.m_consumeStatusEffect) {
                player.m_seman.AddStatusEffect(response.item.m_shared.m_consumeStatusEffect, true);
            }

            if (response.item.m_shared.m_food > 0.0) {
                player.EatFood(response.item);
            }
        }

        private static void RPC_RequestTakeAllItemsResponse(Inventory inventory, Container container, RequestTakeAll response) {
            blockAllSlots = false;

            if (response.items.Count == 0) {
                return;
            }

            int width = response.items.Max(i => i.m_gridPos.x) + 1;
            int height = response.items.Max(i => i.m_gridPos.y) + 1;
            Inventory tmp = new Inventory("tmp", null, width, height);

            foreach (ItemDrop.ItemData item in response.items) {
                tmp.AddItemToInventory(item, item.m_stack, item.m_gridPos);
            }

            inventory.MoveAll(tmp);

            container.m_onTakeAllSuccess?.Invoke();
        }

        private static Inventory GetInventory(ZDOID targetId, int hash) {
            GameObject target = ZNetScene.instance.FindInstance(targetId);

            if (target.TryGetComponent(out Player player)) {
                if (player.GetInventory().IsType("ExtendedInventory") && player.GetInventory().HasField("_inventories")) {
                    List<Inventory> inventories = Player.m_localPlayer.m_inventory.GetField<List<Inventory>>("_inventories");
                    return inventories.FirstOrDefault(i => i.m_name.GetStableHashCode() == hash) ?? player.GetInventory();
                }

                return player.GetInventory();
            }

            if (target.TryGetComponent(out Container container)) {
                return container.GetInventory();
            }

            return null;
        }

        public static void BlockSlot(Vector2i slot) {
            blockedSlots.Add(slot);
        }

        public static void ReleaseSlot(Vector2i slot) {
            blockedSlots.RemoveAll(i => i.x == slot.x && i.y == slot.y);
        }

        public static bool IsSlotBlocked(Vector2i slot) {
            return !blockAllSlots && blockedSlots.Contains(slot);
        }

        public static bool IsAnySlotBlocked() {
            return blockAllSlots || blockedSlots.Count > 0;
        }
    }
}
