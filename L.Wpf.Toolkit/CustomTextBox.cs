using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace System.Windows
{
    public abstract class CustomTextBox : TextBox
    {
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            var newText = Text;
            var caretIndex = CaretIndex;
            var inputText = e.Text;
            var comp = e.TextComposition as FrameworkTextComposition;
            if (comp != null)
                caretIndex = comp.ResultOffset;
            else
                newText = newText.Insert(caretIndex,inputText);
            e.Handled = CheckInputText(caretIndex, inputText, newText) == false;
            base.OnPreviewTextInput(e);
        }
        protected abstract bool CheckInputText(int caretIndex, string inputText, string newText);
    }
    public class RegexTextBox : CustomTextBox
    {
        public static readonly DependencyProperty PatternProperty =
           DependencyProperty.Register(nameof(Pattern), typeof(string), typeof(RegexTextBox), new PropertyMetadata(null));
        public string Pattern
        {
            get { return (string)GetValue(PatternProperty); }
            set { SetValue(PatternProperty, value); }
        }
        protected override bool CheckInputText(int caretIndex, string inputText, string newText)
        {
            var pattern = Pattern;
            if (string.IsNullOrEmpty(pattern))
                return true;
            return Regex.IsMatch(newText, pattern);
        }
    }
    public abstract class LimitTextBox<TComparable> : CustomTextBox
        where TComparable : struct, IFormattable, IComparable<TComparable>
    {
        public static readonly DependencyProperty MaxValueProperty =
          DependencyProperty.Register(nameof(MaxValue), typeof(TComparable?), typeof(LimitTextBox<TComparable>), new PropertyMetadata(null));
        public static readonly DependencyProperty MinValueProperty =
          DependencyProperty.Register(nameof(MinValue), typeof(TComparable?), typeof(LimitTextBox<TComparable>), new PropertyMetadata(null));
        public static readonly DependencyProperty StringFormatProperty =
           DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(LimitTextBox<TComparable>), new PropertyMetadata(null));
        public static readonly DependencyProperty ValueProperty =
           DependencyProperty.Register(nameof(Value), typeof(TComparable), typeof(LimitTextBox<TComparable>), new PropertyMetadata(default(TComparable)));
        public static readonly DependencyProperty CultureInfoProperty =
           DependencyProperty.Register(nameof(CultureInfo), typeof(CultureInfo), typeof(LimitTextBox<TComparable>), new PropertyMetadata(new CultureInfo("zh-CN")));
        public LimitTextBox()
        {
            Text = ConvertToString(Value);
        }
        public TComparable? MaxValue
        {
            get { return (TComparable?)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        public TComparable? MinValue
        {
            get { return (TComparable?)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public TComparable Value
        {
            get { return (TComparable)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public CultureInfo CultureInfo
        {
            get { return (CultureInfo)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ValueProperty)
            {
                Text = ConvertToString(Value);
            }
        }
        protected override bool CheckInputText(int caretIndex, string inputText, string newText)
        {
            TComparable value = ParseFrom(newText);
            //上限控制
            var max = MaxValue;
            if (max != null && value.CompareTo(max.Value) > 0)
            {
                value = max.Value;
                SelectionLength = 0;
            }
            //下限控制
            var min = MinValue;
            if (min != null && value.CompareTo(min.Value) < 0)
            {
                value = min.Value;
                SelectionLength = 0;
            }
            //值
            Value = value;
            //字符串
            var valueText = ConvertToString(value);
            if (valueText != newText)
            {
                Text = valueText;
                //this.Dispatcher.BeginInvoke(new Action(()=>));
                return false;
            }
            return true;
        }

        protected abstract TComparable ParseFrom(string newText);
        protected virtual string ConvertToString(TComparable value)
        {
            var format = StringFormat;
            if (string.IsNullOrEmpty(format) == false)
                return value.ToString(format, CultureInfo);
            return value.ToString();
        }
    }
    public abstract class NumericalTextBox<TNumerical> : LimitTextBox<TNumerical>
         where TNumerical : struct, IFormattable, IComparable<TNumerical>
    {
        public const string Minus = "-";
        public const string Dot = ".";
    }
    public class IntegerTextBox : NumericalTextBox<int>
    {
      
        protected override int ParseFrom(string newText)
        {
            if (newText == Minus)
            {
                var min = MinValue;
                if (min != null)
                    return min.Value;
                else
                    return -1;
             }
            int value;
            if (int.TryParse(newText, out value))
                return value;
            return 0;
        }
    }
    public class DoubleTextBox : NumericalTextBox<double>
    {
        protected override double ParseFrom(string newText)
        {
            double value;
            if (double.TryParse(newText, out value))
                return value;
            return 0;
        }
    }
    public class DateTimeTextBox : LimitTextBox<DateTime>
    {
        protected override DateTime ParseFrom(string newText)
        {
            DateTime time;
            if (DateTime.TryParse(newText, out time))
                return time;
            return DateTime.Now.Date;
        }
    }
}
