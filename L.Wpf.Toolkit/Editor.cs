using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace System.Windows
{
    public abstract class Editor : Control
    {
        public static readonly DependencyProperty EditableViewModelProperty =
            DependencyProperty.Register(nameof(EditableViewModel), typeof(EditableViewModel), typeof(Editor), new PropertyMetadata(null, OnEditableViewModelChanged));
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(Editor), new PropertyMetadata(false, OnIsEditingChanged));
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(Editor), new PropertyMetadata(false, OnIsSelectedChanged));
        private static void OnEditableViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (Editor)d;
            editor.OnEditableViewModelChanged((EditableViewModel)e.OldValue, (EditableViewModel)e.NewValue);
        }
        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (Editor)d;
            editor.OnIsEditingChanged((bool)e.NewValue);
        }
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (Editor)d;
            editor.OnIsSelectedChanged((bool)e.NewValue);
        }
        public Editor()
        {
            BindingOperations.SetBinding(this, IsEditingProperty, new Binding($"{nameof(EditableViewModel)}.{nameof(EditableViewModel.IsEditing)}") { Source = this, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(this, IsSelectedProperty, new Binding($"{nameof(EditableViewModel)}.{nameof(EditableViewModel.IsSelected)}") { Source = this, Mode = BindingMode.TwoWay });
        }
        public EditableViewModel EditableViewModel
        {
            get { return (EditableViewModel)GetValue(EditableViewModelProperty); }
            set { SetValue(EditableViewModelProperty, value); }
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
        protected virtual void OnEditableViewModelChanged(EditableViewModel oldViewModel, EditableViewModel newViewModel) { }
    }
    public class ValueEditor : Editor
    {
        public static readonly DependencyProperty EditedSourcePropertyNameProperty =
            DependencyProperty.Register(nameof(EditedSourcePropertyName), typeof(string), typeof(ValueEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty ValueProperty =
           DependencyProperty.Register(nameof(Value), typeof(object), typeof(ValueEditor), new PropertyMetadata(null, OnValueChanged));
        public static readonly DependencyProperty EditedValueProperty =
           DependencyProperty.Register(nameof(EditedValue), typeof(object), typeof(ValueEditor), new PropertyMetadata(null, OnEditedValueChanged));
        public static readonly DependencyProperty IsChangedProperty =
           DependencyProperty.Register(nameof(IsChanged), typeof(bool), typeof(ValueEditor), new PropertyMetadata(null));
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (ValueEditor)d;
            editor.EditedValue = e.NewValue;
        }
        private static void OnEditedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (ValueEditor)d;
            editor.IsChanged = e.NewValue != editor.Value;
        }
        static ValueEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValueEditor),new FrameworkPropertyMetadata(typeof(ValueEditor)));
        }
        public string EditedSourcePropertyName
        {
            get { return (string)GetValue(EditedSourcePropertyNameProperty); }
            set { SetValue(EditedSourcePropertyNameProperty, value); }
        }
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
            set { SetValue(IsChangedProperty, value); }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            BindingOperations.SetBinding(this, ValueProperty, new Binding($"{nameof(EditableViewModel)}.{nameof(EditableViewModel.Source)}.{EditedSourcePropertyName}") { Source = this });
        }
        protected override void OnIsEditingChanged(bool isEditing)
        {
            base.OnIsEditingChanged(isEditing);
            if (isEditing == false)
            {
                EditedValue = Value;
            }
        }
        protected override void OnEditableViewModelChanged(EditableViewModel oldViewModel, EditableViewModel newViewModel)
        {
            base.OnEditableViewModelChanged(oldViewModel, newViewModel);
            if (oldViewModel != null)
            {
                oldViewModel.OnEditedEvent -= Model_OnEditedEvent;
            }
            if (newViewModel != null)
            {
                newViewModel.OnEditedEvent += Model_OnEditedEvent;
            }
        }
        private void Model_OnEditedEvent(object sender, EditableViewModelEditedEventArgs e)
        {
            if (IsChanged)
            {
                e.EditedValues.Add(new Tuple<string, object>(EditedSourcePropertyName, EditedValue));
            }
        }
    }
    [ContentProperty(nameof(Selections))]
    public class SelectableValueEditor:ValueEditor
    {
        static SelectableValueEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectableValueEditor),new FrameworkPropertyMetadata(typeof(SelectableValueEditor)));
        }
        public SelectableValueEditor()
        {
            Selections = new ObservableCollection<object>();
        }
        public ObservableCollection<object> Selections { get; }
    }
    public class OperationEditor : Editor
    {
        static OperationEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OperationEditor), new FrameworkPropertyMetadata(typeof(OperationEditor)));
        }
        public OperationEditor()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open,new ExecutedRoutedEventHandler(OnPen)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new ExecutedRoutedEventHandler(OnClose)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, new ExecutedRoutedEventHandler(OnSave)));
        }
        private void OnPen(object sender, ExecutedRoutedEventArgs e)
        {
            IsEditing = true;
        }
        private void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            IsEditing = false;
        }
        private async void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (EditableViewModel!=null)
                await EditableViewModel.RaiseSaveAsync();
        }
    }



}
