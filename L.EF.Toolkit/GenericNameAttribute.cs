using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class GenericNameAttribute : Attribute
    {
        public GenericNameAttribute() { }
        public GenericNameAttribute(string name) : this(name, null, true) { }
        public GenericNameAttribute(string name, string kind) : this(name, kind, true) { }
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
