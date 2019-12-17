using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace System.Windows
{
    public class EFDataFilter : Control
    {
        public static readonly DependencyProperty DisplayPropertyInfosProperty =
            DependencyProperty.Register(nameof(DisplayPropertyInfos), typeof(IEnumerable<DisplayPropertyInfo>), typeof(EFDataFilter), new PropertyMetadata(null, OnPropertyChanged));
        private static readonly DependencyPropertyKey QueryExpressionPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(QueryExpression), typeof(Linq.Expressions.Expression), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty QueryExpressionProperty = QueryExpressionPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey FiltersPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Filters), typeof(IEnumerable<EFValueFilter>), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty FiltersProperty = FiltersPropertyKey.DependencyProperty;

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
        static EFDataFilter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EFDataFilter), new FrameworkPropertyMetadata(typeof(EFDataFilter)));
        }
        private Dictionary<string, List<EFValueFilter>> _filters;
        public EFDataFilter()
        {
            _filters = new Dictionary<string, List<EFValueFilter>>();
        }
        public IEnumerable<DisplayPropertyInfo> DisplayPropertyInfos
        {
            get { return (IEnumerable<DisplayPropertyInfo>)GetValue(DisplayPropertyInfosProperty); }
            set { SetValue(DisplayPropertyInfosProperty, value); }
        }
        public Linq.Expressions.Expression QueryExpression
        {
            get { return (Linq.Expressions.Expression)GetValue(QueryExpressionProperty); }
            protected set { SetValue(QueryExpressionPropertyKey, value); }
        }
        public IEnumerable<EFValueFilter> Filters
        {
            get { return (IEnumerable<EFValueFilter>)GetValue(FiltersProperty); }
            protected set { SetValue(FiltersPropertyKey, value); }
        }
    }
    /// <summary>
    ///  筛选条件
    /// </summary>
    public struct EFValueFilter
    {
        /// <summary>
        /// 筛选条件
        /// </summary>
        /// <param name="propertyInfo">属性信息</param>
        /// <param name="comparison">比较方式</param>
        /// <param name="comparisionValue">比较值</param>
        /// <param name="operation">且/或</param>
        public EFValueFilter(DisplayPropertyInfo propertyInfo, EFComparison comparison, object comparisionValue, EFOperation operation = EFOperation.And)
        {
            PropertyInfo = propertyInfo;
            Operation = operation;
            Comparison = comparison;
            ComparisionValue = comparisionValue;
        }
        public DisplayPropertyInfo PropertyInfo { get; }
        public EFOperation Operation { get; }
        public EFComparison Comparison { get; }
        public object ComparisionValue { get; }
        public Linq.Expressions.Expression LoadExpression(ParameterExpression modelExp, Linq.Expressions.Expression body = null)
        {
            if (body == null)
                return ComparisonExpression(modelExp);
            switch (Operation)
            {
                case EFOperation.And:
                    return Linq.Expressions.Expression.AndAlso(body, this.ComparisonExpression(modelExp));
                case EFOperation.Or:
                    return Linq.Expressions.Expression.OrElse(body, this.ComparisonExpression(modelExp));
                default:
                    break;
            }
            return Linq.Expressions.Expression.Add(body, this.ComparisonExpression(modelExp));
        }
        private Linq.Expressions.Expression ComparisonExpression(ParameterExpression modelExp)
        {
            var propertyExp = Linq.Expressions.Expression.Property(modelExp,PropertyInfo.PropertyName);
            var valueExp = Linq.Expressions.Expression.Constant(ComparisionValue);
            switch (Comparison)
            {
                case EFComparison.Equal:
                    return Linq.Expressions.Expression.Equal(propertyExp, valueExp);//i.Property == value;
                case EFComparison.NotEqual:
                    return Linq.Expressions.Expression.NotEqual(propertyExp, valueExp);//i.Property == value;
                case EFComparison.GreaterThan:
                    return Linq.Expressions.Expression.GreaterThan(propertyExp, valueExp);//i.Property == value;
                case EFComparison.GreaterThanOrEqual:
                    return Linq.Expressions.Expression.GreaterThanOrEqual(propertyExp, valueExp);//i.Property == value;
                case EFComparison.LessThan:
                    return Linq.Expressions.Expression.LessThan(propertyExp, valueExp);//i.Property == value;
                case EFComparison.LessThanOrEqual:
                    return Linq.Expressions.Expression.LessThanOrEqual(propertyExp, valueExp);//i.Property == value;
                case EFComparison.Contains:
                    return Linq.Expressions.Expression.Call(propertyExp, nameof(string.Contains), null, valueExp);//i.Property == value;
                case EFComparison.StartWith:
                    return Linq.Expressions.Expression.Call(propertyExp, nameof(string.StartsWith), null, valueExp);//i.Property == value;
                case EFComparison.EndWith:
                    return Linq.Expressions.Expression.Call(propertyExp, nameof(string.EndsWith), null, valueExp);//i.Property == value;
                case EFComparison.Match:
                    return Linq.Expressions.Expression.Call(typeof(Regex), nameof(Regex.Match), null, propertyExp, valueExp);//i.Property == value;
                default:
                    break;
            }

            return Linq.Expressions.Expression.Equal(propertyExp, valueExp);
        }
        private string ComparisionText()
        {
            switch (Comparison)
            {
                case EFComparison.Equal:
                    return "==";
                case EFComparison.NotEqual:
                    return "!=";
                case EFComparison.GreaterThan:
                    return ">";
                case EFComparison.GreaterThanOrEqual:
                    return ">=";
                case EFComparison.LessThan:
                    return "<";
                case EFComparison.LessThanOrEqual:
                    return "<=";
                case EFComparison.Contains:
                    return "Contains";
                case EFComparison.StartWith:
                    return "StartWith";
                case EFComparison.EndWith:
                    return "EndWith";
                case EFComparison.Match:
                    return "Match";
                default:
                    break;
            }
            return null;
        }
    }
    /// <summary>
    /// 比较
    /// </summary>
    public enum EFComparison
    {
        /// <summary>
        /// 等于 
        /// ==
        /// </summary>
        Equal,
        /// <summary>
        /// 不等于 
        /// !=
        /// </summary>
        NotEqual,
        /// <summary>
        /// 大于
        /// >
        /// </summary>
        GreaterThan,
        /// <summary>
        /// 大于或等于
        /// >=
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// 小于
        /// </summary>
        LessThan,
        /// <summary>
        /// 小于或等于
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        /// 包含
        /// </summary>
        Contains,
        /// <summary>
        /// 开头
        /// </summary>
        StartWith,
        /// <summary>
        /// 尾部
        /// </summary>
        EndWith,
        /// <summary>
        /// 匹配
        /// </summary>
        Match,
    }
    /// <summary>
    /// 操作
    /// </summary>
    public enum EFOperation
    {
        /// <summary>
        /// 且
        /// </summary>
        And,
        /// <summary>
        /// 或
        /// </summary>
        Or,
    }
}
