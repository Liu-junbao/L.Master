using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace System.Windows
{
    class EFRowIndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return null;
            var item = values[0];
            var gd = values[1] as EFDataGrid;
            if (gd == null) return null;
            if (EFDataBoxAssist.GetHasAddedItem(gd))
            {
                if (item == EFDataBoxAssist.GetAddedItem(gd)) return "+";
                else return gd.Items.IndexOf(item);
            }
            return gd.Items.IndexOf(item) + 1;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class EFRowIndexExtension : NewMarkupExtension<EFRowIndexConverter> { }

}
