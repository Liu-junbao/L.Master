using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class GenericNameAttribute:Attribute
    {
        public GenericNameAttribute() { }
        public GenericNameAttribute(string name) : this(name, null, false) { }
        public GenericNameAttribute(string name, string kind) : this(name, kind, false) { }
        public GenericNameAttribute(string name, bool isReadOnly) : this(name, null, isReadOnly) { }
        public GenericNameAttribute(string name, string kind, bool isReadOnly)
        {
            Name = name;
            Kind = kind;
            IsReadOnly = isReadOnly;
        }
        public string Name { get; set; }
        public string Kind { get; set; }
        public bool? IsReadOnly { get; set; }
    }
}
