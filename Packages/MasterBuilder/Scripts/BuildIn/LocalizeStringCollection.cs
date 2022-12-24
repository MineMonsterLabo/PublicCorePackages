using System.Collections.Generic;
using UnityEngine;

namespace MasterBuilder.BuildIn
{
    public abstract class LocalizeStringCollection<TLocalize> : MasterCollectionBase<string, TLocalize>
        where TLocalize : LocalizeString
    {
        [SerializeField] private List<TLocalize> _innerCollection = new List<TLocalize>();

        protected override List<TLocalize> InnerCollection => _innerCollection;
    }
}