using NoChestBlock;
using HarmonyLib;
using NUnit.Framework;

namespace UnitTests {
    [SetUpFixture]
    public class Patches {
        [OneTimeSetUp]
        public void GlobalSetup() {
            Harmony harmony = new Harmony("id");
            harmony.PatchAll(typeof(ZLogPatch));
            harmony.PatchAll(typeof(LogPatch));
            harmony.PatchAll(typeof(InventoryAddItemPatch));
            harmony.PatchAll(typeof(WriteItemToPackageNoObjectDB));
        }

        [HarmonyPatch]
        public static class ZLogPatch {
            [HarmonyPatch(typeof(ZLog), nameof(ZLog.Log)), HarmonyPrefix]
            public static bool NoZLogPatch(object o) {
                System.Console.WriteLine(o);
                return false;
            }
        }

        [HarmonyPatch]
        public static class LogPatch {
            [HarmonyPatch(typeof(Log), nameof(Log.LogInfo)), HarmonyPrefix]
            public static bool NoLogPatch(object data) {
                System.Console.WriteLine(data);
                return false;
            }

            [HarmonyPatch(typeof(Log), nameof(Log.LogWarning)), HarmonyPrefix]
            public static bool NoWarningPatch(object data) {
                System.Console.WriteLine(data);
                return false;
            }
        }

        [HarmonyPatch]
        public static class InventoryAddItemPatch {
            [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(string), typeof(int), typeof(float), typeof(Vector2i),
                          typeof(bool), typeof(int), typeof(int), typeof(long), typeof(string)), HarmonyPrefix]
            public static bool AddNoMonoBehaviourPatch(Inventory __instance, ref bool __result, string name, int stack, float durability,
                Vector2i pos,
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

        [HarmonyPatch]
        public static class WriteItemToPackageNoObjectDB {
            [HarmonyPatch(typeof(InventoryHelper), nameof(InventoryHelper.GetItemDataFromObjectDB)), HarmonyPrefix]
            public static bool WriteItemToPackagePatch(ref ItemDrop.ItemData __result, string name) {
                __result = new ItemDrop.ItemData() {
                    m_shared = new ItemDrop.ItemData.SharedData() {
                        m_name = name,
                        m_maxStackSize = 20,
                    }
                };

                return false;
            }
        }
    }
}
