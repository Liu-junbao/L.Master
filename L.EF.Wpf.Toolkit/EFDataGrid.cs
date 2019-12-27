using System.Collections;
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
    public class EFDataGrid : DataGrid
    {
        public static readonly DependencyProperty IsOperableProperty =
           DependencyProperty.Register(nameof(IsOperable), typeof(bool), typeof(EFDataGrid), new PropertyMetadata(true, OnPropertyChanged));
        public static readonly DependencyProperty DisplayPropertyInfosProperty =
           DependencyProperty.Register(nameof(DisplayPropertyInfos), typeof(IEnumerable<EFDisplayPropertyInfo>), typeof(EFDataGrid), new PropertyMetadata(null,OnPropertyChanged));
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
            this.SetBinding(DisplayPropertyInfosProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.DisplayPropertyInfosProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(ItemsSourceProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.ItemsSourceProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
        }
        public bool IsOperable
        {
            get { return (bool)GetValue(IsOperableProperty); }
            set { SetValue(IsOperableProperty, value); }
        }
        public IEnumerable<EFDisplayPropertyInfo> DisplayPropertyInfos
        {
            get { return (IEnumerable<EFDisplayPropertyInfo>)GetValue(DisplayPropertyInfosProperty); }
            set { SetValue(DisplayPropertyInfosProperty, value); }
        }
        private void Invalidate()
        {
            this.Columns.Clear();
            if (DisplayPropertyInfos != null)
            {
                foreach (var item in DisplayPropertyInfos)
                {
                    var propertyName = item.PropertyName;
                    var propertyType = item.PropertyType;
                    var genericName = item.GenericName;
                    var isReadOnly = item.IsReadOnly;
                    var column = new GenerateValueDataGridColumn(propertyName, propertyType) { Header = genericName, IsReadOnly = isReadOnly };
                    this.Columns.Add(column);
                }
                if (IsOperable)
                {
                    var column = new GenerateOperatorGridColumn();
                    this.Columns.Add(column);
                }
            }
           
            this.InvalidateVisual();
        }     
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            DataGridRow row = (DataGridRow)element;
            row.SetBinding(EFDataBoxAssist.IsRowMouseOverProperty, new Binding(nameof(row.IsMouseOver)) { Source = row, Mode = BindingMode.OneWay });
            row.SetBinding(EFDataBoxAssist.IsRowSelectedProperty, new Binding(nameof(row.IsSelected)) { Source = row, Mode = BindingMode.OneWay });
           
            base.PrepareContainerForItemOverride(element, item);
        }
        protected override void OnLoadingRow(DataGridRowEventArgs e)
        {
            e.Row.Unselected += Row_Unselected;
            e.Row.AddHandler(EFValueEditor.ValueChangedEvent, new RoutedEventHandler(OnRowValueChanged));
            base.OnLoadingRow(e);
        }
        protected override void OnUnloadingRow(DataGridRowEventArgs e)
        {
            e.Row.Unselected -= Row_Unselected;
            e.Row.RemoveHandler(EFValueEditor.ValueChangedEvent, new RoutedEventHandler(OnRowValueChanged));
            base.OnUnloadingRow(e);
        }
        private void Row_Unselected(object sender, RoutedEventArgs e)
        {
            EFDataBoxAssist.SetIsRowEditing((DataGridRow)sender, false);
        }
        private void OnRowValueChanged(object sender, RoutedEventArgs e)
        {
            var row = sender as DataGridRow;
            bool isChanged = false;
            foreach (var item in row.FindChildren<EFValueEditor>())
            {
                if (item.IsValueChanged)
                {
                    isChanged = true;
                    break;
                }
            }
            EFDataBoxAssist.SetIsRowValueChanged(row, isChanged);
        }
    }
    class GenerateValueDataGridColumn : DataGridColumn
    {
        public GenerateValueDataGridColumn(string propertyName,Type propertyType)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
        }
        public string PropertyName { get; }
        public Type PropertyType { get; }
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var editor = new EFValueEditor(PropertyName, PropertyType);
            editor.SetBinding(EFEditorBase.IsReadOnlyProperty, new Binding(nameof(IsReadOnly)) { Source = this });
            return editor;
        }
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var editor = new EFValueEditor(PropertyName,PropertyType);
            editor.SetBinding(EFEditorBase.IsReadOnlyProperty, new Binding(nameof(IsReadOnly)) { Source = this });
            return editor;
        }
    }
    class GenerateOperatorGridColumn : DataGridColumn
    {
        public GenerateOperatorGridColumn()
        {
            Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var editor = new EFOperator();
            return editor;
        }
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var editor = new EFOperator();
            return editor;
        }
    }
}
