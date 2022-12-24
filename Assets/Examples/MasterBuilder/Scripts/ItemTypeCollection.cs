using System;
using System.Collections.Generic;
using MasterBuilder.Attributes;
using UnityEngine;

namespace MasterBuilder.Examples
{
    public class ItemTypeCollection : MasterDefinitionCollectionBase<int, ItemType>
    {
        [SerializeField] private List<ItemType> _innerCollection = new List<ItemType>();

        protected override List<ItemType> InnerCollection => _innerCollection;
    }

    [Serializable]
    [MasterAsset(AssetPath = "Assets/Examples/MasterBuilder/Resources/Masters/Master2")]
    [Master(Version = 1)]
    public class ItemType : MasterDefinition<int>
    {
        [field: SerializeField] public string Name { get; set; }
    }
}