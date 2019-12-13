using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace System.Windows
{
    public class DataGridRowIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return null;
            var dg = values[0] as DataGrid;
            if (dg == null) return null;
            var item = values[1];
            return dg.Items.IndexOf(item)+1;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DataGridRowIndexExtension : NewMarkupExtension<DataGridRowIndexConverter> { }
}
