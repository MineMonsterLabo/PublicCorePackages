using System;

namespace GroupEditor
{
    public class GroupAttribute : Attribute
    {
        public string Name { get; }
        public int Order { get; }

        public GroupAttribute(string name, int order = 0)
        {
            Name = name;
            Order = order;
        }
    }
}