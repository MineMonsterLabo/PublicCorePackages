using System;
using System.Collections.Generic;
using System.Reflection;
using MasterBuilder.Attributes;
using UnityEngine;

namespace MasterBuilder.Editor
{
    public static class MasterRegistry
    {
        private static readonly Dictionary<string, Type> _masterTypes = new Dictionary<string, Type>();

        public static IReadOnlyDictionary<string, Type> MasterTypes => _masterTypes;

        public static void RegisterMasterTypeFromAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!typeof(IMasterDefinitionCollection).IsAssignableFrom(type))
                    continue;

                if (!typeof(ScriptableObject).IsAssignableFrom(type))
                    continue;

                RegisterMasterType(type);
            }
        }

        public static void RegisterMasterType(Type type)
        {
            if (!typeof(ScriptableObject).IsAssignableFrom(type))
                throw new ArgumentException(
                    $"The type `{type.FullName}` could not be assigned to a `ScriptableObject`.");

            var masterAttribute = type.GetCustomAttribute<MasterAttribute>();
            var masterName = type.Name.Substring(0, 15);
            if (masterAttribute != null)
            {
                masterName = masterAttribute.Name;
            }

            _masterTypes[masterName] = type;
        }
    }
}