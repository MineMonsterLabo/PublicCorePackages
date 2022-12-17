using System.Collections.Generic;

namespace Localize
{
    public abstract class LocalizeBase
    {
        private readonly Dictionary<string, string> _dynamicKeyValue = new Dictionary<string, string>();

        public string Get(string key)
        {
            if (!_dynamicKeyValue.ContainsKey(key))
                return $"Key `{key}` is not found.";

            return _dynamicKeyValue[key];
        }

        public void Add(string key, string value)
        {
            if (_dynamicKeyValue.ContainsKey(key))
                return;

            _dynamicKeyValue.Add(key, value);
        }
    }
}