using System;
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

        public static T InvokeMethod<T>(this object target, string name, params object[] parameter) {
            parameter = parameter.Length == 0 ? null : parameter;
            return (T)target.GetType().GetMethod(name, allFlags)?.Invoke(target, parameter);
        }

        public static T InvokeStaticMethod<T>(string type, string name, params object[] parameter) {
            parameter = parameter.Length == 0 ? null : parameter;
            return (T)Type.GetType(type)?.GetMethod(name, allFlags)?.Invoke(null, parameter);
        }
    }
}
