using System;
using System.Reflection.Emit;
using HarmonyLib;

// Credits to ASharpPen!
// https://github.com/ASharpPen/Valheim.SpawnThat/blob/a753ccc0776b87f9d9721915f5ff57a5f1503873/src/SpawnThat/Utilities/Extensions/CodeMatcherExtensions.cs

namespace NoChestBlock {
    public static class CodeMatcherExtensions {
        public static CodeMatcher GetPosition(this CodeMatcher codeMatcher, out int position) {
            position = codeMatcher.Pos;
            return codeMatcher;
        }

        public static CodeMatcher AddLabel(this CodeMatcher codeMatcher, out Label label) {
            label = new Label();
            codeMatcher.AddLabels(new[] { label });
            return codeMatcher;
        }

        public static CodeMatcher GetOperand(this CodeMatcher codeMatcher, out object operand) {
            operand = codeMatcher.Operand;
            return codeMatcher;
        }

        internal static CodeMatcher Print(this CodeMatcher codeMatcher, int before, int after) {
            for (int i = -before; i <= after; ++i) {
                int currentOffset = i;
                int index = codeMatcher.Pos + currentOffset;

                if (index <= 0) {
                    continue;
                }

                if (index >= codeMatcher.Length) {
                    break;
                }

                try {
                    var line = codeMatcher.InstructionAt(currentOffset);
                    Log.LogDebug($"[{currentOffset}] " + line.ToString());
                } catch (Exception e) {
                    Log.LogDebug(e.Message);
                }
            }

            return codeMatcher;
        }
    }
}
