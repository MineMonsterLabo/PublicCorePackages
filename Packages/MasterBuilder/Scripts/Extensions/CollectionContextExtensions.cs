using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MasterBuilder.Extensions
{
    public static class CollectionContextExtensions
    {
        public static IMasterCollection<TKey, TMasterDefine>
            Context<T, TKey, TMasterDefine>(this IEnumerable<T> contexts, string contextName)
            where T : ScriptableObject, IMasterCollection<TKey, TMasterDefine>
            where TKey : IComparable<TKey>, IEquatable<TKey>
            where TMasterDefine : MasterDefinition<TKey>
        {
            return contexts.FirstOrDefault(e =>
            {
                var splitName = e.name.Split("_");
                if (splitName.Length != 2)
                    return false;

                return splitName[1] == contextName;
            });
        }
    }
}