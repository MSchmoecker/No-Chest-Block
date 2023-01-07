using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using MultiUserChest;
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
            harmony.PatchAll(typeof(DropPatch));
            harmony.PatchAll(typeof(PathsPatches));
            harmony.PatchAll(typeof(MultiUserChest.Patches.PickupPatch));
            harmony.PatchAll(typeof(MultiUserChest.Patches.TombStonePatch));
        }

        private static class ZLogPatch {
            [HarmonyPatch(typeof(ZLog), nameof(ZLog.Log)), HarmonyPrefix]
            public static bool NoZLogPatch(object o) {
                System.Console.WriteLine(o);
                return false;
            }
        }

        private static class LogPatch {
            [HarmonyPatch(typeof(Log), nameof(Log.LogDebug)), HarmonyPrefix]
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

        private static class InventoryAddItemPatch {
            [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(string), typeof(int), typeof(float), typeof(Vector2i),
                          typeof(bool), typeof(int), typeof(int), typeof(long), typeof(string), typeof(Dictionary<string, string>)), HarmonyPrefix]
            public static bool AddNoMonoBehaviourPatch(Inventory __instance, ref bool __result, string name, int stack, float durability,
                Vector2i pos, bool equiped, int quality, int variant, long crafterID, string crafterName, Dictionary<string, string> customData) {
                ItemDrop.ItemData itemData = new ItemDrop.ItemData() {
                    m_stack = stack,
                    m_durability = durability,
                    m_equiped = equiped,
                    m_quality = quality,
                    m_variant = variant,
                    m_crafterID = crafterID,
                    m_crafterName = crafterName,
                    m_customData = customData,
                    m_shared = new ItemDrop.ItemData.SharedData() {
                        m_name = name,
                        m_maxStackSize = 20
                    }
                };
                __instance.AddItem(itemData, itemData.m_stack, pos.x, pos.y);
                __result = true;
                return false;
            }
        }

        public static class DropPatch {
            public static event Action<ItemDrop.ItemData, int> OnDrop;

            [HarmonyPatch(typeof(InventoryHandler), nameof(InventoryHandler.DropItem), typeof(ItemDrop.ItemData), typeof(int)), HarmonyPrefix]
            private static bool NoDrop(ItemDrop.ItemData item, int amount) {
                OnDrop?.Invoke(item, amount);
                return false;
            }
        }

        private static class PathsPatches {
            private static string BepInExConfigPath = "";

            [HarmonyPatch(typeof(Paths), nameof(Paths.BepInExConfigPath), MethodType.Getter), HarmonyPostfix]
            public static void PatchPluginInfos(ref string __result) {
                if (BepInExConfigPath == "") {
                    BepInExConfigPath = Path.GetTempFileName();
                }

                __result = BepInExConfigPath;
            }
        }
    }
}
