using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Localize
{
    public class LocalizeContainer
    {
        private readonly Dictionary<string, LocalizeBase> _registeredLocalize =
            new Dictionary<string, LocalizeBase>();

        public T GetDefault<T>() where T : LocalizeBase
        {
            var locale = CultureInfo.CurrentCulture.Name;
            return GetByLocale<T>(locale);
        }

        public T GetByLocale<T>(string locale) where T : LocalizeBase
        {
            if (!_registeredLocalize.ContainsKey(locale))
                return null;

            return _registeredLocalize[locale] as T;
        }

        public void RegisterLocalize(string locale, LocalizeBase localize)
        {
            if (_registeredLocalize.ContainsKey(locale))
                return;

            _registeredLocalize[locale] = localize;
        }

        public void AutoLoadLocalizeTypes(Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(e => typeof(LocalizeBase).IsAssignableFrom(e) && !e.IsAbstract && !e.IsSealed).ToArray();
            foreach (Type type in types)
            {
                var pattern = type.Name.Split("__");
                if (pattern.Length != 2)
                    continue;

                var locale = pattern[1];
                var instance = Activator.CreateInstance(type) as LocalizeBase;
                _registeredLocalize[locale] = instance;
            }
        }
    }
}