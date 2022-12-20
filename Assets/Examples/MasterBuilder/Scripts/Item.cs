using System;
using MasterBuilder.Attributes;
using UnityEngine;

namespace MasterBuilder.Examples
{
    [Serializable]
    [MasterAsset(AssetPath = "Assets/Examples/MasterBuilder/Resources/Masters/Master2")]
    public class Item : MasterDefinition<int>
    {
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Description { get; set; }

        [field: SerializeField]
        [MasterReference(ReferenceType = typeof(ItemType), ReferenceKeyName = nameof(ItemType.Key),
            DisplayColumnName = nameof(ItemType.Name))]
        public int ItemTypeId { get; set; }
    }
}