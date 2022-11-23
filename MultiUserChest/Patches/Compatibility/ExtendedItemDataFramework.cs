using System;
using System.Reflection;
using BepInEx.Bootstrap;

namespace MultiUserChest.Patches.Compatibility {
    public static class ExtendedItemDataFramework {
        private static ConstructorInfo ExtendedItemDataConstructor { get; } = FindExtendedItemDataConstructor();

        public static ItemDrop.ItemData CreateExtendedItemData(ItemDrop.ItemData itemData) {
            object[] parameter = {
                itemData,
                itemData.m_stack,
                itemData.m_durability,
                itemData.m_gridPos,
                itemData.m_equiped,
                itemData.m_quality,
                itemData.m_variant,
                itemData.m_crafterID,
                itemData.m_crafterName
            };

            return (ItemDrop.ItemData)ExtendedItemDataConstructor?.Invoke(parameter);
        }

        private static ConstructorInfo FindExtendedItemDataConstructor() {
            const string typeName = "ExtendedItemDataFramework.ExtendedItemData, ExtendedItemDataFramework";
            Type type = Type.GetType(typeName);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            Type[] parameterTypes = {
                typeof(ItemDrop.ItemData),
                typeof(int),
                typeof(float),
                typeof(Vector2i),
                typeof(bool),
                typeof(int),
                typeof(int),
                typeof(long),
                typeof(string)
            };

            return type?.GetConstructor(flags, null, CallingConventions.HasThis, parameterTypes, null);
        }
    }
}
