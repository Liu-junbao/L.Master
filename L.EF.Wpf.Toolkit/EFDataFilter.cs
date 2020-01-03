using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static readonly DependencyProperty QueryExpressionProperty =
            DependencyProperty.Register(nameof(QueryExpression), typeof(Linq.Expressions.Expression), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty EntityTypeProperty =
            DependencyProperty.Register(nameof(EntityType), typeof(Type), typeof(EFDataFilter), new PropertyMetadata(default));
        public static readonly DependencyProperty DisplayPropertyInfosProperty =
            DependencyProperty.Register(nameof(DisplayPropertyInfos), typeof(IEnumerable<EFDisplayPropertyInfo>), typeof(EFDataFilter), new PropertyMetadata(null, OnPropertyChanged));
        private static readonly DependencyPropertyKey FiltersPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(PropertyFilters), typeof(IEnumerable), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty PropertyFiltersProperty = FiltersPropertyKey.DependencyProperty;
        public static readonly DependencyProperty SelectedPropertyInfoProperty =
            DependencyProperty.Register(nameof(SelectedPropertyInfo), typeof(EFDisplayPropertyInfo), typeof(EFDataFilter), new PropertyMetadata(default(EFDisplayPropertyInfo), OnPropertyChanged));
        private static readonly DependencyPropertyKey ComparisonSelectionsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ComparisonSelections), typeof(IEnumerable<EFComparison>), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty ComparisonSelectionsProperty = ComparisonSelectionsPropertyKey.DependencyProperty;
        public static readonly DependencyProperty SelectedComparisonProperty =
            DependencyProperty.Register(nameof(SelectedComparison), typeof(EFComparison), typeof(EFDataFilter), new PropertyMetadata(EFComparison.Equal, OnPropertyChanged));
        public static readonly DependencyProperty ComparisonValueProperty =
            DependencyProperty.Register(nameof(ComparisonValue), typeof(object), typeof(EFDataFilter), new PropertyMetadata(null, OnPropertyChanged));
        private static readonly DependencyPropertyKey IsValidComparisonValuePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsValidComparisonValue), typeof(bool), typeof(EFDataFilter), new PropertyMetadata(false));
        public static readonly DependencyProperty IsValidComparisonValueProperty = IsValidComparisonValuePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey ValidComparisonValuePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ValidComparisonValue), typeof(object), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty ValidComparisonValueProperty = ValidComparisonValuePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsExistsItemPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsExistsItem), typeof(bool), typeof(EFDataFilter), new PropertyMetadata(false));
        public static readonly DependencyProperty IsExistsItemProperty = IsExistsItemPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsSelectedPropertyExistsItemPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsSelectedPropertyExistsItem), typeof(bool), typeof(EFDataFilter), new PropertyMetadata(false));
        public static readonly DependencyProperty IsSelectedPropertyExistsItemProperty = IsSelectedPropertyExistsItemPropertyKey.DependencyProperty;
        public static readonly DependencyProperty SelectedOperationProperty =
            DependencyProperty.Register(nameof(SelectedOperation), typeof(EFOperation), typeof(EFDataFilter), new PropertyMetadata(EFOperation.And, OnPropertyChanged));
        private static readonly DependencyPropertyKey OperationSelectionsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(OperationSelections), typeof(IEnumerable<EFOperation>), typeof(EFDataFilter), new PropertyMetadata(null));
        public static readonly DependencyProperty OperationSelectionsProperty = OperationSelectionsPropertyKey.DependencyProperty;

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var filter = (EFDataFilter)d;
            if (e.Property == DisplayPropertyInfosProperty)
            {
                filter.InitalizeDisplayPropertyInfos();
            }
            else if (e.Property == SelectedPropertyInfoProperty)
            {
                filter.OnSelectedPropertyInfoChanged((EFDisplayPropertyInfo)e.OldValue, (EFDisplayPropertyInfo)e.NewValue);
            }
            else if (e.Property == ComparisonValueProperty)
            {
                filter.OnComparisonValueChanged(e.OldValue, e.NewValue);
            }
            else if (e.Property == SelectedComparisonProperty)
            {
                filter.OnSelectedComparisonChanged((EFComparison)e.OldValue, (EFComparison)e.NewValue);
            }
        }
        static EFDataFilter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EFDataFilter), new FrameworkPropertyMetadata(typeof(EFDataFilter)));
        }
        private Dictionary<EFDisplayPropertyInfo, EFDataPropertyFilter> _propertyFilters;
       
        public EFDataFilter()
        {
            _propertyFilters = new Dictionary<EFDisplayPropertyInfo, EFDataPropertyFilter>();
            this.SetBinding(EntityTypeProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.EntityTypeProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(DisplayPropertyInfosProperty, new Binding($"({nameof(EFDataBoxAssist)}.{EFDataBoxAssist.DisplayPropertyInfosProperty.Name})") { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(QueryExpressionProperty, new Binding($"({nameof(EFDataBox.QueryExpression)})") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(EFDataBox), 1), Mode = BindingMode.TwoWay });
            this.CommandBindings.Add(new CommandBinding(AddCommand, new ExecutedRoutedEventHandler(OnAdd)));
            this.CommandBindings.Add(new CommandBinding(DeleteCommand, new ExecutedRoutedEventHandler(OnDelete)));
            this.CommandBindings.Add(new CommandBinding(ClearCommand, new ExecutedRoutedEventHandler(OnClear)));
        }
        public Linq.Expressions.Expression QueryExpression
        {
            get { return (Linq.Expressions.Expression)GetValue(QueryExpressionProperty); }
            set { SetValue(QueryExpressionProperty, value); }
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
        public EFOperation SelectedOperation
        {
            get { return (EFOperation)GetValue(SelectedOperationProperty); }
            set { SetValue(SelectedOperationProperty, value); }
        }
        public IEnumerable<EFOperation> OperationSelections
        {
            get { return (IEnumerable<EFOperation>)GetValue(OperationSelectionsProperty); }
            protected set { SetValue(OperationSelectionsPropertyKey, value); }
        } 
        public bool IsExistsItem
        {
            get { return (bool)GetValue(IsExistsItemProperty); }
            protected set { SetValue(IsExistsItemPropertyKey, value); }
        }
        public bool IsSelectedPropertyExistsItem
        {
            get { return (bool)GetValue(IsSelectedPropertyExistsItemProperty); }
            protected set { SetValue(IsSelectedPropertyExistsItemPropertyKey, value); }
        }
        public IEnumerable PropertyFilters
        {
            get { return (IEnumerable)GetValue(PropertyFiltersProperty); }
            protected set { SetValue(FiltersPropertyKey, value); }
        }
        private void InitalizeDisplayPropertyInfos()
        {
            _propertyFilters.Clear();
            PropertyFilters = _propertyFilters.Values.ToArray();
            SelectedPropertyInfo = DisplayPropertyInfos.FirstOrDefault();
            IsExistsItem = false;
            IsSelectedPropertyExistsItem = false;
            OperationSelections = GetOperations();
            InitializeQueryExpression();
        }
        protected virtual void OnSelectedPropertyInfoChanged(EFDisplayPropertyInfo oldValue, EFDisplayPropertyInfo newValue)
        {
            InitializeSelectedPropertyInfoComparisons();
            IsSelectedPropertyExistsItem = _propertyFilters.ContainsKey(newValue) && _propertyFilters[newValue].IsEmpty == false;
            ResetComparisonValue();
        }

        #region comparison
        private void InitializeSelectedPropertyInfoComparisons()
        {
            var selectedPropertyInfo = SelectedPropertyInfo;
            if (_propertyFilters.ContainsKey(selectedPropertyInfo))
            {
                var filter = _propertyFilters[selectedPropertyInfo];
                ComparisonSelections = GetComparisons(selectedPropertyInfo.PropertyType);
            }
            else
            {
                ComparisonSelections = GetComparisons(selectedPropertyInfo.PropertyType);
            }
            SelectedComparison = ComparisonSelections.FirstOrDefault();
        }
        private IEnumerable<EFOperation> GetOperations()
        {
            yield return EFOperation.And;
            yield return EFOperation.Or;
        }
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

        protected void OnSelectedComparisonChanged(EFComparison oldValue, EFComparison newValue)
        {
            ReCheckComparisonValue();
        }

        #region comparisonValue
        private TextBox _comparisonValueTextBox;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (_comparisonValueTextBox != null)
                _comparisonValueTextBox.TextChanged -= ComparisonValue_TextChanged;
            _comparisonValueTextBox = this.Template.FindName(PART_TextBox, this) as TextBox;
            if (_comparisonValueTextBox != null)
                _comparisonValueTextBox.TextChanged += ComparisonValue_TextChanged;
        }
        public object ComparisonValue
        {
            get { return (object)GetValue(ComparisonValueProperty); }
            set { SetValue(ComparisonValueProperty, value); }
        }
        public bool IsValidComparisonValue
        {
            get { return (bool)GetValue(IsValidComparisonValueProperty); }
            protected set { SetValue(IsValidComparisonValuePropertyKey, value); }
        }
        public object ValidComparisonValue
        {
            get { return (object)GetValue(ValidComparisonValueProperty); }
            protected set { SetValue(ValidComparisonValuePropertyKey, value); }
        }
        private void ComparisonValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckComparisonValue(_comparisonValueTextBox.Text);
        }
        private void OnComparisonValueChanged(object oldValue, object newValue)
        {
            CheckComparisonValue(newValue);
        }
        private void CheckComparisonValue(object value)
        {
            object validValue;
            var isValidValue = TryParseComparisonValue(value, out validValue);
            IsValidComparisonValue = isValidValue;
            ValidComparisonValue = validValue;
        }
        private void ResetComparisonValue()
        {
            ComparisonValue = null;
            if (_comparisonValueTextBox != null)
                _comparisonValueTextBox.Text = null;
            CheckComparisonValue(null);
        }
        private void ReCheckComparisonValue()
        {
            var value = ComparisonValue;
            if (_comparisonValueTextBox != null)
                value = _comparisonValueTextBox.Text;
            CheckComparisonValue(value);
        }
        private bool TryParseComparisonValue(object value, out object validValue)
        {
            validValue = null;
            try
            {
                var propertyType = SelectedPropertyInfo.PropertyType;
                var comparison = SelectedComparison;
                validValue = Convert.ChangeType(value, SelectedPropertyInfo.PropertyType);
                return IsValidValueForComparison(comparison, validValue);
            }
            catch
            {
                Debug.Write("类型转换");
            }
            return false;
        }
        private bool IsValidValueForComparison(EFComparison comparison,object value)
        {
            switch (comparison)
            {
                case EFComparison.Equal:
                    break;
                case EFComparison.NotEqual:
                    break;
                case EFComparison.GreaterThan:
                    break;
                case EFComparison.GreaterThanOrEqual:
                    break;
                case EFComparison.LessThan:
                    break;
                case EFComparison.LessThanOrEqual:
                    break;
                case EFComparison.Contains:
                    return string.IsNullOrEmpty(value?.ToString()) == false;
                case EFComparison.NotContains:
                    return string.IsNullOrEmpty(value?.ToString()) == false;
                case EFComparison.StartWith:
                    return string.IsNullOrEmpty(value?.ToString()) == false;
                case EFComparison.EndWith:
                    return string.IsNullOrEmpty(value?.ToString()) == false;
                default:
                    break;
            }
            return true;
        }
        #endregion

        private void OnAdd(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsValidComparisonValue == false) return;
            var comparison = SelectedComparison;
            var comparisonValue = ValidComparisonValue;
            var operation = SelectedOperation;
            var selectedPropertyInfo = SelectedPropertyInfo;
            if (DisplayPropertyInfos.Contains(selectedPropertyInfo) == false)
                return;
            EFDataPropertyFilter filter;
            if (_propertyFilters.ContainsKey(selectedPropertyInfo))
            {
                filter = _propertyFilters[selectedPropertyInfo];
            }
            else
            {
                filter = PreparePropertyItemFrom(selectedPropertyInfo) ?? throw new Exception();
                _propertyFilters.Add(selectedPropertyInfo, filter);
            }
            if (filter.AddLast(operation, comparison, comparisonValue))
            {
                PropertyFilters = _propertyFilters.Values.ToArray();
                IsExistsItem = true;
                IsSelectedPropertyExistsItem = true;
                InitializeSelectedPropertyInfoComparisons();
                InitializeQueryExpression();
            }
        }
        private void OnDelete(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedPropertyInfo = SelectedPropertyInfo;
            if (DisplayPropertyInfos.Contains(selectedPropertyInfo) == false)
                return;
            if (_propertyFilters.ContainsKey(selectedPropertyInfo) == false)
                return;
            var filter = _propertyFilters[selectedPropertyInfo];
            filter.RemoveLast();
            if (filter.IsEmpty)
                _propertyFilters.Remove(selectedPropertyInfo);
            PropertyFilters = _propertyFilters.Values.ToArray();
            IsExistsItem = _propertyFilters.Count > 0;
            IsSelectedPropertyExistsItem = filter.IsEmpty == false;
            InitializeSelectedPropertyInfoComparisons();
            InitializeQueryExpression();
        }
        private void OnClear(object sender, ExecutedRoutedEventArgs e)
        {
            _propertyFilters.Clear();
            PropertyFilters = _propertyFilters.Values.ToArray();
            IsExistsItem = false;
            IsSelectedPropertyExistsItem = false;
            InitializeSelectedPropertyInfoComparisons();
            InitializeQueryExpression();
        }
        protected virtual EFDataPropertyFilter PreparePropertyItemFrom(EFDisplayPropertyInfo propertyInfo)
        {
            return new EFDataPropertyFilter(propertyInfo);
        }

        #region queryExpression
        private void InitializeQueryExpression()
        {
            QueryExpression = GetQueryExpression(EntityType, _propertyFilters.Values);
        }
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
            DependencyProperty.RegisterReadOnly(nameof(GenericName), typeof(string), typeof(EFDataPropertyFilter), new PropertyMetadata(null));
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
           DependencyProperty.RegisterReadOnly(nameof(IsEmpty), typeof(bool), typeof(EFDataPropertyFilter), new PropertyMetadata(true));
        public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;
        static EFDataPropertyFilter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EFDataPropertyFilter), new FrameworkPropertyMetadata(typeof(EFDataPropertyFilter)));
        }
        private LinkedList<EFValueFilter> _filters;
        public EFDataPropertyFilter()
        {
            _filters = new LinkedList<EFValueFilter>();
            Filters = _filters.AsEnumerable();
        }
        public EFDataPropertyFilter(EFDisplayPropertyInfo propertyInfo):this()
        {
            PropertyInfo = propertyInfo;
            PropertyName = propertyInfo.PropertyName;
            GenericName = propertyInfo.GenericName;
            PropertyType = propertyInfo.PropertyType;
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
        public bool AddLast(EFOperation operation, EFComparison comparison, object comparisonValue)
        {
            if (_filters.Count == 0)
                _filters.AddLast(new EFValueFilter(comparison, comparisonValue, null));
            else
                _filters.AddLast(new EFValueFilter(comparison, comparisonValue, operation));
            Filters = _filters.ToArray();
            IsEmpty = false;
            return true;
        }
        public void RemoveLast()
        {
            if (_filters.Count > 0)
            {
                _filters.RemoveLast();
            }
            Filters = _filters.ToArray();
            IsEmpty = _filters.Count == 0;
        }
        public void Clear()
        {
            _filters.Clear();
            Filters = _filters.ToArray();
            IsEmpty = true;
        }
        public Linq.Expressions.Expression GetConditionExpression(ParameterExpression modelExp)
        {
            if (_filters.Count == 0)
                return null;
            var propertyType = PropertyType;
            var propertyName = PropertyName;
            var first = _filters.First;
            var value = first.Value;
            var body = GetExpression(modelExp, propertyType, propertyName, value.Comparison, value.ComparisonValue);
            var next = first.Next;
            while (next != null)
            {
                value = next.Value;
                switch (value.Operation)
                {
                    case EFOperation.And:
                        body = Linq.Expressions.Expression.AndAlso(body, GetExpression(modelExp, propertyType, propertyName, value.Comparison, value.ComparisonValue));//i.Key>0 && i.Key<1
                        break;
                    case EFOperation.Or:
                        body = Linq.Expressions.Expression.OrElse(body, GetExpression(modelExp, propertyType, propertyName, value.Comparison, value.ComparisonValue));//i.Key>0 || i.Key<1
                        break;
                    default:
                        break;
                }

                next = next.Next;
            }
            return body;
        }
        private Linq.Expressions.Expression GetExpression(ParameterExpression modelExp,Type propertyType,string propertyName,EFComparison comparison,object comparisonValue)
        {
            var propertyExp = Linq.Expressions.Expression.Property(modelExp, propertyName);
            var valueExp = Linq.Expressions.Expression.Constant(comparisonValue);
            var trueExp = Linq.Expressions.Expression.Constant(true);
            switch (comparison)
            {
                case EFComparison.Equal:
                    if (propertyType == typeof(string) && string.IsNullOrEmpty(comparisonValue?.ToString()))
                        return Linq.Expressions.Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, propertyExp);// string.IsNullOrEmpty(i.PropertyName)
                    else
                        return Linq.Expressions.Expression.Equal(propertyExp, valueExp);//i.PropertyName = value;
                case EFComparison.NotEqual:
                    if (propertyType == typeof(string) && string.IsNullOrEmpty(comparisonValue?.ToString()))// string.IsNullOrEmpty(i.PropertyName)==false
                    {
                        var isNullExp = Linq.Expressions.Expression.Call(typeof(string), nameof(string.IsNullOrEmpty), null, propertyExp);// string.IsNullOrEmpty(i.PropertyName)
                        return Linq.Expressions.Expression.NotEqual(isNullExp, trueExp);//string.IsNullOrEmpty(i.PropertyName) != true
                    }
                    else
                        return Linq.Expressions.Expression.NotEqual(propertyExp, valueExp);//i.PropertyName != value;
                case EFComparison.GreaterThan:
                    return Linq.Expressions.Expression.GreaterThan(propertyExp, valueExp);//i.PropertyName > value;
                case EFComparison.GreaterThanOrEqual:
                    return Linq.Expressions.Expression.GreaterThanOrEqual(propertyExp, valueExp);//i.PropertyName >= value;
                case EFComparison.LessThan:
                    return Linq.Expressions.Expression.LessThan(propertyExp, valueExp);//i.PropertyName < value;
                case EFComparison.LessThanOrEqual:
                    return Linq.Expressions.Expression.LessThanOrEqual(propertyExp, valueExp);//i.PropertyName <= value;
                case EFComparison.Contains:
                    return Linq.Expressions.Expression.Call(propertyExp, nameof(string.Contains), null, valueExp);// i.PropertyName.Contains(value);
                case EFComparison.NotContains:
                    var containsExp = Linq.Expressions.Expression.Call(propertyExp, nameof(string.Contains), null, valueExp);
                    return Linq.Expressions.Expression.NotEqual(containsExp, trueExp);//i.PropertyName.Contains(value) != true
                case EFComparison.StartWith:
                    return Linq.Expressions.Expression.Call(propertyExp, nameof(string.StartsWith), null, valueExp);// i.PropertyName.StartsWith(value);
                case EFComparison.EndWith:
                    return Linq.Expressions.Expression.Call(propertyExp, nameof(string.EndsWith), null, valueExp);// i.PropertyName.EndsWith(value);
                default:
                    break;
            }
            return null;
        }
    }

}
