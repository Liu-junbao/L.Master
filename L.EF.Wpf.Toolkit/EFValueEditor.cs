using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace System.Windows
{
    [TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
    public class EFValueEditor : EFEditorBase
    {
        public const string PART_TextBox = nameof(PART_TextBox);

        public static readonly RoutedEvent ValueChangedEvent =
                  EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EFValueEditor));

        private static readonly DependencyPropertyKey PropertyNamePropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(PropertyName), typeof(string), typeof(EFValueEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty PropertyNameProperty = PropertyNamePropertyKey.DependencyProperty;
        public static readonly DependencyProperty PropertyValueProperty =
           DependencyProperty.Register(nameof(PropertyValue), typeof(object), typeof(EFValueEditor), new PropertyMetadata(null, OnPropertyValueChanged));
        private static readonly DependencyPropertyKey PropertyTypePropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(PropertyType), typeof(Type), typeof(EFValueEditor), new PropertyMetadata(default));
        public static readonly DependencyProperty PropertyTypeProperty = PropertyTypePropertyKey.DependencyProperty;
        public static readonly DependencyProperty ValueProperty =
           DependencyProperty.Register(nameof(Value), typeof(object), typeof(EFValueEditor), new PropertyMetadata(null, OnValueChanged));
        private static readonly DependencyPropertyKey IsValueChangedPropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(IsValueChanged), typeof(bool), typeof(EFValueEditor), new PropertyMetadata(false));
        public static readonly DependencyProperty IsValueChangedProperty = IsValueChangedPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsValidValuePropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(IsValidValue), typeof(bool), typeof(EFValueEditor), new PropertyMetadata(false));
        public static readonly DependencyProperty IsValidValueProperty = IsValidValuePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey ValidValuePropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(ValidValue), typeof(object), typeof(EFValueEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty ValidValueProperty = ValidValuePropertyKey.DependencyProperty;
        private static void OnPropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EFValueEditor)d;
            editor.OnPropertyValueChanged(e.OldValue, e.NewValue);
        }
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EFValueEditor)d;
            editor.OnValueChanged(e.OldValue, e.NewValue);
        }
        static EFValueEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EFValueEditor), new FrameworkPropertyMetadata(typeof(EFValueEditor)));
        }
        private BindingExpressionBase _propertyValueBinding;
        private TextBox _valueTextBox;
        public EFValueEditor() : this(null, default) { }
        public EFValueEditor(string propertyName, Type propertyType)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            _propertyValueBinding = this.SetBinding(PropertyValueProperty, new Binding($"{nameof(this.DataContext)}.{propertyName}") { Source = this, Mode = BindingMode.OneWay });
        }
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            protected set { SetValue(PropertyNamePropertyKey, value); }
        }
        public object PropertyValue
        {
            get { return (object)GetValue(PropertyValueProperty); }
            set { SetValue(PropertyValueProperty, value); }
        }
        public Type PropertyType
        {
            get { return (Type)GetValue(PropertyTypeProperty); }
            protected set { SetValue(PropertyTypePropertyKey, value); }
        }
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public bool IsValueChanged
        {
            get { return (bool)GetValue(IsValueChangedProperty); }
            protected set { SetValue(IsValueChangedPropertyKey, value); }
        }
        public bool IsValidValue
        {
            get { return (bool)GetValue(IsValidValueProperty); }
            protected set { SetValue(IsValidValuePropertyKey, value); }
        }
        public object ValidValue
        {
            get { return (object)GetValue(ValidValueProperty); }
            protected set { SetValue(ValidValuePropertyKey, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_valueTextBox != null)
                _valueTextBox.TextChanged -= ValueTextBox_TextChanged;
            _valueTextBox = this.Template.FindName(PART_TextBox, this) as TextBox;
            if (_valueTextBox != null)
                _valueTextBox.TextChanged += ValueTextBox_TextChanged;
        }
        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsEditing == false) return;
            var newValue = _valueTextBox?.Text;
            if (newValue?.Equals(PropertyValue) == false)
            {
                object validValue;
                var isValidValue = TryParse(newValue, out validValue);
                IsValidValue = isValidValue;
                ValidValue = validValue;
                IsValueChanged = isValidValue && validValue?.Equals(PropertyValue) != true;
            }
            else
            {
                IsValidValue = true;
                ValidValue = newValue;
                IsValueChanged = false;
            }
            RaiseValueChanged();
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsRowEditableProperty || e.Property == IsAddedItemProperty || e.Property == IsEditingProperty)
            {
                _propertyValueBinding.UpdateTarget();
                var value = PropertyValue;
                Value = value;
            }
        }
        private void OnPropertyValueChanged(object oldValue, object newValue)
        {
            Value = newValue;
            if (newValue != null)
                PropertyType = newValue.GetType();
        }
        protected void OnValueChanged(object oldValue, object newValue)
        {
            if (newValue?.Equals(PropertyValue) == false)
            {
                object validValue;
                var isValidValue = TryParse(newValue, out validValue);
                IsValidValue = isValidValue;
                ValidValue = validValue;
                IsValueChanged = isValidValue && validValue?.Equals(PropertyValue) != true;
            }
            else
            {
                IsValidValue = true;
                ValidValue = newValue;
                IsValueChanged = false;
            }
            RaiseValueChanged();
        }
        protected void OnValueTextChanged(string oldValue, string newValue)
        {
            if (newValue?.Equals(PropertyValue) == false)
            {
                object validValue;
                var isValidValue = TryParse(newValue, out validValue);
                IsValidValue = isValidValue;
                ValidValue = validValue;
                IsValueChanged = isValidValue && validValue?.Equals(PropertyValue) != true;
            }
            else
            {
                IsValidValue = true;
                ValidValue = newValue;
                IsValueChanged = false;
            }
            RaiseValueChanged();
        }
        private bool TryParse(object value, out object validValue)
        {
            validValue = null;
            try
            {
                validValue = Convert.ChangeType(value, PropertyType);
                return true;
            }
            catch { }
            return false;
        }

        public event RoutedEventHandler ValueChanged
        {
            add { this.AddHandler(ValueChangedEvent, value); }
            remove { this.RemoveHandler(ValueChangedEvent, value); }
        }
        private void RaiseValueChanged()
        {
            var e = new RoutedEventArgs(ValueChangedEvent);
            OnValueChanged(e);
        }
        protected virtual void OnValueChanged(RoutedEventArgs e)
        {
            this.RaiseEvent(e);
        }

        public override string ToString() => $"{PropertyName}:{PropertyValue}";
    }
}
