﻿using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Jotunn.Utils;

namespace MultiUserChest {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInIncompatibility("aedenthorn.QuickStore")]
    [BepInIncompatibility("aedenthorn.SimpleSort")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "MultiUserChest";
        public const string ModGuid = "com.maxsch.valheim.MultiUserChest";
        public const string ModVersion = "0.4.2";

        public static Plugin Instance { get; private set; }
        private static Harmony harmony = new Harmony(ModGuid);

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            harmony.PatchAll();

            InvokeRepeating(nameof(UpdateAccessedContainer), 0, 0.1f);
        }

        private void Start() {
            ApplyModPatches();
        }

        public static void ApplyModPatches() {
            if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.valheim.quick_stack")) {
                harmony.PatchAll(typeof(QuickStackPatch));
            }
        }

        private void UpdateAccessedContainer() {
            if (!Player.m_localPlayer || !Player.m_localPlayer.m_nview.IsValid() || !InventoryGui.instance) {
                return;
            }

            Container container = InventoryGui.instance.m_currentContainer;

            if (container && container.m_nview.IsValid()) {
                Player.m_localPlayer.m_nview.GetZDO().Set("accessed-container", container.m_nview.GetZDO().m_uid);
            } else {
                Player.m_localPlayer.m_nview.GetZDO().Set("accessed-container", ZDOID.None);
            }
        }
    }
}
