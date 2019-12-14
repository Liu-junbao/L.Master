using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class GenericNameAttribute:Attribute
    {
        public GenericNameAttribute() { }
        public GenericNameAttribute(string name)
        {
            Name = name;
        }
        public GenericNameAttribute(string name,string kind)
        {
            Name = name;
            Kind = kind;
        }
        public GenericNameAttribute(string name, string kind, bool isReadOnly)
        {
            Name = name;
            Kind = kind;
            IsReadOnly = isReadOnly;
        }
        public string Name { get; set; }
        public string Kind { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
