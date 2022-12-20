using System;
using MasterBuilder.Attributes;
using UnityEngine;

namespace MasterBuilder.BuildIn
{
    [Serializable]
    [Master(Contexts = new[] { "ja-JP", "en-US" })]
    public abstract class LocalizeString : MasterDefinition<string>
    {
        [field: SerializeField]
        [MasterColumn(IsContextSwitch = true)]
        public string Value { get; private set; }
    }
}