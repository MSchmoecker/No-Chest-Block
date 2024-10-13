using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Bootstrap;
using HarmonyLib;
using MultiUserChest;
using MultiUserChest.Patches;
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

        [Test]
        public void IgnoreZdoFlaggedContainer() {
            Inventory containerInvA = new Inventory("container", null, 5, 5);
            Inventory containerInvB = new Inventory("container", null, 5, 5);
            Container containerA = Helper.CreateContainer(containerInvA);
            Container containerB = Helper.CreateContainer(containerInvB);

            containerInvB.CreateItem("item A", 5, 0, 0);
            containerB.m_nview.GetZDO().Set("MUC_Ignore", true);

            bool intercepted = InventoryPatch.InterceptAddItem(containerInvA, containerInvB.GetItemAt(0, 0), 5, new Vector2i(1, 1), out bool successfulAdded);

            Assert.False(intercepted);
            Assert.False(successfulAdded);
        }
    }
}
