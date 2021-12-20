using ChestFix;
using HarmonyLib;
using NUnit.Framework;
using UnityEngine;

namespace UnitTests {
    [TestFixture]
    public class ContainerHandlerTest {
        [OneTimeSetUp]
        public void Setup() {
            Harmony harmony = new Harmony("id2");
            harmony.PatchAll(typeof(LogPatch));
            harmony.PatchAll(typeof(InventoryAddItemPatch));
        }

        [HarmonyPatch]
        public static class LogPatch {
            [HarmonyPatch(typeof(Log), nameof(Log.LogInfo)), HarmonyPrefix]
            public static bool NoLogPatch(object data) {
                System.Console.WriteLine(data);
                return false;
            }
        }

        [HarmonyPatch]
        public static class InventoryAddItemPatch {
            [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(string), typeof(int), typeof(float), typeof(Vector2i),
                          typeof(bool), typeof(int), typeof(int), typeof(long), typeof(string)), HarmonyPrefix]
            public static bool AddNoMonoBehaviourPatch(Inventory __instance, ref bool __result, string name, int stack, float durability, Vector2i pos,
                bool equiped, int quality, int variant,
                long crafterID, string crafterName) {
                ItemDrop.ItemData itemData = new ItemDrop.ItemData() {
                    m_stack = stack,
                    m_durability = durability,
                    m_equiped = equiped,
                    m_quality = quality,
                    m_variant = variant,
                    m_crafterID = crafterID,
                    m_crafterName = crafterName,
                    m_shared = new ItemDrop.ItemData.SharedData() {
                        m_name = name,
                        m_maxStackSize = 20
                    }
                };
                __result = __instance.AddItem(itemData, itemData.m_stack, pos.x, pos.y);
                return false;
            }
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotExactAmountAsContainer() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item", 5, 10), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(false); // no item
            package.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotFewerAmountAsContainer() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item", 5, 10), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(3); // drag amount
            package.Write(false); // no item
            package.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(3, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(2, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotMoreAmountAsContainer() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item", 5, 10), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(7); // drag amount
            package.Write(false); // no item
            package.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveToEmptyInventorySlotItemNotInContainer() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(false); // no item
            package.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.False(success);
            Assert.AreEqual(0, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveDifferentItemToInventory() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(true); // switch with item
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item B", 3, 15), package, true); //item to switch

            package.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.True(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual("my item B", inventory.m_inventory[0].m_shared.m_name);
            Assert.AreEqual(3, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveDifferentItemToInventoryWithTrySplit() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 10), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(3); // drag amount
            package.Write(true); // switch with item
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item B", 3, 15), package, true); //item to switch

            package.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.False(success);
            Assert.AreEqual(0, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual("my item A", inventory.m_inventory[0].m_shared.m_name);
            Assert.AreEqual(5, inventory.m_inventory[0].m_stack);
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemToInventoryCanStack() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(true); // switch with item
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item A", 5, 20), package, true); //item to stack

            package.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(5, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(0, inventory.m_inventory.Count);
        }

        [Test]
        public void RPC_RequestItemRemoveSameItemToInventoryCanStackNotAll() {
            Inventory inventory = new Inventory("inventory", null, 4, 5);
            inventory.AddItem(Helper.CreateItem("my item A", 5, 20), 5, 2, 2);

            ZPackage package = new ZPackage();
            package.Write(new Vector2i(2, 2)); // from container pos
            package.Write(new Vector2i(4, 4)); // to inventory pos
            package.Write(5); // drag amount
            package.Write(true); // switch with item
            InventoryHelper.WriteItemToPackage(Helper.CreateItem("my item A", 19, 20), package, true); //item to stack

            package.SetPos(0);

            ZPackage response = inventory.RPC_RequestItemRemove(0L, package);
            response.SetPos(0);

            bool success = response.ReadBool();
            int addedAmount = response.ReadInt();
            bool hasSwitched = response.ReadBool();

            Assert.True(success);
            Assert.AreEqual(1, addedAmount);
            Assert.False(hasSwitched);

            Assert.AreEqual(1, inventory.m_inventory.Count);
            Assert.AreEqual(4, inventory.m_inventory[0].m_stack);
        }
    }
}
