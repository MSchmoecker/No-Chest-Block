﻿using BepInEx;
using HarmonyLib;

namespace NoChestBlock {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "MultiUserChest";
        public const string ModGuid = "com.maxsch.valheim.MultiUserChest";
        public const string ModVersion = "0.1.2";

        public static Plugin Instance { get; private set; }

        private Harmony harmony;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            InvokeRepeating(nameof(UpdateAccessedContainer), 0, 0.1f);
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
