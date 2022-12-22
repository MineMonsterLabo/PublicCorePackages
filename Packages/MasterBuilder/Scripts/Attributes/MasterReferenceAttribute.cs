using System;

namespace MasterBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MasterReferenceAttribute : Attribute
    {
        public Type ReferenceType { get; set; }

        public int ReferenceKeyColumnIndex { get; set; }

        public int DisplayColumnIndex { get; set; }
    }
}