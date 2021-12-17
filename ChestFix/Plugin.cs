using System.Reflection;
using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using UnityEngine;
using Jotunn.Utils;
using Jotunn.Managers;

namespace ChestFix {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "ChestFix";
        public const string ModGuid = "com.maxsch.valheim.ChestFix";
        public const string ModVersion = "0.0.0";

        public static Plugin Instance { get; private set; }
        public static AssetBundle AssetBundle { get; private set; }

        private Harmony harmony;

        private void Awake() {
            Instance = this;

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();

            // load embedded asset bundle
            // AssetBundle = AssetUtils.LoadAssetBundleFromResources("ChestFix_AssetBundle", Assembly.GetExecutingAssembly());

            // load embedded localisation
            CustomLocalization localization = new CustomLocalization();
            string englishJson = AssetUtils.LoadTextFromResources("Localization.English.json", Assembly.GetExecutingAssembly());
            localization.AddJsonFile("English", englishJson);
            LocalizationManager.Instance.AddLocalization(localization);
        }
    }
}
