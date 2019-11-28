using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace System.Windows
{
    public class RateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return value;
            double dValue = (double)value;
            double rate = 1;
            if (parameter != null) rate = System.Convert.ToDouble(parameter.ToString());
            return System.Convert.ChangeType(dValue * rate, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return value;
            double dValue = (double)value;
            double rate = 1;
            if (parameter != null) rate = System.Convert.ToDouble(parameter.ToString());
            return System.Convert.ChangeType(dValue / rate, targetType);
        }
    }
    public class RateExtension : NewMarkupExtension<RateConverter> { }
}
