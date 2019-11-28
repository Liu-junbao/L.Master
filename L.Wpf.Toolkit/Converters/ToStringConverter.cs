using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace System.Windows
{
    public class ToStringConverter : IValueConverter
    {
        private Type _valueType;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            _valueType = value.GetType();
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_valueType == null)
                return null;
            return System.Convert.ChangeType(value.ToString(), _valueType);
        }
    }
    public class ToStringExtension : NewMarkupExtension<ToStringConverter> { }
}
