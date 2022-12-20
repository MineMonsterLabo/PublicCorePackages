using System.Collections.Generic;
using UnityEngine;

namespace MasterBuilder.Examples
{
    public class ItemCollection : MasterDefinitionCollectionBase<int, Item>
    {
        [SerializeField] private List<Item> _innerCollection = new List<Item>();

        protected override List<Item> InnerCollection => _innerCollection;
    }
}