using System;

namespace MasterBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MasterAttribute : Attribute
    {
        public string Name { get; }
    }
}