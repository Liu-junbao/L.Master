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
        public static readonly DependencyProperty IsEditingProperty =
           DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false, OnIsEditingChanged));
        public static readonly DependencyProperty IsReadOnlyProperty =
           DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false));
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
            this.SetBinding(IsEditingProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.IsRowEditingProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
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
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        protected virtual void OnIsEditingChanged(bool isEditing) { }
    }
}
