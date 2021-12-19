using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace ChestFix {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "ChestFix";
        public const string ModGuid = "com.maxsch.valheim.ChestFix";
        public const string ModVersion = "0.0.0";

        public static Plugin Instance { get; private set; }
        public static AssetBundle AssetBundle { get; private set; }

        private Harmony harmony;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();
        }
    }
}
