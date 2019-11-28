using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows
{
    #region viewModel
    /// <summary>
    /// 可编辑视图
    /// </summary>
    public class EditableViewModel : NotifyPropertyChanged
    {
        private object _source;
        private bool _isEnabled;
        private bool _isEditing;
        private bool _isEditable;
        private ISourceService _sourceService;
        public EditableViewModel()
        {
            _isEnabled = true;
        }
        public object Source
        {
            get { return _source; }
            set { SetProperty(ref _source, value); }
        }
        public ISourceService SourceService
        {
            get { return _sourceService; }
            set { SetProperty(ref _sourceService, value); }
        }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }
        public bool IsEditing
        {
            get { return _isEditing; }
            set { SetProperty(ref _isEditing, value); }
        }
        public bool IsEditable
        {
            get { return _isEditable; }
            set { SetProperty(ref _isEditable, value, OnIsEditableChanged); }
        }
        protected virtual void OnIsEditableChanged(bool oldIsSelected, bool newIsSelected)
        {
            //
            if (newIsSelected == false)
            {
                IsEditing = false;
            }
        }
        public async Task RaiseSaveAsync()
        {
            var sourceService = SourceService;
            if (sourceService != null)
            {
                var arg = new EditableViewModelSavingEventArgs(new Dictionary<string, object>());
                SavingEvent?.Invoke(this, arg);
                IsEnabled = false;
                try
                {
                    var source = this.Source;
                    Dictionary<PropertyInfo, object> changedPropertyValues;
                    if (CheckValues(sourceService, source, arg.EditedValues, out changedPropertyValues))
                    {
                        await sourceService.SaveChangedPropertys(source, changedPropertyValues);
                    }
                }
                finally
                {
                    IsEnabled = true;
                    IsEditing = false;
                }
                SavedEvent?.Invoke(this, null);
            }
        }
        private bool CheckValues(ISourceService sourceService, object source, Dictionary<string, object> changedValues, out Dictionary<PropertyInfo, object> changedPropertyValues)
        {
            changedPropertyValues = null;
            if (source == null) return false;
            var type = source.GetType();
            changedPropertyValues = new Dictionary<PropertyInfo, object>();
            foreach (var item in changedValues)
            {
                var property = type.GetProperty(item.Key);
                if (property != null)
                {
                    object value;
                    if (TryConvert(item.Value, property.PropertyType, out value))
                    {
                        changedPropertyValues.Add(property, value);
                    }
                    else
                    {
                        sourceService.OnCaptureErrorEditedValue(item.Key, item.Value);
                        return false;
                    }
                }
            }
            return changedPropertyValues.Count > 0;
        }
        private bool TryConvert(object value, Type type, out object result)
        {
            result = null;
            try
            {
                result = Convert.ChangeType(value, type);
                return true;
            }
            catch { }
            return false;
        }
        public event EventHandler<EditableViewModelSavingEventArgs> SavingEvent;
        public event EventHandler SavedEvent;
    }
    public class EditableViewModelSavingEventArgs : EventArgs
    {
        public EditableViewModelSavingEventArgs(Dictionary<string, object> editedValues)
        {
            EditedValues = editedValues;
        }
        public Dictionary<string, object> EditedValues { get; }
    }
    public interface ISourceService
    {
        void OnCaptureErrorEditedValue(string propertyName, object editedValue);
        Task SaveChangedPropertys(object source, Dictionary<PropertyInfo, object> changedPropertys);
    }
    #endregion

    #region editor
    public abstract class EditableViewModelEditor : Control
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(EditableViewModel), typeof(EditableViewModelEditor), new PropertyMetadata(null, OnEditableViewModelChanged));
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EditableViewModelEditor), new PropertyMetadata(false, OnIsEditingChanged));
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(EditableViewModelEditor), new PropertyMetadata(false, OnIsEditableChanged));
        private static void OnEditableViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EditableViewModelEditor)d;
            editor.OnEditableViewModelChanged((EditableViewModel)e.OldValue, (EditableViewModel)e.NewValue);
        }
        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EditableViewModelEditor)d;
            editor.OnIsEditingChanged((bool)e.NewValue);
        }
        private static void OnIsEditableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EditableViewModelEditor)d;
            editor.OnIsEditableChanged((bool)e.NewValue);
        }
        public EditableViewModel ViewModel
        {
            get { return (EditableViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }
        protected virtual void OnIsEditingChanged(bool isEditing)
        {
            var viewModel = ViewModel;
            if (viewModel != null)
                viewModel.IsEditing = isEditing;
        }
        protected virtual void OnIsEditableChanged(bool isEditable)
        {
            var viewModel = ViewModel;
            if (viewModel != null)
                viewModel.IsEditable = isEditable;
        }
        protected virtual void OnEditableViewModelChanged(EditableViewModel oldViewModel, EditableViewModel newViewModel)
        {
            if (oldViewModel != null)
            {
                newViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                oldViewModel.SavingEvent -= SaveEvent;
                oldViewModel.SavedEvent -= SavedEvent;
            }
            if (newViewModel != null)
            {
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
                newViewModel.SavingEvent += SaveEvent;
                newViewModel.SavedEvent += SavedEvent;

                IsEditing = newViewModel.IsEditing;
                IsEditable = newViewModel.IsEditable;

                var sourceService = EditableViewModelAssist.GetSourceService(this);
                if (sourceService != null)
                    newViewModel.SourceService = sourceService;
            }
        }
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           
            switch (e.PropertyName)
            {
                case nameof(EditableViewModel.IsEditing):
                    IsEditing = ViewModel.IsEditing;
                    break;
                case nameof(EditableViewModel.IsEditable):
                    IsEditable = ViewModel.IsEditable;
                    break;
                default:
                    break;
            }
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == EditableViewModelAssist.SourceServiceProperty)
            {
                var sourceService = EditableViewModelAssist.GetSourceService(this);
                var viewModel = ViewModel;
                if (sourceService != null && viewModel != null)
                {
                    viewModel.SourceService = sourceService;
                }
            }
            else if (e.Property == EditableViewModelAssist.IsEditableProperty)
            {
                IsEditable = EditableViewModelAssist.GetIsEditable(this);
            }
        }
        private void SaveEvent(object sender, EditableViewModelSavingEventArgs e)
        {
            OnSaving(e);
        }
        private void SavedEvent(object sender, EventArgs e)
        {
            OnSaved();
        }
        protected virtual void OnSaving(EditableViewModelSavingEventArgs e) { }
        protected virtual void OnSaved() { }
    }
    public class TextEditor : EditableViewModelEditor
    {
        public static readonly DependencyProperty EditedSourcePropertyNameProperty =
            DependencyProperty.Register(nameof(EditedSourcePropertyName), typeof(string), typeof(TextEditor), new PropertyMetadata(null));
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(TextEditor), new PropertyMetadata(null, OnValueChanged));
        public static readonly DependencyProperty EditedValueProperty =
            DependencyProperty.Register(nameof(EditedValue), typeof(object), typeof(TextEditor), new PropertyMetadata(null, OnEditedValueChanged));
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(TextEditor), new PropertyMetadata(false));
        public static readonly DependencyProperty IsChangedProperty =
            DependencyProperty.Register(nameof(IsChanged), typeof(bool), typeof(TextEditor), new PropertyMetadata(null));
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (TextEditor)d;
            editor.EditedValue = e.NewValue;
        }
        private static void OnEditedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (TextEditor)d;
            editor.IsChanged = e.NewValue != editor.Value;
        }
        private BindingExpressionBase _valueBinding;
        static TextEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextEditor), new FrameworkPropertyMetadata(typeof(TextEditor)));
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
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public bool IsChanged
        {
            get { return (bool)GetValue(IsChangedProperty); }
            set { SetValue(IsChangedProperty, value); }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _valueBinding = BindingOperations.SetBinding(this, ValueProperty, new Binding($"{nameof(ViewModel)}.{nameof(ViewModel.Source)}.{EditedSourcePropertyName}") { Source = this });
        }
        protected override void OnIsEditingChanged(bool isEditing)
        {
            base.OnIsEditingChanged(isEditing);
            if (isEditing == false)
            {
                EditedValue = Value;
            }
        }
        protected override void OnSaving(EditableViewModelSavingEventArgs e)
        {
            base.OnSaving(e);
            var propertyName = EditedSourcePropertyName;
            if (string.IsNullOrEmpty(propertyName) == false && IsChanged)
            {
                if (e.EditedValues.ContainsKey(propertyName))
                    e.EditedValues.Add(propertyName, EditedValue);
                else
                    e.EditedValues[propertyName] = EditedValue;
            }
        }
        protected override void OnSaved()
        {
            base.OnSaved();
            _valueBinding?.UpdateTarget();
        }
    }
    [ContentProperty(nameof(Selections))]
    public class ListEditor : TextEditor
    {
        static ListEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListEditor), new FrameworkPropertyMetadata(typeof(ListEditor)));
        }
        public ListEditor()
        {
            Selections = new ObservableCollection<object>();
        }
        public ObservableCollection<object> Selections { get; }
    }
    public class OperEditor : EditableViewModelEditor
    {
        static OperEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OperEditor), new FrameworkPropertyMetadata(typeof(OperEditor)));
        }
        public OperEditor()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, new ExecutedRoutedEventHandler(OnPen)));
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
            if (ViewModel != null)
            {
                await ViewModel.RaiseSaveAsync();
            }
        }
    }
    #endregion

    [ContentProperty(nameof(Selections))]
    public class EditableDataGridColumn : DataGridColumn
    {
        public static readonly DependencyProperty EditedPropertyNameProperty =
            DependencyProperty.Register(nameof(EditedPropertyName), typeof(string), typeof(EditableDataGridColumn), new PropertyMetadata(null));
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(EditorKind), typeof(EditableDataGridColumn), new PropertyMetadata(EditorKind.Text));
        public EditableDataGridColumn()
        {
            Selections = new List<object>();
        }
        public string EditedPropertyName
        {
            get { return (string)GetValue(EditedPropertyNameProperty); }
            set { SetValue(EditedPropertyNameProperty, value); }
        }
        public EditorKind Kind
        {
            get { return (EditorKind)GetValue(KindProperty); }
            set { SetValue(KindProperty, value); }
        }
        public List<object> Selections { get; }
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GetEditor(cell, dataItem);
        }
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            return GetEditor(cell, dataItem);
        }
        private FrameworkElement GetEditor(DataGridCell cell, object dataItem)
        {
            EditableViewModelEditor editor = null;
            switch (Kind)
            {
                case EditorKind.Text:
                    editor = new TextEditor() { EditedSourcePropertyName = EditedPropertyName };
                    break;
                case EditorKind.List:
                    var selectableValueEditor = new ListEditor() { EditedSourcePropertyName = EditedPropertyName };
                    foreach (var item in Selections)
                    {
                        selectableValueEditor.Selections.Add(item);
                    }
                    editor = selectableValueEditor;
                    break;
                case EditorKind.Oper:
                    editor = new OperEditor();
                    break;
                default:
                    break;
            }

            if (editor != null)
            {
                editor.SetBinding(TextEditor.IsReadOnlyProperty, new Binding(nameof(IsReadOnly)) { Source = this });

                EditableViewModel model = null;
                if (dataItem is EditableViewModel)
                    model = (EditableViewModel)dataItem;
                else
                {
                    model = GetEditableViewModel(cell);
                    if (model == null || model.Source != dataItem)
                    {
                        model = new EditableViewModel() { Source = dataItem };
                        var row = FindRow(cell);
                        SetEditableViewModel(row, model);

                        //row.SetBinding(EditableViewModelAssist.SourceServiceProperty, new Binding(nameof(EditableViewModel.SourceService)) { Source = model, Mode = BindingMode.TwoWay });
                        //row.SetBinding(EditableViewModelAssist.IsEditableProperty, new Binding(nameof(EditableViewModel.IsEditable)) { Source = model, Mode = BindingMode.TwoWay });
                    }
                }

                editor.ViewModel = model;
                //editor.SetBinding(EditableViewModelEditor.IsEditableProperty, new Binding($"({nameof(EditableViewModelAssist)}.IsEditable)") { Source = editor, Mode = BindingMode.OneWay });
                //cell.SetBinding(EditableViewModelAssist.SourceServiceProperty, new Binding(nameof(editor.SourceService)) { Source = editor, Mode = BindingMode.OneWayToSource });
                //cell.SetBinding(EditableViewModelAssist.IsEditableProperty, new Binding(nameof(editor.IsEditable)) { Source = editor, Mode = BindingMode.OneWayToSource });
            }

            return editor;
        }
        private DataGridRow FindRow(DataGridCell cell)
        {
            DependencyObject child = cell;
            do
            {
                var parent = LogicalTreeHelper.GetParent(child);
                if (parent == null)
                    parent = VisualTreeHelper.GetParent(child);
                if (parent is DataGridRow) return parent as DataGridRow;
                child = parent;
            } while (child != null);
            return null;
        }
        internal static EditableViewModel GetEditableViewModel(DependencyObject obj)
        {
            return (EditableViewModel)obj.GetValue(EditableViewModelProperty);
        }
        internal static void SetEditableViewModel(DependencyObject obj, EditableViewModel value)
        {
            obj.SetValue(EditableViewModelProperty, value);
        }
        internal static readonly DependencyProperty EditableViewModelProperty =
            DependencyProperty.RegisterAttached("EditableViewModel", typeof(EditableViewModel), typeof(EditableDataGridColumn), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
    }
    public enum EditorKind
    {
        Text,
        List,
        Oper,
    }
    public static class EditableViewModelAssist
    {
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.RegisterAttached("IsEditable", typeof(bool), typeof(EditableViewModelAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty SourceServiceProperty =
            DependencyProperty.RegisterAttached("SourceService", typeof(ISourceService), typeof(EditableViewModelAssist), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
       

        public static bool GetIsEditable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEditableProperty);
        }
        public static void SetIsEditable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEditableProperty, value);
        }
        public static ISourceService GetSourceService(DependencyObject obj)
        {
            return (ISourceService)obj.GetValue(SourceServiceProperty);
        }
        public static void SetSourceService(DependencyObject obj, ISourceService value)
        {
            obj.SetValue(SourceServiceProperty, value);
        }
    }
}
