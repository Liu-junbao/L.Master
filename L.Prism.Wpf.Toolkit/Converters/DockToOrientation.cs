using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Prism.Converters
{
    public class DockToOrientation : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            switch ((Dock)value)
            {
                case Dock.Left:
                    return Orientation.Vertical;
                case Dock.Top:
                    return Orientation.Horizontal;
                case Dock.Right:
                    return Orientation.Vertical;
                case Dock.Bottom:
                    return Orientation.Horizontal;
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

    public class DockToOrientationExtension : NewMarkupExtension<DockToOrientation> { }
}
