using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public struct EFEditedPropertyInfo
    {
        private Expression<Func<object, object>> _getValue;
        private Expression<Action<object, object>> _setValue;
        public EFEditedPropertyInfo(Type modelType, EFDisplayPropertyInfo displayPropertyInfo, object editedValue) : this(modelType, displayPropertyInfo.PropertyName, displayPropertyInfo.PropertyType, editedValue, displayPropertyInfo.GenericName) { }
        public EFEditedPropertyInfo(Type modelType, string propertyName, Type propertyType, object editedValue) : this(modelType, propertyName, propertyType, editedValue, propertyName) { }
        public EFEditedPropertyInfo(Type modelType,string propertyName,Type propertyType,object editedValue,string genericName)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            GenericName = genericName;
            EditedValue = editedValue;
            var argExp = Expression.Parameter(typeof(object));
            var modelExp = Expression.TypeAs(argExp, modelType);
            var propertyExp = Expression.Property(modelExp, propertyName);
            var getPropertyValueExp = Expression.Convert(propertyExp, typeof(object));
            _getValue = Expression.Lambda<Func<object, object>>(getPropertyValueExp, argExp);
            var arg1Exp = Expression.Parameter(typeof(object));
            var valueExp = Expression.Convert(arg1Exp, propertyType);
            var assignExp = Expression.Assign(propertyExp, valueExp);
            _setValue = Expression.Lambda<Action<object, object>>(assignExp, argExp, arg1Exp);
        }
       
        public string PropertyName { get; }
        public Type PropertyType { get; }
        public string GenericName { get; }
        public object EditedValue { get; }
        public object GetValue(object model)
        {
            return _getValue.Compile().Invoke(model);
        }
        public void SetValue(object model,object value)
        {
            _setValue.Compile().Invoke(model, value);
        }
        /// <summary>
        /// 将当前EditedValue写入到model
        /// </summary>
        /// <param name="model"></param>
        public void SetValue(object model)
        {
            _setValue.Compile().Invoke(model, EditedValue);
        }
    }
}
