using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MasterBuilder
{
    public abstract class
        MasterCollectionBase<TKey, TMasterDefine> : ScriptableObject, IMasterCollection<TKey, TMasterDefine>
        where TKey : IComparable<TKey>, IEquatable<TKey> where TMasterDefine : MasterDefinition<TKey>
    {
        protected Dictionary<TKey, TMasterDefine> CacheTable { get; set; } = new Dictionary<TKey, TMasterDefine>();

        protected abstract List<TMasterDefine> InnerCollection { get; }

        public int Count => InnerCollection.Count;
        public bool IsReadOnly => false;

        public void CreateAllCache()
        {
            CacheTable = InnerCollection.ToDictionary(e => e.Key, e => e);
        }

        public TMasterDefine Get(TKey key)
        {
            if (CacheTable.TryGetValue(key, out var cacheValue))
                return cacheValue;

            var value = InnerCollection.First(e => e.Key.Equals(key));
            CacheTable.TryAdd(key, value);
            return value;
        }

        public TMasterDefine GetOrNull(TKey key)
        {
            if (CacheTable.TryGetValue(key, out var cacheValue))
                return cacheValue;

            var value = InnerCollection.FirstOrDefault(e => e.Key.Equals(key));
            if (value != null)
                CacheTable.TryAdd(key, value);

            return value;
        }

        public IEnumerator<TMasterDefine> GetEnumerator()
        {
            return InnerCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TMasterDefine item)
        {
            InnerCollection.Add(item);
        }

        public void Clear()
        {
            InnerCollection.Clear();
        }

        public bool Contains(TMasterDefine item)
        {
            return InnerCollection.Contains(item);
        }

        public void CopyTo(TMasterDefine[] array, int arrayIndex)
        {
            InnerCollection.CopyTo(array, arrayIndex);
        }

        public bool Remove(TMasterDefine item)
        {
            return InnerCollection.Remove(item);
        }
    }
}