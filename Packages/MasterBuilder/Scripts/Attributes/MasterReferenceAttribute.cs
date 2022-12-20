using System;

namespace MasterBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MasterReferenceAttribute : Attribute
    {
        public Type ReferenceType { get; set; }

        public string ReferenceKeyName { get; set; }

        public string DisplayColumnName { get; set; }
    }
}