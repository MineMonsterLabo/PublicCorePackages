using System;

namespace MasterBuilder
{
    [Serializable]
    public class MasterDefinition<TKey>
    {
        public TKey Key { get; private set; }
    }
}