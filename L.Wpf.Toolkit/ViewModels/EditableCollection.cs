using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace System
{
    public class EditableCollection<TModel> : IEnumerable, INotifyCollectionChanged
    {
        private ObservableCollection<EditableViewModel> _viewModels;
        private Dictionary<object, EditableViewModel> _key_viewModels;
        private Func<TModel, List<Tuple<string, object>>, Task> _saveAction;
        public EditableCollection(Func<TModel, List<Tuple<string, object>>, Task> saveAction = null)
        {
            _viewModels = new ObservableCollection<EditableViewModel>();
            _viewModels.CollectionChanged += ViewModels_CollectionChanged;
            _key_viewModels = new Dictionary<object, EditableViewModel>();
            _saveAction = saveAction;
        }
        public void Change<TKey>(Dictionary<TKey, TModel> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            //olds
            var oldPairs = _key_viewModels.Where(i => (i.Key is TKey) == false || source.ContainsKey((TKey)i.Key) == false).ToList();
            foreach (var item in oldPairs)
            {
                _key_viewModels.Remove(item.Key);
            }
            //news
            var newKeys = source.Where(i => _key_viewModels.ContainsKey(i.Key) == false).ToList();
            var newItems = new List<EditableViewModel>();
            foreach (var item in newKeys)
            {
                var viewModel = new EditableViewModel(OnSaveItem);
                _key_viewModels.Add(item.Key, viewModel);
                newItems.Add(viewModel);
            }
            //exist
            foreach (var item in _key_viewModels)
            {
                item.Value.Source = source[(TKey)item.Key];
            }
            //update viewModels
            UIInvoke(() =>
            {
                foreach (var item in oldPairs)
                {
                    _viewModels.Remove(item.Value);
                }
                foreach (var item in newItems)
                {
                    _viewModels.Add(item);
                }
            });
        }
        public void Change(IEnumerable<TModel> models) => Change(models.ToDictionary(i => i));
        private Task OnSaveItem(object source, List<Tuple<string, object>> changeValues)
        {
            return _saveAction?.Invoke((TModel)source, changeValues);
        }
        public void Clear()
        {
            _key_viewModels.Clear();
            UIInvoke(() => _viewModels.Clear());
        }
        private void UIInvoke(Action action)
        {
            if (Thread.CurrentThread != Application.Current.Dispatcher.Thread)
                Application.Current.Dispatcher.BeginInvoke(action);
            else
                action();
        }
        public IEnumerator GetEnumerator()
        {
            return _key_viewModels.Values.GetEnumerator();
        }
        private void ViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
    public class EditableViewModel : NotifyPropertyChanged
    {
        private object _source;
        private bool _isEnabled;
        private bool _isEditing;
        private bool _isSelected;
        private Func<object, List<Tuple<string, object>>, Task> _saveAction;
        public EditableViewModel(Func<object, List<Tuple<string, object>>, Task> saveAction)
        {
            _isEnabled = true;
            _saveAction = saveAction;
        }
        public object Source
        {
            get { return _source; }
            set { SetProperty(ref _source, value); }
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
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value, OnIsSelectedChanged); }
        }
        protected virtual void OnIsSelectedChanged(bool oldIsSelected, bool newIsSelected)
        {
            //
            if (newIsSelected == false)
            {
                IsEditing = false;
            }
        }
        internal async Task RaiseSaveAsync()
        {
            var arg = new EditableViewModelEditedEventArgs(new List<Tuple<string, object>>());
            OnEditedEvent?.Invoke(this, arg);
            if (_saveAction != null)
            {
                IsEnabled = false;
                try
                {
                    await _saveAction.Invoke(this.Source, arg.EditedValues);
                }
                finally
                {
                    IsEnabled = true;
                    IsEditing = false;
                }
            }
        }
        internal event EventHandler<EditableViewModelEditedEventArgs> OnEditedEvent;
    }
    internal class EditableViewModelEditedEventArgs : EventArgs
    {
        public EditableViewModelEditedEventArgs(List<Tuple<string, object>> editedValues)
        {
            EditedValues = editedValues;
        }
        public List<Tuple<string, object>> EditedValues { get; }
    }
    public abstract class EditorViewModel:DependencyObject
    {
        public static readonly DependencyProperty IsEnabledProperty =
           DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(EditorViewModel), new PropertyMetadata(true));
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EditorViewModel), new PropertyMetadata(false,OnIsEditingChanged));
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(EditorViewModel), new PropertyMetadata(false,OnIsSelectedChanged));
        
        private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EditorViewModel)d;
            editor.OnIsEditingChanged((bool)e.NewValue);
        }
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (EditorViewModel)d;
            editor.OnIsSelectedChanged((bool)e.NewValue);
        }
        public EditorViewModel(EditableViewModel model)
        {
            BindingOperations.SetBinding(this, IsEnabledProperty, new Binding(nameof(EditableViewModel.IsEnabled)) { Source = model, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(this, IsEditingProperty, new Binding(nameof(EditableViewModel.IsEditing)) { Source = model, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(this, IsSelectedProperty, new Binding(nameof(EditableViewModel.IsSelected)) { Source = model, Mode = BindingMode.TwoWay });
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

    #region valueEditor
    public class ValueEditorViewModel : EditorViewModel
    {
        public static readonly DependencyProperty ValueProperty =
           DependencyProperty.Register(nameof(Value), typeof(object), typeof(ValueEditorViewModel), new PropertyMetadata(null, OnValueChanged));
        public static readonly DependencyProperty EditedValueProperty =
           DependencyProperty.Register(nameof(EditedValue), typeof(object), typeof(ValueEditorViewModel), new PropertyMetadata(null, OnEditedValueChanged));
        public static readonly DependencyProperty IsChangedProperty =
           DependencyProperty.Register(nameof(IsChanged), typeof(bool), typeof(ValueEditorViewModel), new PropertyMetadata(null));
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (ValueEditorViewModel)d;
            editor.EditedValue = e.NewValue;
        }
        private static void OnEditedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (ValueEditorViewModel)d;
            editor.IsChanged = e.NewValue != editor.Value;
        }
        public ValueEditorViewModel(EditableViewModel model, string propertyName) : base(model)
        {
            PropertyName = propertyName;
            BindingOperations.SetBinding(this, ValueProperty, new Binding($"{nameof(EditableViewModel.Source)}.{propertyName}") { Source = model });
            model.OnEditedEvent += Model_OnEditedEvent;
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
            set { SetValue(IsChangedProperty, value); }
        }
        protected override void OnIsEditingChanged(bool isEditing)
        {
            base.OnIsEditingChanged(isEditing);
            if (isEditing == false)
            {
                EditedValue = Value;
            }
        }
        private void Model_OnEditedEvent(object sender, EditableViewModelEditedEventArgs e)
        {
            if (IsChanged)
            {
                e.EditedValues.Add(new Tuple<string, object>(PropertyName, EditedValue));
            }
        }
    }
    public class ValueEditorViewModelConverter : IValueConverter
    {
        public ValueEditorViewModelConverter(string propertyName)
        {
            PropertyName = propertyName;
        }
        public ValueEditorViewModelConverter() { }
        public string PropertyName { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return new ValueEditorViewModel((EditableViewModel)value, PropertyName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ValueEditorViewModelConverterExtension : NewMarkupExtension<ValueEditorViewModelConverter>
    {
        public ValueEditorViewModelConverterExtension(string propertyName)
        {
            PropertyName = propertyName;
        }
        public ValueEditorViewModelConverterExtension() { }
        [ConstructorArgument("propertyName")]
        public string PropertyName { get; set; }
        protected override object[] CreateArgs()
        {
            return new object[] { PropertyName };
        }
    }

    #endregion

    #region operationEditor
    public class OperationEditorViewModel : EditorViewModel
    {
        private static readonly DependencyPropertyKey EditCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(EditCommand), typeof(ICommand), typeof(OperationEditorViewModel), new PropertyMetadata(null));
        public static readonly DependencyProperty EditCommandProperty = EditCommandPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey CancelCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CancelCommand), typeof(ICommand), typeof(OperationEditorViewModel), new PropertyMetadata(null));
        public static readonly DependencyProperty CancelCommandProperty = CancelCommandPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey SaveCommandPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(SaveCommand), typeof(ICommand), typeof(OperationEditorViewModel), new PropertyMetadata(null));
        public static readonly DependencyProperty SaveCommandProperty = SaveCommandPropertyKey.DependencyProperty;

        public OperationEditorViewModel(EditableViewModel model) : base(model)
        {
            EditCommand = new Command(OnEdit);
            CancelCommand = new Command(OnCancel);
            SaveCommand = new AsyncCommand(model.RaiseSaveAsync);
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
            IsEditing = true;
        }
        private void OnCancel()
        {
            IsEditing = false;
        }
    }
    public class OperationEditorViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return new OperationEditorViewModel((EditableViewModel)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class OperationEditorViewModelConverterExtension : NewMarkupExtension<OperationEditorViewModelConverter> { }
    #endregion
}
