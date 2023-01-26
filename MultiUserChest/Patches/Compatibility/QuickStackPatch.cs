using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace MultiUserChest {
    public static class QuickStackPatch {
        [HarmonyPatch("QuickStack.QuickStackPlugin, QuickStack", "StackToMany"), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> QuickStackPatchTranspiler(IEnumerable<CodeInstruction> instructions) {
            return new CodeMatcher(instructions)
                   .MatchForward(false, new CodeMatch[] {
                       new CodeMatch(OpCodes.Ldloc_S),
                       new CodeMatch(i => i.IsVirtCall(nameof(ZNetView), nameof(ZNetView.ClaimOwnership)))
                   })
                   .RemoveInstructions(9)
                   .InstructionEnumeration();
        }
    }
}
