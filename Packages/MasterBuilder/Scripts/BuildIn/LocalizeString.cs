using System;
using MasterBuilder.Attributes;

namespace MasterBuilder.BuildIn
{
    [Serializable]
    [Master(Contexts = new[] { "ja-JP", "en-US" })]
    public class LocalizeString : MasterDefinition<string>
    {
        [MasterColumn(IsContextSwitch = true)] public string Value { get; private set; }
    }
}