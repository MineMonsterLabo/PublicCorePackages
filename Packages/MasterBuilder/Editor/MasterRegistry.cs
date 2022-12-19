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
        private static readonly Dictionary<Type, Type> _linkedCollectionTypes = new Dictionary<Type, Type>();

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

            var innerType = type.GetMethod("GetEnumerator")?.ReturnType.GetGenericArguments()[0];
            if (innerType == null)
                throw new ArgumentException("`GetEnumerator` not found.");

            var masterAttribute = innerType.GetCustomAttribute<MasterAttribute>();
            var masterName = innerType.Name[..(innerType.Name.Length <= 15 ? innerType.Name.Length : 15)];
            if (masterAttribute != null && !string.IsNullOrWhiteSpace(masterAttribute.Name))
            {
                masterName = masterAttribute.Name[..(innerType.Name.Length <= 15 ? innerType.Name.Length : 15)];
            }

            _masterTypes[masterName] = innerType;
            _linkedCollectionTypes[innerType] = type;
        }

        public static Type GetMasterCollectionType(Type type)
        {
            if (!_linkedCollectionTypes.ContainsKey(type))
                return null;

            return _linkedCollectionTypes[type];
        }
    }
}