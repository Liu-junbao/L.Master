using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace System.Windows
{
    public class EFValueEditor : EFEditorBase
    {
        private static readonly DependencyPropertyKey PropertyNamePropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(PropertyName), typeof(string), typeof(EFValueEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty PropertyNameProperty = PropertyNamePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey PropertyTypePropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(PropertyType), typeof(Type), typeof(EFValueEditor), new PropertyMetadata(default));
        public static readonly DependencyProperty PropertyTypeProperty = PropertyTypePropertyKey.DependencyProperty;
        public static readonly DependencyProperty PropertyValueProperty =
           DependencyProperty.Register(nameof(PropertyValue), typeof(object), typeof(EFDataGrid), new PropertyMetadata(null, OnValueChanged));
        public static readonly DependencyProperty ValueProperty =
           DependencyProperty.Register(nameof(Value), typeof(object), typeof(EFDataGrid), new PropertyMetadata(null, OnEditedValueChanged));
        private static readonly DependencyPropertyKey IsValueChangedPropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(IsValueChanged), typeof(bool), typeof(EFValueEditor), new PropertyMetadata(false));
        public static readonly DependencyProperty IsValueChangedProperty = IsValueChangedPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsValidValuePropertyKey =
         DependencyProperty.RegisterReadOnly(nameof(IsValidValue), typeof(bool), typeof(EFValueEditor), new PropertyMetadata(false));
        public static readonly DependencyProperty IsValidValueProperty = IsValidValuePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey ValidValuePropertyKey =
          DependencyProperty.RegisterReadOnly(nameof(ValidValue), typeof(object), typeof(EFDataGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty ValidValueProperty = ValidValuePropertyKey.DependencyProperty;
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EFValueEditor)d;
            editor.OnPropertyValueChanged(e.OldValue, e.NewValue);
        }
        private static void OnEditedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EFValueEditor)d;
            editor.OnValueChanged(e.OldValue, e.NewValue);
        }
        static EFValueEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EFValueEditor), new FrameworkPropertyMetadata(typeof(EFValueEditor)));
        }
        private BindingExpressionBase _propertyValueBinding;
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
        public Type PropertyType
        {
            get { return (Type)GetValue(PropertyTypeProperty); }
            protected set { SetValue(PropertyTypePropertyKey, value); }
        }
        public object PropertyValue
        {
            get { return (object)GetValue(PropertyValueProperty); }
            set { SetValue(PropertyValueProperty, value); }
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
        protected override void OnIsEditingChanged(bool isEditing)
        {
            _propertyValueBinding.UpdateTarget();
            Value = PropertyValue;
        }
        private void OnPropertyValueChanged(object oldValue, object newValue)
        {
            Value = newValue;
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
    }
}
