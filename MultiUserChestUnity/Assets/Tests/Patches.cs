using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            harmony.PatchAll(typeof(InventoryAddItemPatch));
            harmony.PatchAll(typeof(DropPatch));
            harmony.PatchAll(typeof(PathsPatches));
            harmony.PatchAll(typeof(ZNetPatches));
            harmony.PatchAll(typeof(SteamManagerPatches));

            harmony.PatchAll(typeof(MultiUserChest.Patches.InventoryPatch));
        }

        private static class InventoryAddItemPatch {
            [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), typeof(string), typeof(int), typeof(float), typeof(Vector2i),
                          typeof(bool), typeof(int), typeof(int), typeof(long), typeof(string), typeof(Dictionary<string, string>), typeof(int), typeof(bool))]
            [HarmonyPrefix]
            public static bool AddNoMonoBehaviourPatch(Inventory __instance, ref bool __result, string name, int stack, float durability,
                Vector2i pos, bool equipped, int quality, int variant, long crafterID, string crafterName, Dictionary<string, string> customData,
                int worldLevel, bool pickedUp) {

                ItemDrop.ItemData itemData = new ItemDrop.ItemData() {
                    m_stack = stack,
                    m_durability = durability,
                    m_equipped = equipped,
                    m_quality = quality,
                    m_variant = variant,
                    m_crafterID = crafterID,
                    m_crafterName = crafterName,
                    m_customData = customData,
                    m_shared = new ItemDrop.ItemData.SharedData() {
                        m_name = name,
                        m_maxStackSize = 20
                    },
                    m_pickedUp = pickedUp,
                    m_worldLevel = worldLevel,
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

        public static class ZNetPatches {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ZNetView), nameof(ZNetView.InvokeRPC), new[] { typeof(long), typeof(string), typeof(object[]) })]
            [HarmonyPatch(typeof(ZNetView), nameof(ZNetView.InvokeRPC), new[] { typeof(string), typeof(object[]) })]
            public static bool NoZNetViewInvokeRPC(ZNetView __instance, string method, params object[] parameters) {
                if (ZRoutedRpc.s_instance == null) {
                    ZRoutedRpc.s_instance = new ZRoutedRpc(false);
                }

                ZNetSimulate.routedRpcs.Enqueue(new ZNetSimulate.RoutedNetViewRpc() {
                    netView = __instance,
                    method = method,
                    parameters = parameters,
                });

                return false;
            }
        }

        public static class SteamManagerPatches {
            [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Awake)), HarmonyPrefix]
            public static bool NoSteamManagerAwake() {
                return false;
            }

            [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.InitializeSteam)), HarmonyPrefix]
            public static bool NoFejdStartupInitializeSteam() {
                return false;
            }
        }

        public static class PreventAddItem {
            public static bool Enabled { get; private set; }

            private static MethodInfo[] methodTargets = AccessTools.GetDeclaredMethods(typeof(Inventory))
                .Where(i => i.Name == nameof(Inventory.AddItem) && i.ReturnType == typeof(bool))
                .ToArray();

            private static Harmony harmony = new Harmony("prevent-add-item");

            public static void Enable() {
                if (Enabled) {
                    throw new Exception("Already enabled");
                }

                Enabled = true;

                foreach (MethodInfo methodInfo in methodTargets) {
                    harmony.Patch(methodInfo, new HarmonyMethod(typeof(PreventAddItem), nameof(PreventAddItemPatch)));
                }
            }

            public static void Disable() {
                if (!Enabled) {
                    throw new Exception("Already disabled");
                }

                Enabled = false;

                foreach (MethodInfo methodInfo in methodTargets) {
                    harmony.Unpatch(methodInfo, AccessTools.Method(typeof(PreventAddItem), nameof(PreventAddItemPatch)));
                }
            }

            public static bool PreventAddItemPatch(Inventory __instance, ref bool __result) {
                Log.LogInfo("AddItem: Prevented");
                __result = false;
                return false;
            }
        }
    }
}
