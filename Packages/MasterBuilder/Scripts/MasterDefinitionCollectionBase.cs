using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MasterBuilder
{
    public abstract class MasterDefinitionCollectionBase<TKey, TMasterDefine> : ScriptableObject,
        ICollection<TMasterDefine>, IMasterDefinitionCollection where TMasterDefine : MasterDefinition<TKey>
    {
        protected abstract List<TMasterDefine> InnerCollection { get; }

        public int Count => InnerCollection.Count;
        public bool IsReadOnly => false;

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