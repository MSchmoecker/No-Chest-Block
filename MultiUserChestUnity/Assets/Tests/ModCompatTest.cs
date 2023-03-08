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

        private static void HasNoHarmonyPatchAttribute(Type type) {
            Assert.False(type.CustomAttributes.Any(i => i.AttributeType.IsEquivalentTo(typeof(HarmonyPatch))),
                $"{type.Name} has [HarmonyPatch] Attribute");
        }
    }
}
