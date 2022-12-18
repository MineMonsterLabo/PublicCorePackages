using System;

namespace MasterBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MasterAssetAttribute : Attribute
    {
        public string AssetPath { get; set; } = "Assets/Resources/Masters/Master";

        public string[] Contexts { get; set; } = { "en-US", "ja-JP" };
    }
}