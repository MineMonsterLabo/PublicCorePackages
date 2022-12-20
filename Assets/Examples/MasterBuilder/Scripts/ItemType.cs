using System;
using MasterBuilder.Attributes;
using UnityEngine;

namespace MasterBuilder.Examples
{
    [Serializable]
    [MasterAsset(AssetPath = "Assets/Examples/MasterBuilder/Resources/Masters/Master2")]
    public class ItemType : MasterDefinition<int>
    {
        [field: SerializeField] public string Name { get; set; }
    }
}