using System.Collections.Generic;
using UnityEngine;

namespace MasterBuilder.Examples
{
    public class ItemTypeCollection : MasterDefinitionCollectionBase<int, ItemType>
    {
        [SerializeField] private List<ItemType> _innerCollection = new List<ItemType>();

        protected override List<ItemType> InnerCollection => _innerCollection;
    }
}