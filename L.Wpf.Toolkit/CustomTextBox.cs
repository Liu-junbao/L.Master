using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
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
            e.Handled = CheckInputText(caretIndex, inputText, newText) == false;
            base.OnPreviewTextInput(e);
        }
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            CheckInputText(CaretIndex,null,Text);
            base.OnLostKeyboardFocus(e);
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
    public abstract class NumericalTextBox<TNumeric> : CustomTextBox
        where TNumeric : struct, IFormattable, IComparable<TNumeric>
    {
        public static readonly DependencyProperty MaxValueProperty =
          DependencyProperty.Register(nameof(MaxValue), typeof(TNumeric?), typeof(NumericalTextBox<TNumeric>), new PropertyMetadata(null));
        public static readonly DependencyProperty MinValueProperty =
          DependencyProperty.Register(nameof(MinValue), typeof(TNumeric?), typeof(NumericalTextBox<TNumeric>), new PropertyMetadata(null));
        public static readonly DependencyProperty StringFormatProperty =
           DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(NumericalTextBox<TNumeric>), new PropertyMetadata(null));
        public static readonly DependencyProperty ValueProperty =
           DependencyProperty.Register(nameof(Value), typeof(TNumeric), typeof(NumericalTextBox<TNumeric>), new PropertyMetadata(default(TNumeric)));
        public static readonly DependencyProperty CultureInfoProperty =
           DependencyProperty.Register(nameof(CultureInfo), typeof(CultureInfo), typeof(NumericalTextBox<TNumeric>), new PropertyMetadata(new CultureInfo("zh-CN")));

        public TNumeric? MaxValue
        {
            get { return (TNumeric)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        public TNumeric? MinValue
        {
            get { return (TNumeric)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public TNumeric Value
        {
            get { return (TNumeric)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public CultureInfo CultureInfo
        {
            get { return (CultureInfo)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }
        protected override bool CheckInputText(int caretIndex, string inputText, string newText)
        {
            TNumeric value;
            string valueText;
            int valueCaretIndex;
            if (TryParseFromFormat(newText,caretIndex,out value, out valueText,out valueCaretIndex))
            {
                //上限控制
                var max = MaxValue;
                if (max != null && value.CompareTo(max.Value) > 0)
                {
                    Text = ConvertToString(max.Value, StringFormat);
                    CaretIndex = Text?.Length ?? 0;
                    return false;
                }
                //下限控制
                var min = MinValue;
                if (min != null && value.CompareTo(min.Value) < 0)
                {
                    Text = ConvertToString(min.Value, StringFormat);
                    CaretIndex = Text?.Length ?? 0;
                    return false;
                }
                //修改了输入值
                if (valueText != newText)
                {
                    Text = valueText;
                }
                return true;
            }
            return true;
        }
        protected abstract bool TryParseFromFormat(string newText, int caretIndex, out TNumeric value, out string valueText, out int valueCaretIndex);
        protected virtual string ConvertToString(TNumeric value, string format)
        {
            if (string.IsNullOrEmpty(format) == false)
                return value.ToString(format, CultureInfo);
            return value.ToString();
        }
    }
    public class IntegerTextBox : NumericalTextBox<int>
    {
        protected override bool TryParseFromFormat(string newText, int caretIndex, out int value, out string valueText, out int valueCaretIndex)
        {
            value = 0;
            valueText = newText;
            valueCaretIndex = caretIndex;
            return false;
        }
    }
    public class DoubleTextBox : NumericalTextBox<double>
    {
        protected override bool TryParseFromFormat(string newText, int caretIndex, out double value, out string valueText, out int valueCaretIndex)
        {
            value = 0;
            valueText = newText;
            valueCaretIndex = caretIndex;
            return false;
        }
    }
    public class DateTimeTextBox : NumericalTextBox<DateTime>
    {
        protected override bool TryParseFromFormat(string newText, int caretIndex, out DateTime value, out string valueText, out int valueCaretIndex)
        {
            value = DateTime.Now;
            valueText = newText;
            valueCaretIndex = caretIndex;
            return false;
        }
    }
}
