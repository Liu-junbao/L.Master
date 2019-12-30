using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace System.Windows
{
    public abstract class EFEditorBase : Control
    {
        public static readonly DependencyProperty IsRowMouseOverProperty =
           DependencyProperty.Register(nameof(IsRowMouseOver), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false));
        public static readonly DependencyProperty IsRowSelectedProperty =
           DependencyProperty.Register(nameof(IsRowSelected), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false));
        public static readonly DependencyProperty IsRowValueChangedProperty =
           DependencyProperty.Register(nameof(IsRowValueChanged), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false));
        public static readonly DependencyProperty IsRowEditableProperty =
           DependencyProperty.Register(nameof(IsRowEditable), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false));
        public static readonly DependencyProperty IsReadOnlyProperty =
           DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false));
        public static readonly DependencyProperty IsAddedItemProperty =
           DependencyProperty.Register(nameof(IsAddedItem), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false));
        public static readonly DependencyProperty HasAddedItemProperty =
           DependencyProperty.Register(nameof(HasAddedItem), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false));
        public static readonly DependencyProperty IsEditingProperty =
           DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false, OnIsEditingChanged));
        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EFEditorBase editor = (EFEditorBase)d;
            editor.OnIsEditingChanged((bool)e.NewValue);
        }
        public EFEditorBase()
        {
            this.SetBinding(IsRowMouseOverProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.IsRowMouseOverProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(IsRowSelectedProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.IsRowSelectedProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(IsRowValueChangedProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.IsRowValueChangedProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(IsRowEditableProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.IsRowEditableProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(HasAddedItemProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.HasAddedItemProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(IsAddedItemProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.IsAddedItemProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
        }
        public DataGridRow RowOwner => this.FindParent<DataGridRow>();
        public bool IsRowMouseOver
        {
            get { return (bool)GetValue(IsRowMouseOverProperty); }
            set { SetValue(IsRowMouseOverProperty, value); }
        }
        public bool IsRowSelected
        {
            get { return (bool)GetValue(IsRowSelectedProperty); }
            set { SetValue(IsRowSelectedProperty, value); }
        }
        public bool IsRowValueChanged
        {
            get { return (bool)GetValue(IsRowValueChangedProperty); }
            set { SetValue(IsRowValueChangedProperty, value); }
        }
        public bool IsRowEditable
        {
            get { return (bool)GetValue(IsRowEditableProperty); }
            set { SetValue(IsRowEditableProperty, value); }
        }
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public bool IsAddedItem
        {
            get { return (bool)GetValue(IsAddedItemProperty); }
            set { SetValue(IsAddedItemProperty, value); }
        }
        public bool HasAddedItem
        {
            get { return (bool)GetValue(HasAddedItemProperty); }
            set { SetValue(HasAddedItemProperty, value); }
        }
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        protected virtual void OnIsEditingChanged(bool isEditing) { }
    }
}
