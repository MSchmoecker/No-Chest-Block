using BepInEx;
using HarmonyLib;

namespace NoChestBlock {
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin {
        public const string ModName = "MultiUserChest";
        public const string ModGuid = "com.maxsch.valheim.MultiUserChest";
        public const string ModVersion = "0.1.1";

        public static Plugin Instance { get; private set; }

        private Harmony harmony;

        private void Awake() {
            Instance = this;
            Log.Init(Logger);

            harmony = new Harmony(ModGuid);
            harmony.PatchAll();
        }
    }
}
