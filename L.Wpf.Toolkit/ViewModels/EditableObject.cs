using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace System.Windows
{
    public class EditableObject : DependencyObject
    {
        public static readonly DependencyProperty IsEnabledProperty =
          DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(EditableObject), new PropertyMetadata(true));
        public static readonly DependencyProperty IsEditingProperty =
          DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EditableObject), new PropertyMetadata(false, OnValueChanged));
        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(EditableObject), new PropertyMetadata(false, OnValueChanged));
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (EditableObject)d;
            if (e.Property == IsEditingProperty)
                obj.OnIsEditingChanged((bool)e.NewValue);
            else if (e.Property == IsSelectedProperty)
                obj.OnIsSelectedChanged((bool)e.NewValue);
        }
        public EditableObject(EditableViewModel editableViewModel)
        {
            BindingOperations.SetBinding(this, IsEnabledProperty, new Binding(nameof(editableViewModel.IsEnabled)) { Source = editableViewModel, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(this, IsEditingProperty, new Binding(nameof(editableViewModel.IsEditing)) { Source = editableViewModel, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(this, IsSelectedProperty, new Binding(nameof(editableViewModel.IsSelected)) { Source = editableViewModel });
        }
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        protected virtual void OnIsEditingChanged(bool isEditing) { }
        protected virtual void OnIsSelectedChanged(bool isSelected) { }
    }
    public class EditableValueEditor : EditableObject
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(EditableValueEditor), new PropertyMetadata(null, OnValueChanged));
        public static readonly DependencyProperty EditedValueProperty =
            DependencyProperty.Register(nameof(EditedValue), typeof(object), typeof(EditableValueEditor), new PropertyMetadata(null, OnEditedValueChanged));
        private static readonly DependencyPropertyKey IsChangedPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsChanged), typeof(bool), typeof(EditableValueEditor), new PropertyMetadata(false));
        public static readonly DependencyProperty IsChangedProperty = IsChangedPropertyKey.DependencyProperty;

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EditableValueEditor)d;
            editor.EditedValue = e.NewValue;
        }
        private static void OnEditedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EditableValueEditor)d;
            editor.IsChanged = editor.Value != e.NewValue;
        }
        public EditableValueEditor(EditableViewModel editableViewModel, string propertyName) : base(editableViewModel)
        {
            PropertyName = propertyName;
            BindingOperations.SetBinding(this, ValueProperty, new Binding(propertyName) { Source = editableViewModel.Source });
            editableViewModel.OnEditedEvent += EditableViewModel_OnEditedEvent;
        }
        public string PropertyName { get; }
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public object EditedValue
        {
            get { return (object)GetValue(EditedValueProperty); }
            set { SetValue(EditedValueProperty, value); }
        }
        public bool IsChanged
        {
            get { return (bool)GetValue(IsChangedProperty); }
            protected set { SetValue(IsChangedPropertyKey, value); }
        }
        protected override void OnIsEditingChanged(bool isEditing)
        {
            base.OnIsEditingChanged(isEditing);
            if (isEditing == false)
                EditedValue = Value;
        }
        private void EditableViewModel_OnEditedEvent(object sender, EditableViewModelEditedEventArgs e)
        {
            if (IsChanged)
            {
                e.EditedValues.Add(new Tuple<string, object>(PropertyName, EditedValue));
            }
        }
    }
    public class EditableValueOperation : EditableObject
    {
        private static readonly DependencyPropertyKey EditCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(EditCommand), typeof(ICommand), typeof(EditableValueOperation), new PropertyMetadata(null));
        public static readonly DependencyProperty EditCommandProperty = EditCommandPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CancelCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CancelCommand), typeof(ICommand), typeof(EditableValueOperation), new PropertyMetadata(null));
        public static readonly DependencyProperty CancelCommandProperty = CancelCommandPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey SaveCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(SaveCommand), typeof(ICommand), typeof(EditableValueOperation), new PropertyMetadata(null));
        public static readonly DependencyProperty SaveCommandProperty = SaveCommandPropertyKey.DependencyProperty;

        public EditableValueOperation(EditableViewModel editableViewModel) : base(editableViewModel)
        {
            EditCommand = new Command(OnEdit);
            CancelCommand = new Command(OnCancel);
            SaveCommand = new AsyncCommand(editableViewModel.RaiseSaveAsync);
        }
        public ICommand EditCommand
        {
            get { return (ICommand)GetValue(EditCommandProperty); }
            protected set { SetValue(EditCommandPropertyKey, value); }
        }
        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            protected set { SetValue(CancelCommandPropertyKey, value); }
        }
        public ICommand SaveCommand
        {
            get { return (ICommand)GetValue(SaveCommandProperty); }
            protected set { SetValue(SaveCommandPropertyKey, value); }
        }
        private void OnEdit()
        {
            this.IsEditing = true;
        }
        private void OnCancel()
        {
            this.IsEditing = false;
        }
    }
    public class EditableValueEditorConverter : IValueConverter
    {
        public EditableValueEditorConverter() { }
        public EditableValueEditorConverter(string bindingPropertyName)
        {
            BindingPropertyName = bindingPropertyName;
        }
        public string BindingPropertyName { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return new EditableValueEditor((EditableViewModel)value, BindingPropertyName);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EditableValueEditorExtension : NewMarkupExtension<EditableValueEditorConverter>
    {
        public EditableValueEditorExtension(string bindingPropertyName)
        {
            BindingPropertyName = bindingPropertyName;
        }
        public string BindingPropertyName { get; set; }
        protected override object[] CreateArgs()
        {
            return new object[] { BindingPropertyName };
        }
    }
    public class EditableValueOperationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return new EditableValueOperation((EditableViewModel)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EditableValueOperationExtension : NewMarkupExtension<EditableValueEditorConverter> { }
}
