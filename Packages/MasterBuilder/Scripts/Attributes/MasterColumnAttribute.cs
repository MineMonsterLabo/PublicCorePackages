using System;

namespace MasterBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MasterColumnAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string IsContextSwitch { get; set; }
        public string[] DisableContexts { get; set; }
    }
}