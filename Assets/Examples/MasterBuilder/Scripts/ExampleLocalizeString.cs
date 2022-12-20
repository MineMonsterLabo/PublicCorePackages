using System;
using MasterBuilder.Attributes;
using MasterBuilder.BuildIn;

namespace MasterBuilder.Examples
{
    [Serializable]
    [MasterAsset(AssetPath = "Assets/Examples/MasterBuilder/Resources/Masters/Master")]
    [Master(Contexts = new[] { "ja-JP", "en-US" })]
    public class ExampleLocalizeString : LocalizeString
    {
    }
}