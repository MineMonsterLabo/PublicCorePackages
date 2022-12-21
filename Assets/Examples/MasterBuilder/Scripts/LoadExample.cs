using UnityEngine;

namespace MasterBuilder.Examples
{
    public static class LoadExample
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            var stringCollections = Resources.LoadAll<ExampleLocalizeStringCollection>("Masters/Master");
            var itemCollection = Resources.Load<ItemCollection>("Masters/Master2");
        }
    }
}