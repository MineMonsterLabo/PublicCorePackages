using System;
using MasterBuilder.Attributes;
using MasterBuilder.Extensions;
using UnityEngine;

namespace MasterBuilder.BuildIn
{
    public class InternalLocalizeStringCollection : LocalizeStringCollection<InternalLocalizeString>
    {
    }

    [Serializable]
    [MasterAsset(AssetPath = "Assets/Resources/MasterBuilder/Localize")]
    [Master(Contexts = new[] { "en-US", "ja-JP" }, Version = 1)]
    public class InternalLocalizeString : LocalizeString
    {
    }

    public static class InternalLocalizeStringCollectionExtensions
    {
        private static InternalLocalizeStringCollection[] _stringCollections;

        public static string InternalLocalizeString(this string key, params object[] args)
        {
            _stringCollections ??= Resources.LoadAll<InternalLocalizeStringCollection>("MasterBuilder/Localize");

            var collection = _stringCollections
                .CultureContext<InternalLocalizeStringCollection, string, InternalLocalizeString>();
            return string.Format(collection.Get(key).Value, args);
        }
    }
}