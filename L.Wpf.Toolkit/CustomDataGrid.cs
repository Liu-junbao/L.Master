using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
    [ContentProperty(nameof(Columns))]
    public class CustomDataGrid:DataGrid
    {
        static CustomDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomDataGrid), new FrameworkPropertyMetadata(typeof(CustomDataGrid)));
        }
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CustomDataGridRow;
        }
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CustomDataGridRow(this);
        }
    }
    public class CustomDataGridRow:DataGridRow
    {
        private CustomDataGrid _owner;
        public CustomDataGridRow(CustomDataGrid owner)
        {
            _owner = owner;
        }
    }
    public class CustomDataGridBar: Control
    {
        #region commands
        private static RoutedUICommand _moveToFirstCommand;
        public static ICommand MoveToFirstCommand
        {
            get
            {
                if (_moveToFirstCommand == null)
                {
                    _moveToFirstCommand = new RoutedUICommand("move to first command", nameof(MoveToFirstCommand), typeof(CustomDataGridBar));
                    //注册热键
                    //_moveToFirstCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _moveToFirstCommand;
            }
        }

        private static RoutedUICommand _moveToLastCommand;
        public static ICommand MoveToLastCommand
        {
            get
            {
                if (_moveToLastCommand == null)
                {
                    _moveToLastCommand = new RoutedUICommand("move to last command", nameof(MoveToLastCommand), typeof(CustomDataGridBar));
                    //注册热键
                    //_moveToLastCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _moveToLastCommand;
            }
        }

        private static RoutedUICommand _moveToNextCommand;
        public static ICommand MoveToNextCommand
        {
            get
            {
                if (_moveToNextCommand == null)
                {
                    _moveToNextCommand = new RoutedUICommand("move to next command", nameof(MoveToNextCommand), typeof(CustomDataGridBar));
                    //注册热键
                    //_moveToNextCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _moveToNextCommand;
            }
        }

        private static RoutedUICommand _moveToPreviousCommand;
        public static ICommand MoveToPreviousCommand
        {
            get
            {
                if (_moveToPreviousCommand == null)
                {
                    _moveToPreviousCommand = new RoutedUICommand("move to previous command", nameof(MoveToPreviousCommand), typeof(CustomDataGridBar));
                    //注册热键
                    //_moveToPreviousCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _moveToPreviousCommand;
            }
        }

        private static RoutedUICommand _moveToCurrentCommand;
        public static ICommand MoveToCurrentCommand
        {
            get
            {
                if (_moveToCurrentCommand == null)
                {
                    _moveToCurrentCommand = new RoutedUICommand("move to current command", nameof(MoveToCurrentCommand), typeof(CustomDataGridBar));
                    //注册热键
                    //_moveToCurrentCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _moveToCurrentCommand;
            }
        }

        #endregion

        public static readonly DependencyProperty CountProperty =
          DependencyProperty.Register(nameof(Count), typeof(int), typeof(CustomDataGridBar), new PropertyMetadata(0));
        public static readonly DependencyProperty PageCountProperty =
          DependencyProperty.Register(nameof(PageCount), typeof(int), typeof(CustomDataGridBar), new PropertyMetadata(0));
        public static readonly DependencyProperty PageIndexProperty =
          DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(CustomDataGridBar), new PropertyMetadata(0));
        public static readonly DependencyProperty DisplayCountProperty =
          DependencyProperty.Register(nameof(DisplayCount), typeof(int), typeof(CustomDataGridBar), new PropertyMetadata(5));
        private static readonly DependencyPropertyKey DisplayIndexStatusPropertyKey =
          DependencyProperty.RegisterReadOnly(nameof(DisplayIndexStatus), typeof(DisplayIndexStatus), typeof(CustomDataGridBar), new PropertyMetadata(DisplayIndexStatus.None));
        public static readonly DependencyProperty DisplayIndexStatusProperty = DisplayIndexStatusPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey DisplayIndexSourcePropertyKey =
          DependencyProperty.RegisterReadOnly(nameof(DisplayIndexSource), typeof(int[]), typeof(CustomDataGridBar), new PropertyMetadata(null));
        public static readonly DependencyProperty DisplayIndexSourceProperty = DisplayIndexSourcePropertyKey.DependencyProperty;
        static CustomDataGridBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomDataGridBar),new FrameworkPropertyMetadata(typeof(CustomDataGridBar)));
        }
        private int _displayIndex;
        public CustomDataGridBar()
        {
            this.CommandBindings.Add(new CommandBinding(MoveToFirstCommand,new ExecutedRoutedEventHandler(OnMoveToFirst)));
            this.CommandBindings.Add(new CommandBinding(MoveToLastCommand, new ExecutedRoutedEventHandler(OnMoveToLast)));
            this.CommandBindings.Add(new CommandBinding(MoveToNextCommand, new ExecutedRoutedEventHandler(OnMoveToNext)));
            this.CommandBindings.Add(new CommandBinding(MoveToPreviousCommand, new ExecutedRoutedEventHandler(OnMoveToPrevious)));
            this.CommandBindings.Add(new CommandBinding(MoveToCurrentCommand, new ExecutedRoutedEventHandler(OnMoveToCurrent)));
        }

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }
        public int PageCount
        {
            get { return (int)GetValue(PageCountProperty); }
            set { SetValue(PageCountProperty, value); }
        }
        public int PageIndex
        {
            get { return (int)GetValue(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }
        public int DisplayCount
        {
            get { return (int)GetValue(DisplayCountProperty); }
            set { SetValue(DisplayCountProperty, value); }
        }
        public DisplayIndexStatus DisplayIndexStatus
        {
            get { return (DisplayIndexStatus)GetValue(DisplayIndexStatusProperty); }
            protected set { SetValue(DisplayIndexStatusPropertyKey, value); }
        }
        public int[] DisplayIndexSource
        {
            get { return (int[])GetValue(DisplayIndexSourceProperty); }
            protected set { SetValue(DisplayIndexSourcePropertyKey, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == PageCountProperty || e.Property == DisplayCountProperty)
            {
                _displayIndex = 0;
                InvalidateDisplayIndex();
            }
            else if (e.Property == PageIndexProperty)
            {
                BringPageIndexIntoView();
            }
        }
        private void OnMoveToFirst(object sender, ExecutedRoutedEventArgs e)
        {
            _displayIndex = 0;
            InvalidateDisplayIndex();
        }
        private void OnMoveToLast(object sender, ExecutedRoutedEventArgs e)
        {
            _displayIndex = PageCount;
            InvalidateDisplayIndex();
        }
        private void OnMoveToPrevious(object sender, ExecutedRoutedEventArgs e)
        {
            _displayIndex--;
            InvalidateDisplayIndex();
        }
        private void OnMoveToNext(object sender, ExecutedRoutedEventArgs e)
        {
            _displayIndex++;
            InvalidateDisplayIndex();
        }
        private void OnMoveToCurrent(object sender, ExecutedRoutedEventArgs e)
        {
            BringPageIndexIntoView();
        }
        private void BringPageIndexIntoView()
        {
            var pageIndex = PageIndex;
            if (pageIndex < 0)
            {
                PageIndex = 0;
                return;
            }
            if (pageIndex > PageCount)
            {
                PageIndex = PageCount;
                return;
            }
            var source = DisplayIndexSource;
            if (source == null || source.Contains(pageIndex) == false)
            {
                var displayCount = DisplayCount;
                if (displayCount <= 0)
                    displayCount = 1;
                int remain;
                var displayIndex = Math.DivRem(pageIndex, displayCount, out remain) - 1;
                if (remain > 0)
                    displayIndex++;
                _displayIndex = displayIndex;
                InvalidateDisplayIndex();
            }
        }
        private void InvalidateDisplayIndex()
        {
            var count = PageCount;
            var targetDisplayIndex = _displayIndex;
            if (count > 0)
            {
                var displayCount = DisplayCount;
                if (displayCount < 1) displayCount = 1;
                if (targetDisplayIndex < 0) targetDisplayIndex = 0;
                int minIndex = targetDisplayIndex * displayCount;
                while (minIndex >= count)
                {
                    targetDisplayIndex--;
                    minIndex = targetDisplayIndex * displayCount;
                }

                _displayIndex = targetDisplayIndex;

                bool isFirstPage = minIndex == 0;
                bool isLastPage = false;
                List<int> indexs = new List<int>();
                for (int i = 0; i < displayCount; i++)
                {
                    var displayIndex = minIndex + i + 1;
                    if (displayIndex <= count)
                    {
                        indexs.Add(displayIndex);
                        if (displayIndex == count) isLastPage = true;
                    }
                    else
                    {
                        isLastPage = true;
                        break;
                    }
                }

                DisplayIndexSource = indexs.ToArray();
                if (isFirstPage && isLastPage)
                    DisplayIndexStatus = DisplayIndexStatus.None;
                else if (isFirstPage)
                    DisplayIndexStatus = DisplayIndexStatus.Start;
                else if (isLastPage)
                    DisplayIndexStatus = DisplayIndexStatus.End;
                else
                    DisplayIndexStatus = DisplayIndexStatus.Center;
            }
            else
            {
                DisplayIndexSource = null;
                DisplayIndexStatus = DisplayIndexStatus.None;
            }
        }
    }

    public enum DisplayIndexStatus
    {
        None,
        Start,
        End,
        Center
    }

    [ContentProperty(nameof(Selections))]
    public class EditableDataGridColumn : DataGridColumn
    {
        public static readonly DependencyProperty EditedPropertyNameProperty =
            DependencyProperty.Register(nameof(EditedPropertyName), typeof(string), typeof(EditableDataGridColumn), new PropertyMetadata(null));
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind), typeof(EditorKind), typeof(EditableDataGridColumn), new PropertyMetadata(EditorKind.Text));
        public static readonly DependencyProperty IsDeleteableProperty =
            DependencyProperty.Register(nameof(IsDeleteable), typeof(bool), typeof(EditableDataGridColumn), new PropertyMetadata(false));

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
        public bool IsDeleteable
        {
            get { return (bool)GetValue(IsDeleteableProperty); }
            set { SetValue(IsDeleteableProperty, value); }
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsFrozenProperty)
            {
                if (Header == null)
                {
                    var propertyName = EditedPropertyName;
                    var owner = this.DataGridOwner;
                    if (owner != null && string.IsNullOrEmpty(propertyName) == false)
                    {
                        var source = EditableViewModelAssist.GetSourceService(this.DataGridOwner);
                        Header = source?.GetHeader(EditedPropertyName);
                    }
                }
            }
        }
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
                    var operEditor = new OperEditor();
                    operEditor.SetBinding(OperEditor.IsDeleteableProperty, new Binding(nameof(IsDeleteable)) { Source = this });
                    editor = operEditor;
                    break;
                default:
                    break;
            }

            if (editor != null)
            {
                editor.SetBinding(EditableViewModelEditor.IsReadOnlyProperty, new Binding(nameof(IsReadOnly)) { Source = this });
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
    }
    public enum EditorKind
    {
        Text,
        List,
        Oper,
    }

    #region editor
    public abstract class EditableViewModelEditor : Control
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(EditableViewModel), typeof(EditableViewModelEditor), new PropertyMetadata(null, OnEditableViewModelChanged));
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EditableViewModelEditor), new PropertyMetadata(false, OnIsEditingChanged));
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(EditableViewModelEditor), new PropertyMetadata(false, OnIsEditableChanged));
        public static readonly DependencyProperty IsReadOnlyProperty =
          DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(EditableViewModelEditor), new PropertyMetadata(false));
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
        public EditableViewModelEditor()
        {
            this.SetBinding(IsEditingProperty, new Binding($"{nameof(ViewModel)}.{nameof(EditableViewModel.IsEditing)}") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(IsEditableProperty, new Binding($"{nameof(ViewModel)}.{nameof(EditableViewModel.IsEditable)}") { Source = this, Mode = BindingMode.OneWay });
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
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        protected virtual void OnIsEditingChanged(bool isEditing) { }
        protected virtual void OnIsEditableChanged(bool isEditable) { }
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

                var sourceService = EditableViewModelAssist.GetSourceService(this);
                if (sourceService != null)
                    newViewModel.SourceService = sourceService;
            }
        }
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(EditableViewModel.Source):
                    OnSourceChanged();
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
                var viewModel = ViewModel;
                if (viewModel != null)
                {
                    viewModel.IsEditable = EditableViewModelAssist.GetIsEditable(this);
                }
            }
        }
        protected virtual void OnSourceChanged() { }
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
      
        public bool IsChanged
        {
            get { return (bool)GetValue(IsChangedProperty); }
            set { SetValue(IsChangedProperty, value); }
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == EditedSourcePropertyNameProperty)
            {
                var editedPropertyName = EditedSourcePropertyName;
                _valueBinding = BindingOperations.SetBinding(this, ValueProperty, new Binding($"{nameof(ViewModel)}.{nameof(ViewModel.Source)}.{EditedSourcePropertyName}") { Source = this });
            }
        }
        protected override void OnSourceChanged()
        {
            _valueBinding?.UpdateTarget();
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
        public static readonly DependencyProperty IsDeleteableProperty =
           DependencyProperty.Register(nameof(IsDeleteable), typeof(bool), typeof(OperEditor), new PropertyMetadata(false));

        static OperEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OperEditor), new FrameworkPropertyMetadata(typeof(OperEditor)));
        }
        public OperEditor()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, new ExecutedRoutedEventHandler(OnPen)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new ExecutedRoutedEventHandler(OnClose)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, new ExecutedRoutedEventHandler(OnSave)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, new ExecutedRoutedEventHandler(OnDelete)));
        }
        public bool IsDeleteable
        {
            get { return (bool)GetValue(IsDeleteableProperty); }
            set { SetValue(IsDeleteableProperty, value); }
        }
        private void OnPen(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = ViewModel;
            if (viewModel != null)
            {
                viewModel.IsEditing = true;
            }
        }
        private void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = ViewModel;
            if (viewModel != null)
            {
                viewModel.IsEditing = false;
            }
        }
        private async void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.RaiseSaveAsync();
            }
        }
        private async void OnDelete(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.RaiseDelecteAsync();
            }
        }
    }
    #endregion

}
