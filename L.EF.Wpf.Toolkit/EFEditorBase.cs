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
        public static readonly DependencyProperty IsEditingProperty =
           DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EFEditorBase), new PropertyMetadata(false, OnIsEditingChanged));
        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EFEditorBase editor = (EFEditorBase)d;
            editor.OnIsEditingChanged((bool)e.NewValue);
        }
        public EFEditorBase()
        {
            this.SetBinding(IsRowMouseOverProperty, new Binding($"({nameof(EFDataGridAssist)}.{EFDataGridAssist.IsRowMouseOverProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(IsRowSelectedProperty, new Binding($"({nameof(EFDataGridAssist)}.{EFDataGridAssist.IsRowSelectedProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(IsEditingProperty, new Binding($"({nameof(EFDataGridAssist)}.{EFDataGridAssist.IsRowEditingProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
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
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }
        protected virtual void OnIsEditingChanged(bool isEditing) { }
    }
}
