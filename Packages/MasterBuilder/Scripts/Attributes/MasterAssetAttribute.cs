using System;

namespace MasterBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MasterAssetAttribute : Attribute
    {
        public string AssetPath { get; set; } = "Assets/Resources/Masters/Master";
    }
}