using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Prism
{
    class NULLToVisibilityConverter : IValueConverter
    {
        public NULLToVisibilityConverter()
        {
            OnNotNULL = Visibility.Visible;
            OnNULL = Visibility.Collapsed;
        }
        public Visibility OnNotNULL { get; set; }
        public Visibility OnNULL { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return OnNULL;
            else
                return OnNotNULL;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
