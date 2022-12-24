using System;
using System.Collections;
using System.Collections.Generic;

namespace MasterBuilder
{
    public interface IMasterCollection : IEnumerable
    {
    }

    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface IMasterCollection<in TKey, TMasterDefine> : IReadOnlyMasterCollection<TKey, TMasterDefine>,
        IMasterCollection, ICollection<TMasterDefine> where TKey : IComparable<TKey>, IEquatable<TKey>
        where TMasterDefine : MasterDefinition<TKey>
    {
    }
}