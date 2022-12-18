using System;

namespace MasterBuilder.BuildIn
{
    [Serializable]
    public class LocalizeString : MasterDefinition<string>
    {
        public string Locale { get; private set; }
        public string Value { get; private set; }
    }
}