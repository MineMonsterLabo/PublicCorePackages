using System;
using System.Collections.Generic;
using MasterBuilder.Attributes;
using UnityEngine;

namespace MasterBuilder.Examples
{
    public class ItemCollection : MasterDefinitionCollectionBase<int, Item>
    {
        [SerializeField] private List<Item> _innerCollection = new List<Item>();

        protected override List<Item> InnerCollection => _innerCollection;
    }

    [Serializable]
    [MasterAsset(AssetPath = "Assets/Examples/MasterBuilder/Resources/Masters/Master2")]
    public class Item : MasterDefinition<int>
    {
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Description { get; set; }

        [field: SerializeField]
        [MasterReference(ReferenceType = typeof(ItemType), ReferenceKeyColumnIndex = 0, DisplayColumnIndex = 1)]
        public int ItemTypeId { get; set; }

        [field: SerializeField]
        [MasterColumn(IsAllowEmpty = true)]
        public string Tag { get; set; }
    }
}