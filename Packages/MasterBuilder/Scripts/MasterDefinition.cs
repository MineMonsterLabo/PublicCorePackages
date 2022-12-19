using System;
using UnityEngine;

namespace MasterBuilder
{
    [Serializable]
    public class MasterDefinition<TKey>
    {
        [field: SerializeField] public TKey Key { get; private set; }
    }
}