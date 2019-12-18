//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq.Expressions;
//using System.Text;

//namespace System
//{
//    public interface IPropertyFilterCollection : IEnumerable<EFValueFilter>
//    {
//        int Count { get; }
//        EFDisplayPropertyInfo PropertyInfo { get; }
//    }
//    class PropertyFilterCollection : ObservableCollection<EFValueFilter>, IPropertyFilterCollection
//    {
//        public PropertyFilterCollection(EFDisplayPropertyInfo propertyInfo)
//        {
//            PropertyInfo = propertyInfo;
//        }
//        public EFDisplayPropertyInfo PropertyInfo { get; }
//        public Expression LoadExpression(ParameterExpression modelExp, Expression body = null)
//        {
//            if (body == null)
//                return ComparisonExpression(modelExp);
//            switch (Operation)
//            {
//                case EFOperation.And:
//                    return Expression.AndAlso(body, this.ComparisonExpression(modelExp));
//                case EFOperation.Or:
//                    return Expression.OrElse(body, this.ComparisonExpression(modelExp));
//                default:
//                    break;
//            }
//            return Expression.Add(body, this.ComparisonExpression(modelExp));
//        }
//        private Expression ComparisonExpression(ParameterExpression modelExp, EFValueFilter filter)
//        {
//            var propertyExp = Linq.Expressions.Expression.Property(modelExp, PropertyInfo.PropertyName);
//            var valueExp = Linq.Expressions.Expression.Constant(filter.ComparisionValue);
//            switch (filter.Comparison)
//            {
//                case EFComparison.Equal:
//                    return Expression.Equal(propertyExp, valueExp);//i.Property == value;
//                case EFComparison.NotEqual:
//                    return Expression.NotEqual(propertyExp, valueExp);//i.Property == value;
//                case EFComparison.GreaterThan:
//                    return Expression.GreaterThan(propertyExp, valueExp);//i.Property == value;
//                case EFComparison.GreaterThanOrEqual:
//                    return Expression.GreaterThanOrEqual(propertyExp, valueExp);//i.Property == value;
//                case EFComparison.LessThan:
//                    return Expression.LessThan(propertyExp, valueExp);//i.Property == value;
//                case EFComparison.LessThanOrEqual:
//                    return Linq.Expressions.Expression.LessThanOrEqual(propertyExp, valueExp);//i.Property == value;
//                case EFComparison.Contains:
//                    return Linq.Expressions.Expression.Call(propertyExp, nameof(string.Contains), null, valueExp);//i.Property == value;
//                case EFComparison.StartWith:
//                    return Linq.Expressions.Expression.Call(propertyExp, nameof(string.StartsWith), null, valueExp);//i.Property == value;
//                case EFComparison.EndWith:
//                    return Linq.Expressions.Expression.Call(propertyExp, nameof(string.EndsWith), null, valueExp);//i.Property == value;
//                case EFComparison.Match:
//                    return Linq.Expressions.Expression.Call(typeof(Regex), nameof(Regex.Match), null, propertyExp, valueExp);//i.Property == value;
//                default:
//                    break;
//            }

//            return Expression.Equal(propertyExp, valueExp);
//        }
//    }

  

   
  
//}
