using System;
using System.Collections;
using System.Collections.Generic;

namespace MasterBuilder
{
    public interface IMasterDefinitionCollection : IEnumerable
    {
    }

    public interface
        IMasterDefinitionCollection<TKey, TMasterDefine> : IMasterDefinitionCollection, ICollection<TMasterDefine>
        where TKey : IComparable<TKey>, IEquatable<TKey> where TMasterDefine : MasterDefinition<TKey>
    {
    }
}