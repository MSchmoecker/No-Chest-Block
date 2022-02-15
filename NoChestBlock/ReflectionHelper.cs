using System.Reflection;

namespace NoChestBlock {
    public static class ReflectionHelper {
        private const BindingFlags allFlags = (BindingFlags)(-1);

        public static bool IsType(this object target, string name) {
            return target.GetType().Name == name;
        }

        public static bool HasField(this object target, string name) {
            return target.GetType().GetField(name, allFlags) != null;
        }

        public static T GetField<T>(this object target, string name) {
            return (T)target.GetType().GetField(name, allFlags)?.GetValue(target);
        }
    }
}
