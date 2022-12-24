using System;
using System.Collections.Generic;

namespace MasterBuilder
{
    public interface IReadOnlyMasterCollection<in TKey, out TMasterDefine> : IReadOnlyCollection<TMasterDefine>
        where TKey : IComparable<TKey>, IEquatable<TKey> where TMasterDefine : MasterDefinition<TKey>
    {
        void CreateAllCache();
        TMasterDefine Get(TKey key);
        TMasterDefine GetOrNull(TKey key);
    }
}