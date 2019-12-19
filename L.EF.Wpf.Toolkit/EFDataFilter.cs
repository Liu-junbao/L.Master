using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace System.Windows
{
    public class EFDataFilter : Control
    {
        public const string PART_TextBox = nameof(PART_TextBox);

        #region  commands
        private static RoutedUICommand _addCommand;
        public static ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RoutedUICommand("add command", nameof(AddCommand), typeof(EFDataFilter));
                    //注册热键
                    //_addCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _addCommand;
            }
        }
        private static RoutedUICommand _deleteCommand;
        public static ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RoutedUICommand("delete command", nameof(DeleteCommand), typeof(EFDataFilter));
                    //注册热键
                    //_deleteCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _deleteCommand;
            }
        }

        private static RoutedUICommand _clearCommand;
        public static ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                {
                    _clearCommand = new RoutedUICommand("clear command", nameof(ClearCommand), typeof(EFDataFilter));
                    //注册热键
                    //_clearCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _clearCommand;
            }
        }
        #endregion

        private static readonly DependencyPropertyKey QueryExpressionPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(QueryExpression), typeof(Linq.Expressions.Expression), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty EntityTypeProperty =
            DependencyProperty.Register(nameof(EntityType), typeof(Type), typeof(EFDataFilter), new PropertyMetadata(default));
        public static readonly DependencyProperty DisplayPropertyInfosProperty =
            DependencyProperty.Register(nameof(DisplayPropertyInfos), typeof(IEnumerable<EFDisplayPropertyInfo>), typeof(EFDataFilter), new PropertyMetadata(null, OnPropertyChanged));
        private static readonly DependencyPropertyKey PropertyInfoSelectionsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DisplayPropertyInfoSelections), typeof(IEnumerable<EFDisplayPropertyInfo>), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty DisplayPropertyInfoSelectionsProperty = PropertyInfoSelectionsPropertyKey.DependencyProperty;
        public static readonly DependencyProperty QueryExpressionProperty = QueryExpressionPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey FiltersPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(PropertyFilters), typeof(IEnumerable), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty PropertyFiltersProperty = FiltersPropertyKey.DependencyProperty;
        public static readonly DependencyProperty SelectedPropertyInfoProperty =
            DependencyProperty.Register(nameof(SelectedPropertyInfo), typeof(EFDisplayPropertyInfo), typeof(EFDataFilter), new PropertyMetadata(null, OnPropertyChanged));
        private static readonly DependencyPropertyKey ComparisonSelectionsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ComparisonSelections), typeof(IEnumerable<EFComparison>), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty ComparisonSelectionsProperty = ComparisonSelectionsPropertyKey.DependencyProperty;
        public static readonly DependencyProperty SelectedComparisonProperty =
            DependencyProperty.Register(nameof(SelectedComparison), typeof(EFComparison), typeof(EFDataFilter), new PropertyMetadata(EFComparison.Equal));
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(EFDataFilter), new PropertyMetadata(null, OnPropertyChanged));
        private static readonly DependencyPropertyKey IsValidValuePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsValidValue), typeof(bool), typeof(EFDataFilter), new PropertyMetadata(false));
        public static readonly DependencyProperty IsValidValueProperty = IsValidValuePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey ValidValuePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ValidValue), typeof(object), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty ValidValueProperty = ValidValuePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsExistsItemPropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(IsExistsItem), typeof(bool), typeof(EFDataFilter), new PropertyMetadata(false));
        public static readonly DependencyProperty IsExistsItemProperty = IsExistsItemPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsSelectedExistsItemPropertyKey =
          DependencyProperty.RegisterReadOnly(nameof(IsSelectedExistsItem), typeof(bool), typeof(EFDataFilter), new PropertyMetadata(false));
        public static readonly DependencyProperty IsSelectedExistsItemProperty = IsSelectedExistsItemPropertyKey.DependencyProperty;

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var filter = (EFDataFilter)d;
            if (e.Property == DisplayPropertyInfosProperty)
            {
                filter.Invalidate();
            }
            else if (e.Property == SelectedPropertyInfoProperty)
            {

            }
        }
        static EFDataFilter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EFDataFilter), new FrameworkPropertyMetadata(typeof(EFDataFilter)));
        }
        private Dictionary<EFDisplayPropertyInfo, EFDataPropertyFilter> _propertyFilters;
        private TextBox _valueTextBox;
        public EFDataFilter()
        {
            _propertyFilters = new Dictionary<EFDisplayPropertyInfo, EFDataPropertyFilter>();
            this.SetBinding(EntityTypeProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.EntityTypeProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(DisplayPropertyInfosProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.DisplayPropertyInfosProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(QueryExpressionProperty, new Binding(nameof(EFDataBox.QueryExpression)) { Source = this, Mode = BindingMode.TwoWay });
            this.CommandBindings.Add(new CommandBinding(AddCommand, new ExecutedRoutedEventHandler(OnAdd)));
            this.CommandBindings.Add(new CommandBinding(DeleteCommand, new ExecutedRoutedEventHandler(OnDelete)));
            this.CommandBindings.Add(new CommandBinding(ClearCommand, new ExecutedRoutedEventHandler(OnClear)));
        }
        public Linq.Expressions.Expression QueryExpression
        {
            get { return (Linq.Expressions.Expression)GetValue(QueryExpressionProperty); }
            protected set { SetValue(QueryExpressionPropertyKey, value); }
        }
        public Type EntityType
        {
            get { return (Type)GetValue(EntityTypeProperty); }
            set { SetValue(EntityTypeProperty, value); }
        }
        public IEnumerable<EFDisplayPropertyInfo> DisplayPropertyInfos
        {
            get { return (IEnumerable<EFDisplayPropertyInfo>)GetValue(DisplayPropertyInfosProperty); }
            set { SetValue(DisplayPropertyInfosProperty, value); }
        }
        public IEnumerable<EFDisplayPropertyInfo> DisplayPropertyInfoSelections
        {
            get { return (IEnumerable<EFDisplayPropertyInfo>)GetValue(DisplayPropertyInfoSelectionsProperty); }
            protected set { SetValue(PropertyInfoSelectionsPropertyKey, value); }
        }
        public EFDisplayPropertyInfo SelectedPropertyInfo
        {
            get { return (EFDisplayPropertyInfo)GetValue(SelectedPropertyInfoProperty); }
            set { SetValue(SelectedPropertyInfoProperty, value); }
        }
        public IEnumerable<EFComparison> ComparisonSelections
        {
            get { return (IEnumerable<EFComparison>)GetValue(ComparisonSelectionsProperty); }
            protected set { SetValue(ComparisonSelectionsPropertyKey, value); }
        }
        public EFComparison SelectedComparison
        {
            get { return (EFComparison)GetValue(SelectedComparisonProperty); }
            set { SetValue(SelectedComparisonProperty, value); }
        }
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public bool IsValidValue
        {
            get { return (bool)GetValue(IsValidValueProperty); }
            protected set { SetValue(IsValidValuePropertyKey, value); }
        }
        public object ValidValue
        {
            get { return (object)GetValue(ValidValueProperty); }
            protected set { SetValue(ValidValuePropertyKey, value); }
        }
        public bool IsExistsItem
        {
            get { return (bool)GetValue(IsExistsItemProperty); }
            protected set { SetValue(IsExistsItemPropertyKey, value); }
        }
        public bool IsSelectedExistsItem
        {
            get { return (bool)GetValue(IsSelectedExistsItemProperty); }
            protected set { SetValue(IsSelectedExistsItemPropertyKey, value); }
        }
        public IEnumerable PropertyFilters
        {
            get { return (IEnumerable)GetValue(PropertyFiltersProperty); }
            protected set { SetValue(FiltersPropertyKey, value); }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (_valueTextBox != null)
                _valueTextBox.TextChanged -= ValueTextBox_TextChanged;
            _valueTextBox = this.Template.FindName(PART_TextBox, this) as TextBox;
            if (_valueTextBox != null)
                _valueTextBox.TextChanged += ValueTextBox_TextChanged;
        }
        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var newValue = _valueTextBox?.Text;
            object validValue;
            var isValidValue = TryParse(newValue, out validValue);
            IsValidValue = isValidValue;
            ValidValue = validValue;
        }
        private bool TryParse(object value, out object validValue)
        {
            validValue = null;
            try
            {
                validValue = Convert.ChangeType(value, SelectedPropertyInfo.PropertyType);
                return true;
            }
            catch { }
            return false;
        }

        private void Invalidate()
        {
            _propertyFilters.Clear();
            PropertyFilters = _propertyFilters.Values;
            DisplayPropertyInfoSelections = DisplayPropertyInfos;
            SelectedPropertyInfo = DisplayPropertyInfos.FirstOrDefault();
            InitializeQueryExpression();
        }
        private void OnSelectedPropertyInfoChanged(EFDisplayPropertyInfo oldValue, EFDisplayPropertyInfo newValue)
        {
            InitializeSelectedPropertyInfoComparisons();
        }
        private void OnAdd(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedPropertyInfo = SelectedPropertyInfo;
            if (_propertyFilters.ContainsKey(selectedPropertyInfo) || DisplayPropertyInfos.Contains(selectedPropertyInfo) == false)
                return;
            var filter = PreparePropertyItemFrom(selectedPropertyInfo) ?? throw new Exception();
            _propertyFilters.Add(selectedPropertyInfo, filter);
            PropertyFilters = _propertyFilters.Values;
            DisplayPropertyInfoSelections = DisplayPropertyInfos.Where(i => _propertyFilters.ContainsKey(i) == false);
            InitializeSelectedPropertyInfoComparisons();
        }
        private void OnDelete(object sender, ExecutedRoutedEventArgs e)
        {
            var propertyFilter = e.Source as EFDataPropertyFilter;
            if (propertyFilter != null)
            {
                _propertyFilters.Remove(propertyFilter.PropertyInfo);
                PropertyFilters = _propertyFilters.Values;
                DisplayPropertyInfoSelections = DisplayPropertyInfos.Where(i => _propertyFilters.ContainsKey(i) == false);
                InitializeQueryExpression();
            }
        }
        private void OnClear(object sender, ExecutedRoutedEventArgs e)
        {
            _propertyFilters.Clear();
            PropertyFilters = _propertyFilters.Values;
            DisplayPropertyInfoSelections = DisplayPropertyInfos.Where(i => _propertyFilters.ContainsKey(i) == false);
            InitializeQueryExpression();
        }
        private void InitializeQueryExpression()
        {
            QueryExpression = GetQueryExpression(EntityType, _propertyFilters.Values);
        }
        private void InitializeSelectedPropertyInfoComparisons()
        {
            var info = SelectedPropertyInfo;
            if (_propertyFilters.ContainsKey(info))
            {
                var filter = _propertyFilters[info];
                var exists = filter.Filters.Select(i => i.Comparison).ToList();
                ComparisonSelections = GetComparisons(info.PropertyType).Where(i => exists.Contains(i) == false);
            }
            else
            {
                ComparisonSelections = GetComparisons(info.PropertyType);
            }
        }
        protected virtual EFDataPropertyFilter PreparePropertyItemFrom(EFDisplayPropertyInfo propertyInfo)
        {
            return new EFDataPropertyFilter(propertyInfo);
        }
    

        #region comparison
        private IEnumerable<EFComparison> GetComparisons(Type type)
        {
            yield return EFComparison.Equal;
            yield return EFComparison.NotEqual;

            if (type == typeof(bool) || type == typeof(bool?))
            {
                yield break;
            }
            else if (type == typeof(string))
            {
                yield return EFComparison.Contains;
                yield return EFComparison.NotContains;
                yield return EFComparison.StartWith;
                yield return EFComparison.EndWith;
                yield return EFComparison.Match;
            }
            else if (typeof(IComparable).IsAssignableFrom(type))
            {
                yield return EFComparison.GreaterThan;
                yield return EFComparison.GreaterThanOrEqual;
                yield return EFComparison.LessThan;
                yield return EFComparison.LessThanOrEqual;
                yield break;
            }
        }
        #endregion

        #region linq
        private Linq.Expressions.Expression GetQueryExpression(Type entityType, ICollection<EFDataPropertyFilter> propertyFilters)
        {
            if (entityType == default || propertyFilters == default || propertyFilters.Count == 0) return null;

            //表达式 IQueryable<EntityType> query => query.Where(Condition)
            var queryExp = Linq.Expressions.Expression.Parameter(typeof(IQueryable<>).MakeGenericType(entityType));
            var modelExp = Linq.Expressions.Expression.Parameter(entityType);
            Linq.Expressions.Expression conditionExp = null;
            foreach (var item in propertyFilters)
            {
                if (conditionExp == null)
                    conditionExp = item.GetConditionExpression(modelExp);
                else
                {
                    var expression = item.GetConditionExpression(modelExp);
                    if (expression != null)
                        conditionExp = Linq.Expressions.Expression.AndAlso(conditionExp, expression);
                }
            }

            if (conditionExp == null) return null;
            var whereExp = Linq.Expressions.Expression.Lambda(conditionExp, modelExp);
            var body = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.Where), new Type[] { entityType }, queryExp, whereExp);
            return Linq.Expressions.Expression.Lambda(body, queryExp);
        }
        #endregion
    }

    public class EFDataPropertyFilter : Control
    {
        #region commands
        private static RoutedUICommand _addCommand;
        public static ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RoutedUICommand("add command", nameof(AddCommand), typeof(EFDataPropertyFilter));
                    //注册热键
                    //_addCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _addCommand;
            }
        }
        private static RoutedUICommand _cancelCommand;
        public static ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RoutedUICommand("cancel commmand", nameof(CancelCommand), typeof(EFDataPropertyFilter));
                    //注册热键
                    //_cancelCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _cancelCommand;
            }
        }
        #endregion

        private static readonly DependencyPropertyKey PropertyNamePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(PropertyName), typeof(string), typeof(EFDataPropertyFilter), new PropertyMetadata(null));
        private static readonly DependencyPropertyKey GenericNamePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(GenericName), typeof(string), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty GenericNameProperty = GenericNamePropertyKey.DependencyProperty;
        public static readonly DependencyProperty PropertyNameProperty = PropertyNamePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey PropertyTypePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(PropertyType), typeof(Type), typeof(EFDataPropertyFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty PropertyTypeProperty = PropertyTypePropertyKey.DependencyProperty;
        public static readonly DependencyProperty OperationProperty =
            DependencyProperty.Register(nameof(Operation), typeof(EFOperation), typeof(EFDataPropertyFilter), new PropertyMetadata(EFOperation.And));
        private static readonly DependencyPropertyKey FiltersPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Filters), typeof(IEnumerable<EFValueFilter>), typeof(EFDataPropertyFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty FiltersProperty = FiltersPropertyKey.DependencyProperty;
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(EFDataPropertyFilter), new PropertyMetadata(false));
        private static readonly DependencyPropertyKey IsEmptyPropertyKey =
           DependencyProperty.RegisterReadOnly(nameof(IsEmpty), typeof(bool), typeof(EFDataFilter), new PropertyMetadata(true));
        public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;
        static EFDataPropertyFilter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EFDataPropertyFilter), new FrameworkPropertyMetadata(typeof(EFDataPropertyFilter)));
        }

        private LinkedList<EFValueFilter> _filters;
        public EFDataPropertyFilter()
        {
            _filters = new LinkedList<EFValueFilter>();
        }
        public EFDataPropertyFilter(EFDisplayPropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            PropertyName = propertyInfo.PropertyName;
            GenericName = propertyInfo.GenericName;
            PropertyType = propertyInfo.PropertyType;
            _filters = new LinkedList<EFValueFilter>();
        }
        public EFDisplayPropertyInfo PropertyInfo { get; }
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            protected set { SetValue(PropertyNamePropertyKey, value); }
        }
        public string GenericName
        {
            get { return (string)GetValue(GenericNameProperty); }
            protected set { SetValue(GenericNamePropertyKey, value); }
        }
        public Type PropertyType
        {
            get { return (Type)GetValue(PropertyTypeProperty); }
            protected set { SetValue(PropertyTypePropertyKey, value); }
        }
        public EFOperation Operation
        {
            get { return (EFOperation)GetValue(OperationProperty); }
            set { SetValue(OperationProperty, value); }
        }
        public IEnumerable<EFValueFilter> Filters
        {
            get { return (IEnumerable<EFValueFilter>)GetValue(FiltersProperty); }
            protected set { SetValue(FiltersPropertyKey, value); }
        }
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public bool IsEmpty
        {
            get { return (bool)GetValue(IsEmptyProperty); }
            protected set { SetValue(IsEmptyPropertyKey, value); }
        }
        public void AddLast(EFValueFilter filter)
        {
            _filters.AddLast(filter);
            Filters = _filters.AsEnumerable();
            IsEmpty = false;
        }
        public void RemoveLast()
        {
            if (_filters.Count > 0)
                _filters.RemoveLast();
            Filters = _filters.AsEnumerable();
            IsEmpty = _filters.Count == 0;
        }
        public void Clear()
        {
            _filters.Clear();
            Filters = _filters.AsEnumerable();
            IsEmpty = true;
        }
        public Linq.Expressions.Expression GetConditionExpression(ParameterExpression modelExp)
        {
            return null;
        }
    }

}
