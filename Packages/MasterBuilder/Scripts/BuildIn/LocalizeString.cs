using System;
using MasterBuilder.Attributes;
using UnityEngine;

namespace MasterBuilder.BuildIn
{
    [Serializable]
    public abstract class LocalizeString : MasterDefinition<string>
    {
        [field: SerializeField]
        [MasterColumn(IsContextSwitch = true)]
        public string Value { get; private set; }
    }
}