using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace System.Windows
{
    [ContentProperty(nameof(Converts))]
    public class SwitchConverter : IValueConverter
    {
        public SwitchConverter()
        {
            Converts = new List<CaseConvert>();
            ConvertBacks = new List<CaseConvert>();
        }
        public object ConvertDefault { get; set; }
        public object ConvertBackDefault { get; set; }
        public List<CaseConvert> Converts { get; set; }
        public List<CaseConvert> ConvertBacks { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Converts != null)
            {
                foreach (var item in Converts)
                {
                    object returnValue;
                    if (item.CanConvert(value, out returnValue))
                    {
                        return returnValue;
                    }
                }
            }
            return ConvertDefault;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ConvertBacks != null)
            {
                foreach (var item in ConvertBacks)
                {
                    object returnValue;
                    if (item.CanConvert(value, out returnValue))
                    {
                        return returnValue;
                    }
                }
            }
            return ConvertBackDefault;
        }
    }
    public abstract class CaseConvert
    {
        public abstract bool CanConvert(object value, out object returnValue);
    }
    public abstract class CaseConvert<TValue> : CaseConvert
    {
        public TValue Value { get; set; }
        public object BackValue { get; set; }
        public override bool CanConvert(object value, out object returnValue)
        {
            if (value is TValue)
            {
                if (Equals(Value, (TValue)value))
                {
                    returnValue = BackValue;
                    return true;
                }
            }
            returnValue = null;
            return false;
        }
        protected virtual bool Enquals(TValue value)
        {
            return EqualityComparer<TValue>.Default.Equals(Value, value);
        }
    }
    public class StringConvert : CaseConvert<string> { }
    public class BoolConvert : CaseConvert<bool> { }
    public class IntConvert : CaseConvert<int> { }
    public class DoubleConvert : CaseConvert<double> { }
}
