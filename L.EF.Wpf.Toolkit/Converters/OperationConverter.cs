using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace System.Windows
{
    public class OperationConverter : IValueConverter
    {
        public bool IsSign { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is EFOperation == false) return null;
            switch ((EFOperation)value)
            {
                case EFOperation.And:
                    return IsSign ? "And" : "而且";
                case EFOperation.Or:
                    return IsSign ? "Or" : "或者";
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

    public class OperationExtension:NewMarkupExtension<OperationConverter>
    {
        public bool IsSign { get; set; }
        protected override void OnInitialize(OperationConverter value)
        {
            value.IsSign = IsSign;
            base.OnInitialize(value);
        }
    }
}
