using System.Collections.Generic;
using UnityEngine;

namespace MasterBuilder.BuildIn
{
    public class LocalizeStringCollection : MasterDefinitionCollectionBase<string, LocalizeString>
    {
        [SerializeField] private List<LocalizeString> _innerCollection = new List<LocalizeString>();

        protected override List<LocalizeString> InnerCollection => _innerCollection;
    }
}