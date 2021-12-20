using ChestFix;
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
            harmony.PatchAll(typeof(WriteItemToPackageAlwaysNameHackPatch));
            harmony.PatchAll(typeof(LoadItemFromPackageAlwaysNameHackPatch));
        }

        [HarmonyPatch]
        public static class ZLogPatch {
            [HarmonyPatch(typeof(ZLog), nameof(ZLog.Log)), HarmonyPrefix]
            public static bool NoZLogPatch(object o) {
                System.Console.Write(o);
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
        public static class WriteItemToPackageAlwaysNameHackPatch {
            [HarmonyPatch(typeof(InventoryHelper), nameof(InventoryHelper.WriteItemToPackage)), HarmonyPrefix]
            public static void WriteItemToPackagePatch(ref bool nameHack) {
                nameHack = true;
            }
        }

        [HarmonyPatch]
        public static class LoadItemFromPackageAlwaysNameHackPatch {
            [HarmonyPatch(typeof(InventoryHelper), nameof(InventoryHelper.LoadItemFromPackage)), HarmonyPrefix]
            public static void LoadItemFromPackagePatch(ref bool nameHack) {
                nameHack = true;
            }
        }
    }
}
