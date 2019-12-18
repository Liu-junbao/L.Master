using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace System
{
    public struct EFDisplayPropertyInfo
    {
        public EFDisplayPropertyInfo(PropertyInfo info, GenericNameAttribute attribute, GenericNameAttribute entityAttibute)
        {
            PropertyName = info.Name;
            PropertyType = info.PropertyType;
            GenericName = string.IsNullOrEmpty(attribute.Name) ? PropertyName : attribute.Name;
            IsReadOnly = attribute.IsReadOnly != null ? attribute.IsReadOnly.Value : entityAttibute?.IsReadOnly == true;
        }
        public EFDisplayPropertyInfo(string propertyName, Type propertyType, string genericName, bool isReadOnly)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            GenericName = genericName;
            IsReadOnly = isReadOnly;
        }
        public string PropertyName { get; }
        public Type PropertyType { get; }
        public string GenericName { get; }
        public bool IsReadOnly { get; }
    }
}
