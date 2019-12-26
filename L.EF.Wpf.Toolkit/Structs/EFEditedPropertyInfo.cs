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
        public EFEditedPropertyInfo(Type modelType, EFDisplayPropertyInfo displayPropertyInfo,object editedValue)
        {
            PropertyName = displayPropertyInfo.PropertyName;
            PropertyType = displayPropertyInfo.PropertyType;
            GenericName = displayPropertyInfo.GenericName;
            EditedValue = editedValue;
            var argExp = Expression.Parameter(typeof(object));
            var modelExp = Expression.TypeAs(argExp,modelType);
            var propertyExp = Expression.Property(modelExp, PropertyName);
            _getValue = Expression.Lambda<Func<object, object>>(propertyExp,argExp);
            var arg1Exp = Expression.Parameter(typeof(object));
            var valueExp = Expression.TypeAs(arg1Exp, PropertyType);
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
