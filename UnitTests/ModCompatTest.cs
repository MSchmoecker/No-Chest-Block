using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Bootstrap;
using HarmonyLib;
using MultiUserChest;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class ModCompatTest {
        private Harmony harmony;

        [OneTimeSetUp]
        public void OneTimeSetup() {
            harmony = new Harmony("ModCompatTest");
        }

        [SetUp]
        public void Setup() {
            harmony.UnpatchSelf();
            Chainloader.PluginInfos.Clear();
        }

        [Test]
        public void NoCompatHasHarmonyPatchAttribute() {
            foreach (Type type in typeof(Plugin).Module.GetTypes()) {
                if (type.Namespace == "MultiUserChest.Patches.Compatibility") {
                    HasNoHarmonyPatchAttribute(type);
                }
            }
        }

        [Test]
        public void NoCompatApplied() {
            Assert.DoesNotThrow(Plugin.ApplyModPatches);
        }

        [Test]
        public void QuickDepositIsApplied() {
            Chainloader.PluginInfos.Add("com.MaGic.QuickDeposit", null);
            FileNotFoundException exception = Assert.Throws<FileNotFoundException>(Plugin.ApplyModPatches, "QuickDeposit not patched");
            Assert.IsTrue(exception.Message.Contains("QuickDeposit"));
        }

        [Test]
        public void QuickStackIsApplied() {
            Chainloader.PluginInfos.Add("org.bepinex.plugins.valheim.quick_stack", null);
            FileNotFoundException exception = Assert.Throws<FileNotFoundException>(Plugin.ApplyModPatches, "QuickStack not patched");
            Assert.IsTrue(exception.Message.Contains("QuickStack"));
        }

        [Test]
        public void ValheimAutoSortIsApplied() {
            Chainloader.PluginInfos.Add("ch.elusia.plugins.valheim.autosort", null);
            FileNotFoundException exception = Assert.Throws<FileNotFoundException>(Plugin.ApplyModPatches, "ValheimAutoSort not patched");
            Assert.IsTrue(exception.Message.Contains("ValheimAutoSort"));
        }

        private static void HasNoHarmonyPatchAttribute(Type type) {
            Assert.False(type.CustomAttributes.Any(i => i.AttributeType.IsEquivalentTo(typeof(HarmonyPatch))),
                $"{type.Name} has [HarmonyPatch] Attribute");
        }
    }
}
