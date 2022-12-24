using System;
using MasterBuilder.Attributes;

namespace MasterBuilder.BuildIn
{
    public class InternalLocalizeStringCollection : LocalizeStringCollection<InternalLocalizeString>
    {
    }

    [Serializable]
    [MasterAsset(AssetPath = "Assets/Resources/MasterBuilder/Localize")]
    [Master(Contexts = new[] { "en-US", "ja-JP" })]
    public class InternalLocalizeString : LocalizeString
    {
    }
}