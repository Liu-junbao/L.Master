using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace System.Windows
{
    public class ComparisonConverter : IValueConverter
    {
        public bool IsSign { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(EFComparison)) return null;
            switch ((EFComparison)value)
            {
                case EFComparison.Equal:
                    return IsSign ? "==" : "等于";
                case EFComparison.NotEqual:
                    return IsSign ? "!=" : "不等于";
                case EFComparison.GreaterThan:
                    return IsSign ? ">" : "大于";
                case EFComparison.GreaterThanOrEqual:
                    return IsSign ? ">=" : "大于等于";
                case EFComparison.LessThan:
                    return IsSign ? "<" : "小于";
                case EFComparison.LessThanOrEqual:
                    return IsSign ? "<=" : "小于等于";
                case EFComparison.Contains:
                    return IsSign ? "Contains" : "包含";
                case EFComparison.NotContains:
                    return IsSign ? "NotContains" : "不包含";
                case EFComparison.StartWith:
                    return IsSign ? "StartWith" : "开头为";
                case EFComparison.EndWith:
                    return IsSign ? "EndWith" : "结尾为";
                default:
                    break;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ComparisonExtension:NewMarkupExtension<ComparisonConverter>
    {
        public bool IsSign { get; set; }
        protected override void OnInitialize(ComparisonConverter value)
        {
            value.IsSign = IsSign;
            base.OnInitialize(value);
        }
    }
}
