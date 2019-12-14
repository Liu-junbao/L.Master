﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace System.Windows
{

    [ContentProperty(nameof(Columns))]
    [StyleTypedProperty(Property = nameof(ValueEditorStyle), StyleTargetType = typeof(EFValueEditor))]
    public class EFDataGrid : DataGrid
    {
        public static readonly DependencyProperty EditorStyleProperty =
           DependencyProperty.Register(nameof(ValueEditorStyle), typeof(Style), typeof(EFDataGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty OperatorStyleProperty =
           DependencyProperty.Register(nameof(OperatorStyle), typeof(Style), typeof(EFDataGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty IsOperableProperty =
           DependencyProperty.Register(nameof(IsOperable), typeof(bool), typeof(EFDataGrid), new PropertyMetadata(true, OnPropertyChanged));
        public static readonly DependencyProperty DisplayPropertyNamesProperty =
           DependencyProperty.Register(nameof(DisplayPropertyNames), typeof(Dictionary<string, string>), typeof(EFDataGrid), new PropertyMetadata(null,OnPropertyChanged));
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EFDataGrid dataGrid = (EFDataGrid)d;
            dataGrid.Invalidate();
        }
        static EFDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EFDataGrid), new FrameworkPropertyMetadata(typeof(EFDataGrid)));
        }
        public EFDataGrid()
        {
            this.IsReadOnly = true;
            this.AutoGenerateColumns = false;
            this.SetBinding(DisplayPropertyNamesProperty, new Binding($"({nameof(EFDataGridAssist)}.{EFDataGridAssist.DisplayPropertyNamesProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(ItemsSourceProperty, new Binding($"({nameof(EFDataGridAssist)}.{EFDataGridAssist.ItemsSourceProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
        }
        public Style ValueEditorStyle
        {
            get { return (Style)GetValue(EditorStyleProperty); }
            set { SetValue(EditorStyleProperty, value); }
        }
        public Style OperatorStyle
        {
            get { return (Style)GetValue(OperatorStyleProperty); }
            set { SetValue(OperatorStyleProperty, value); }
        }
        public bool IsOperable
        {
            get { return (bool)GetValue(IsOperableProperty); }
            set { SetValue(IsOperableProperty, value); }
        }
        public Dictionary<string,string> DisplayPropertyNames
        {
            get { return (Dictionary<string,string>)GetValue(DisplayPropertyNamesProperty); }
            set { SetValue(DisplayPropertyNamesProperty, value); }
        }
        private void Invalidate()
        {
            this.Columns.Clear();
            if (DisplayPropertyNames != null && DisplayPropertyNames.Count > 0)
            {
                foreach (var item in DisplayPropertyNames)
                {
                    string propertyName = item.Key;
                    string name = item.Value;
                    var column = new GenerateValueDataGridColumn(propertyName) { Header = name };
                    BindingOperations.SetBinding(column, GenerateValueDataGridColumn.EditorStyleProperty, new Binding(nameof(ValueEditorStyle)) { Source = this });
                    this.Columns.Add(column);
                }
                if (IsOperable)
                {
                    var column = new GenerateOperatorGridColumn();
                    BindingOperations.SetBinding(column, GenerateOperatorGridColumn.EditorStyleProperty, new Binding(nameof(OperatorStyle)) { Source = this });
                    this.Columns.Add(column);
                }
            }
           
            this.InvalidateVisual();
        }
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            DataGridRow row = (DataGridRow)element;
            row.SetBinding(EFDataGridAssist.IsRowMouseOverProperty, new Binding(nameof(row.IsMouseOver)) { Source = row, Mode = BindingMode.OneWay });
            row.SetBinding(EFDataGridAssist.IsRowSelectedProperty, new Binding(nameof(row.IsSelected)) { Source = row, Mode = BindingMode.OneWay });
            base.PrepareContainerForItemOverride(element, item);
        }
    }
    public static class EFDataGridAssist
    {
        public static readonly DependencyProperty DisplayPropertyNamesProperty =
                DependencyProperty.RegisterAttached("DisplayPropertyNames", typeof(Dictionary<string, string>), typeof(EFDataGridAssist), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsRowMouseOverProperty =
                DependencyProperty.RegisterAttached("IsRowMouseOver", typeof(bool), typeof(EFDataGridAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsRowSelectedProperty =
                DependencyProperty.RegisterAttached("IsRowSelected", typeof(bool), typeof(EFDataGridAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsRowEditingProperty =
                DependencyProperty.RegisterAttached("IsRowEditing", typeof(bool), typeof(EFDataGridAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty ItemsSourceProperty =
                DependencyProperty.RegisterAttached("ItemsSource", typeof(IEnumerable), typeof(EFDataGridAssist), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static Dictionary<string, string> GetDisplayPropertyNames(DependencyObject obj)
        {
            return (Dictionary<string, string>)obj.GetValue(DisplayPropertyNamesProperty);
        }
        public static void SetDisplayPropertyNames(DependencyObject obj, Dictionary<string, string> value)
        {
            obj.SetValue(DisplayPropertyNamesProperty, value);
        }
        public static bool GetIsRowMouseOver(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRowMouseOverProperty);
        }
        public static void SetIsRowMouseOver(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRowMouseOverProperty, value);
        }
        public static bool GetIsRowSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRowSelectedProperty);
        }
        public static void SetIsRowSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRowSelectedProperty, value);
        }
        public static bool GetIsRowEditing(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRowEditingProperty);
        }
        public static void SetIsRowEditing(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRowEditingProperty, value);
        }
        public static IEnumerable GetItemsSource(DependencyObject obj)
        {
            return (IEnumerable)obj.GetValue(ItemsSourceProperty);
        }
        public static void SetItemsSource(DependencyObject obj, IEnumerable value)
        {
            obj.SetValue(ItemsSourceProperty, value);
        }
    }
    class GenerateValueDataGridColumn : DataGridColumn
    {
        public static readonly DependencyProperty EditorStyleProperty =
            DependencyProperty.Register(nameof(EditorStyle), typeof(Style), typeof(GenerateValueDataGridColumn), new PropertyMetadata(null));
        public GenerateValueDataGridColumn(string propertyName)
        {
            IsReadOnly = false;
            PropertyName = propertyName;
        }
        public Style EditorStyle
        {
            get { return (Style)GetValue(EditorStyleProperty); }
            set { SetValue(EditorStyleProperty, value); }
        }
        public string PropertyName { get; }
        public string Property { get; set; }
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var editor = new EFValueEditor(PropertyName);
            editor.SetBinding(FrameworkElement.StyleProperty, new Binding(nameof(EditorStyle)) { Source = this });
            return editor;
        }
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var editor = new EFValueEditor(PropertyName);
            editor.SetBinding(FrameworkElement.StyleProperty, new Binding(nameof(EditorStyle)) { Source = this });
            return editor;
        }
    }
    class GenerateOperatorGridColumn : DataGridColumn
    {
        public static readonly DependencyProperty EditorStyleProperty =
          DependencyProperty.Register(nameof(EditorStyle), typeof(Style), typeof(GenerateValueDataGridColumn), new PropertyMetadata(null));
        public GenerateOperatorGridColumn()
        {
            IsReadOnly = false;
            Width = new DataGridLength(0, DataGridLengthUnitType.Star);
        }
        public Style EditorStyle
        {
            get { return (Style)GetValue(EditorStyleProperty); }
            set { SetValue(EditorStyleProperty, value); }
        }
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var editor = new EFOperator();
            editor.SetBinding(FrameworkElement.StyleProperty, new Binding(nameof(EditorStyle)) { Source = this });
            return editor;
        }
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var editor = new EFOperator();
            editor.SetBinding(FrameworkElement.StyleProperty, new Binding(nameof(EditorStyle)) { Source = this });
            return editor;
        }
    }
}