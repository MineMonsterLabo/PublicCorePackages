using System;
using MasterBuilder.Attributes;
using UnityEngine;

namespace MasterBuilder
{
    [Serializable]
    public class MasterDefinition<TKey>
    {
        [field: SerializeField]
        [MasterColumn(Order = int.MinValue)]
        public TKey Key { get; private set; }
    }
}