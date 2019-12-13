using System.Globalization;
using System.Windows.Data;

namespace System.Windows
{
    public class SecondsToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return TimeSpan.FromSeconds((double)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SecondsToTimeSpanExtension : NewMarkupExtension<SecondsToTimeSpanConverter> { }
}
