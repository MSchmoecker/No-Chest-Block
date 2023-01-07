using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiUserChest {
    public class InventoryHandler {
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
#if FULL_DEBUG
            Log.LogDebug($"RequestItemMoveResponse:");
            Log.LogDebug($"\tsuccess: {success}");
#endif
        }

        private static void HandleRPC<T>(T package, Func<T, Inventory> inventory, Container container,
            Action<Inventory, Container, T> handle) where T : IPackage {
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
            InventoryBlock.Get(inventory).ReleaseSlot(inventoryPos);

            if (response.Success && inventoryPos.x >= 0 && inventoryPos.y >= 0) {
                bool added = inventory.AddItemToInventory(response.responseItem, response.Amount, inventoryPos);

                if (!added) {
                    int dropAmount = response.Amount;
                    ItemDrop.ItemData exisingItem = inventory.GetItemAt(inventoryPos.x, inventoryPos.y);

                    if (exisingItem != null && InventoryHelper.IsSameItem(response.responseItem, exisingItem)) {
                        int maxStackSize = exisingItem.m_shared.m_maxStackSize;
                        int existingAmount = exisingItem.m_stack;
                        int freeSpace = maxStackSize - existingAmount;

                        exisingItem.m_stack += freeSpace;
                        dropAmount -= freeSpace;
                    }

                    DropItem(response.responseItem, dropAmount);
                }
            } else if (response.responseItem != null) {
                Inventory tmp = new Inventory("tmp", null, 1, 1);
                tmp.AddItem(response.responseItem);
                inventory.MoveItemToThis(tmp, response.responseItem);

                ItemDrop.ItemData notMovedItem = tmp.GetItemAt(0, 0);

                if (notMovedItem != null) {
                    DropItem(notMovedItem, notMovedItem.m_stack);
                }
            }

            UpdateGUIAfterPlayerMove(response.sender);
        }

        private static void RPC_RequestDropResponse(RequestDropResponse response) {
            DropItem(response.responseItem, response.responseItem.m_stack);
            UpdateGUIAfterPlayerMove(response.sender);
        }

        private static void UpdateGUIAfterPlayerMove(ZDOID sender) {
            if (InventoryGui.instance && Player.m_localPlayer && sender == Player.m_localPlayer.GetZDOID()) {
                InventoryGui.instance.UpdateCraftingPanel();
            }
        }

        public static void DropItem(ItemDrop.ItemData item, int amount) {
            DropItem(Player.m_localPlayer, item, amount);
        }

        private static void DropItem(Player player, ItemDrop.ItemData item, int amount) {
            Transform playerTransform = player.transform;

            ItemDrop drop = ItemDrop.DropItem(item, amount, playerTransform.position + playerTransform.forward + playerTransform.up, playerTransform.rotation);
            drop.OnPlayerDrop();
            drop.GetComponent<Rigidbody>().velocity = (playerTransform.forward + Vector3.up) * 5f;
            player.m_zanim.SetTrigger("interact");
            player.m_dropEffects.Create(playerTransform.position, Quaternion.identity);

            ItemDrop.ItemData dropData = drop.m_itemData;
            player.Message(MessageHud.MessageType.TopLeft, "$msg_dropped " + dropData.m_shared.m_name, dropData.m_stack, dropData.GetIcon());
        }

        public static void RPC_RequestItemAddResponse(Inventory inventory, RequestAddResponse response) {
            Vector2i inventoryPos = response.inventoryPos;
            ItemDrop.ItemData switchItem = response.switchItem;

            InventoryBlock.Get(inventory).ReleaseSlot(inventoryPos);

            if (switchItem != null) {
                bool added = inventory.AddItemToInventory(switchItem, switchItem.m_stack, inventoryPos);

                if (!added) {
                    DropItem(switchItem, switchItem.m_stack);
                }
            }

            UpdateGUIAfterPlayerMove(response.sender);
        }

        public static void RPC_RequestItemConsumeResponse(RequestConsumeResponse response) {
            Player player = Player.m_localPlayer;
            InventoryBlock.Get(player.GetInventory()).BlockConsume(false);

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
            InventoryBlock.Get(inventory).BlockAllSlots(false);

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

            foreach (ItemDrop.ItemData notMovedItem in tmp.m_inventory) {
                DropItem(notMovedItem, notMovedItem.m_stack);
            }

            container.m_onTakeAllSuccess?.Invoke();
        }

        private static Inventory GetInventory(ZDOID targetId, int hash) {
            GameObject target = ZNetScene.instance.FindInstance(targetId);

            if (target.TryGetComponent(out Player player)) {
                if (player.GetInventory().IsType("ExtendedInventory") && player.GetInventory().HasField("_inventories")) {
                    List<Inventory> inventories = Player.m_localPlayer.GetInventory().GetField<List<Inventory>>("_inventories");
                    return inventories.FirstOrDefault(i => i.m_name.GetStableHashCode() == hash) ?? player.GetInventory();
                }

                return player.GetInventory();
            }

            if (target.TryGetComponent(out Container container)) {
                return container.GetInventory();
            }

            return null;
        }
    }
}
