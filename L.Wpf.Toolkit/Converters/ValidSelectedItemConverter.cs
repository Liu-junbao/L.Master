using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace System.Windows
{
    public class ValidSelectedItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return null;
            var source = (IEnumerable)values[0];
            var selectedItem = values[1];
            var validSelectedItem =GetVaidSelectedItem(source,selectedItem);
            return validSelectedItem;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (value!=null)
                return new object[] { null, value };
            return null;
        }
        private object GetVaidSelectedItem(IEnumerable source,object selectedItem)
        {
            if (source == null) return null;
            if (selectedItem == null) return null;
            foreach (var item in source)
            {
                if (item.Equals(selectedItem))
                    return selectedItem;
            }
            return null;
        }
    }
    public class ValidSelectedItemExtension : NewMarkupExtension<ValidSelectedItemConverter> { }
}
