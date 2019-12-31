using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace System.Windows
{
    public class EFDataBox : ContentControl
    {
        #region commands
        private static RoutedUICommand _loadCommand;
        public static ICommand LoadCommand
        {
            get
            {
                if (_loadCommand == null)
                {
                    _loadCommand = new RoutedUICommand("load command", nameof(LoadCommand), typeof(EFDataBox));
                    //注册热键
                    //_loadCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _loadCommand;
            }
        }
        private static RoutedUICommand _refreshCommand;
        public static ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RoutedUICommand("refresh command", nameof(RefreshCommand), typeof(EFDataBox));
                    //注册热键
                    //_refreshCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _refreshCommand;
            }
        }
        private static RoutedUICommand _exportCommand;
        public static ICommand ExportCommand
        {
            get
            {
                if (_exportCommand == null)
                {
                    _exportCommand = new RoutedUICommand("export command", nameof(ExportCommand), typeof(EFDataBox));
                    //注册热键
                    _exportCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
                }
                return _exportCommand;
            }
        }
        private static RoutedUICommand _importCommand;
        public static ICommand ImportCommand
        {
            get
            {
                if (_importCommand == null)
                {
                    _importCommand = new RoutedUICommand("import command", nameof(ImportCommand), typeof(EFDataBox));
                    //注册热键
                    //_import.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _importCommand;
            }
        }
        private static RoutedUICommand _editCommand;
        public static ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RoutedUICommand("edit command", nameof(EditCommand), typeof(EFDataBox));
                    //注册热键
                    //_editCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _editCommand;
            }
        }
        private static RoutedUICommand _saveCommand;
        public static ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RoutedUICommand("save command", nameof(SaveCommand), typeof(EFDataBox));
                    //注册热键
                    //_saveCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _saveCommand;
            }
        }
        private static RoutedUICommand _cancelCommand;
        public static ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RoutedUICommand("cancel command", nameof(CancelCommand), typeof(EFDataBox));
                    //注册热键
                    //_cancelCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _cancelCommand;
            }
        }
        private static RoutedUICommand _deleteCommand;
        public static ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RoutedUICommand("delete command", nameof(DeleteCommand), typeof(EFDataBox));
                    //注册热键
                    //_deleteCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _deleteCommand;
            }
        }
        private static RoutedUICommand _addCommand;
        public static ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RoutedUICommand("add command", nameof(AddCommand), typeof(EFDataBox));
                    //注册热键
                    //_addCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _addCommand;
            }
        }

        #endregion

        #region register properties
        public static readonly DependencyProperty DbContextTypeProperty =
           DependencyProperty.Register(nameof(DbContextType), typeof(Type), typeof(EFDataGrid), new PropertyMetadata(default, OnPropertyChanged));
        private static readonly DependencyPropertyKey ActualDbContextTypePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ActualDbContextType), typeof(Type), typeof(EFDataBox), new PropertyMetadata(default, OnPropertyChanged));
        public static readonly DependencyProperty ActualDbContextTypeProperty = ActualDbContextTypePropertyKey.DependencyProperty;
        public static readonly DependencyProperty EntityTypeProperty =
            DependencyProperty.Register(nameof(EntityType), typeof(Type), typeof(EFDataGrid), new PropertyMetadata(default, OnPropertyChanged));
        private static readonly DependencyPropertyKey ActualEntityTypePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ActualEntityType), typeof(Type), typeof(EFDataBox), new PropertyMetadata(default, OnPropertyChanged));
        public static readonly DependencyProperty ActualEntityTypeProperty = ActualEntityTypePropertyKey.DependencyProperty;
        public static readonly DependencyProperty ValidKindProperty =
            DependencyProperty.Register(nameof(ValidKind), typeof(string), typeof(EFDataGrid), new PropertyMetadata(null, OnPropertyChanged));
        private static readonly DependencyPropertyKey DisplayPropertyInfosPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DisplayPropertyInfos), typeof(IEnumerable<EFDisplayPropertyInfo>), typeof(EFDataBox), new PropertyMetadata(null));
        public static readonly DependencyProperty DisplayPropertyInfosProperty = DisplayPropertyInfosPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey DisplayItemsSourcePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DisplayItemsSource), typeof(IEnumerable), typeof(EFDataBox), new PropertyMetadata(null));
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(EFDataBox), new PropertyMetadata(50, null, OnCoereDisplayCount));
        private static readonly DependencyPropertyKey CountPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Count), typeof(int), typeof(EFDataBox), new PropertyMetadata(0));
        public static readonly DependencyProperty CountProperty = CountPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey PageCountPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(PageCount), typeof(int), typeof(EFDataBox), new PropertyMetadata(0));
        public static readonly DependencyProperty PageCountProperty = PageCountPropertyKey.DependencyProperty;
        public static readonly DependencyProperty PageIndexProperty =
            DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(EFDataBox), new PropertyMetadata(0, OnPropertyChanged));
        public static readonly DependencyProperty QueryExpressionProperty =
            DependencyProperty.Register(nameof(QueryExpression), typeof(Linq.Expressions.Expression), typeof(EFDataBox), new PropertyMetadata(null));
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(IEFViewModel), typeof(EFDataBox), new PropertyMetadata(null, OnPropertyChanged));
        public static readonly DependencyProperty DisplayItemsSourceProperty = DisplayItemsSourcePropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey IsLoadingPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsLoading), typeof(bool), typeof(EFDataBox), new PropertyMetadata(false));
        public static readonly DependencyProperty IsLoadingProperty = IsLoadingPropertyKey.DependencyProperty;
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EFDataBox box = (EFDataBox)d;
            if (e.Property == DbContextTypeProperty)
            {
                var dbContextType = (Type)e.NewValue;
                if (dbContextType != null && typeof(DbContext).IsAssignableFrom(dbContextType) == false)
                {
                    throw new Exception("必须是数据库入口类DbContext");
                }
                if (box.ViewModel == null)
                {
                    box.ActualDbContextType = (Type)e.NewValue;
                }
            }
            else if (e.Property == EntityTypeProperty)
            {
                if (box.ViewModel == null)
                {
                    box.ActualEntityType = (Type)e.NewValue;
                }
            }
            else if (e.Property == ActualEntityTypeProperty)
            {
                box.InitializeEntityType();
            }
            else if (e.Property == ViewModelProperty)
            {
                var viewModel = e.NewValue as IEFViewModel;
                if (viewModel == null)
                {
                    box.ActualDbContextType = box.DbContextType;
                    box.ActualEntityType = box.EntityType;
                }
                else
                {
                    box.ActualDbContextType = viewModel.DbContextType;
                    box.ActualEntityType = viewModel.EntityType;
                }
            }
            else if (e.Property == ValidKindProperty)
            {
                box.InitializeDisplayPropertyNames();
            }
            else if (e.Property == PageIndexProperty)
            {
                box.Refresh();
            }
        }
        private static object OnCoereDisplayCount(DependencyObject d, object baseValue)
        {
            if ((int)baseValue <= 0)
                return 50;
            return baseValue;
        }
        static EFDataBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EFDataBox), new FrameworkPropertyMetadata(typeof(EFDataBox)));
        }
        #endregion

        #region fields and constructors
        private readonly object _newKey;
        private ObservableDictionary<object, object> _itemsSource;
        private List<string> _entityNames;
        private string _entityName;
        private Dictionary<string, EFDisplayPropertyInfo> _displayPropertyInfos;
        private Dictionary<string, PropertyInfo> _entitiyPropertys;
        private Dictionary<string, string> _headerToPropertyNames;
        private PropertyInfo _keyPropertyInfo;
        private string _keyName;
        private Func<object, object> _getKey;
        private Expression<Func<DbContext, int>> _queryCountExpression;
        private Expression<Func<DbContext, int, int, IEnumerable<object>>> _queryPageExpression;
        private Expression<Func<DbContext, IEnumerable<object>>> _queryExpression;
        private object _loadingLocker;
        private bool _isLoading;
        public EFDataBox()
        {
            _newKey = $"{nameof(EFDataBox)}_newKey";
            _loadingLocker = new object();
            _isLoading = false;
            _entityNames = new List<string>();
            _displayPropertyInfos = new Dictionary<string, EFDisplayPropertyInfo>();
            _entitiyPropertys = new Dictionary<string, PropertyInfo>();
            _headerToPropertyNames = new Dictionary<string, string>();
            _itemsSource = new ObservableDictionary<object, object>();
            _itemsSource.CollectionChanged += ItemsSource_CollectionChanged;
            EFDataBoxAssist.SetItemsSource(this, _itemsSource);
            DisplayItemsSource = _itemsSource;
            this.SetBinding(EFDataBoxAssist.EntityTypeProperty, new Binding(nameof(ActualEntityType)) { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(EFDataBoxAssist.DisplayPropertyInfosProperty, new Binding(nameof(DisplayPropertyInfos)) { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(EFDataGridBarAssist.CountProperty, new Binding(nameof(Count)) { Source = this, Mode = BindingMode.OneWay });
            this.SetBinding(EFDataGridBarAssist.PageCountProperty, new Binding(nameof(PageCount)) { Source = this, Mode = BindingMode.OneWay });
            this.CommandBindings.Add(new CommandBinding(LoadCommand, new ExecutedRoutedEventHandler(OnLoad)));
            this.CommandBindings.Add(new CommandBinding(RefreshCommand, new ExecutedRoutedEventHandler(OnRefresh)));
            this.CommandBindings.Add(new CommandBinding(ExportCommand, new ExecutedRoutedEventHandler(OnExport)));
            this.CommandBindings.Add(new CommandBinding(ImportCommand, new ExecutedRoutedEventHandler(OnImport)));
            this.CommandBindings.Add(new CommandBinding(EditCommand, new ExecutedRoutedEventHandler(OnEdit)));
            this.CommandBindings.Add(new CommandBinding(CancelCommand, new ExecutedRoutedEventHandler(OnCancel)));
            this.CommandBindings.Add(new CommandBinding(DeleteCommand, new ExecutedRoutedEventHandler(OnDelete)));
            this.CommandBindings.Add(new CommandBinding(SaveCommand, new ExecutedRoutedEventHandler(OnSave)));
            this.CommandBindings.Add(new CommandBinding(AddCommand, new ExecutedRoutedEventHandler(OnAdd)));
        }
        #endregion

        #region properties
        public Type DbContextType
        {
            get { return (Type)GetValue(DbContextTypeProperty); }
            set { SetValue(DbContextTypeProperty, value); }
        }
        public Type ActualDbContextType
        {
            get { return (Type)GetValue(ActualDbContextTypeProperty); }
            protected set { SetValue(ActualDbContextTypePropertyKey, value); }
        }
        public Type EntityType
        {
            get { return (Type)GetValue(EntityTypeProperty); }
            set { SetValue(EntityTypeProperty, value); }
        }
        public Type ActualEntityType
        {
            get { return (Type)GetValue(ActualEntityTypeProperty); }
            protected set { SetValue(ActualEntityTypePropertyKey, value); }
        }
        public string ValidKind
        {
            get { return (string)GetValue(ValidKindProperty); }
            set { SetValue(ValidKindProperty, value); }
        }
        public IEnumerable<EFDisplayPropertyInfo> DisplayPropertyInfos
        {
            get { return (IEnumerable<EFDisplayPropertyInfo>)GetValue(DisplayPropertyInfosProperty); }
            protected set { SetValue(DisplayPropertyInfosPropertyKey, value); }
        }
        public IEnumerable DisplayItemsSource
        {
            get { return (IEnumerable)GetValue(DisplayItemsSourceProperty); }
            protected set { SetValue(DisplayItemsSourcePropertyKey, value); }
        }
        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }
        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            protected set { SetValue(CountPropertyKey, value); }
        }
        public int PageCount
        {
            get { return (int)GetValue(PageCountProperty); }
            protected set { SetValue(PageCountPropertyKey, value); }
        }
        public int PageIndex
        {
            get { return (int)GetValue(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }
        public Linq.Expressions.Expression QueryExpression
        {
            get { return (Linq.Expressions.Expression)GetValue(QueryExpressionProperty); }
            set { SetValue(QueryExpressionProperty, value); }
        }
        public IEFViewModel ViewModel
        {
            get { return (IEFViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            protected set { SetValue(IsLoadingPropertyKey, value); }
        }
        #endregion

        private void ItemsSource_CollectionChanged(object sender, Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var hasAddedItem = _itemsSource.ContainsKey(_newKey);
            EFDataBoxAssist.SetHasAddedItem(this, hasAddedItem);
            if (hasAddedItem == false)
                EFDataBoxAssist.SetAddedItem(this, null);
            else
                EFDataBoxAssist.SetAddedItem(this,_itemsSource[_newKey]);
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualDbContextType = ViewModel?.DbContextType ?? DbContextType;
            ActualEntityType = ViewModel?.EntityType ?? EntityType;
            Load();
        }
        private void InitializeEntityType()
        {
            var type = ActualEntityType;
            _entitiyPropertys.Clear();
            if (type != default)
            {
                foreach (var item in type.GetProperties())
                {
                    _entitiyPropertys.Add(item.Name, item);
                }
            }
            //默认第一个属性为主键
            _keyPropertyInfo = _entitiyPropertys.Values.FirstOrDefault();
            _keyName = _keyPropertyInfo?.Name;
            _getKey = GetKeyFunction(type, _keyName)?.Compile();
            InitializeDisplayPropertyNames();
            _itemsSource?.Clear();
        }
        private void InitializeQueryExpresion()
        {
            var type = ActualEntityType;
            var query = ViewModel?.QueryExpression;
            var query1 = QueryExpression;
            _queryCountExpression = GetQueryCountExpression(type, query, query1);
            _queryPageExpression = GetQueryPageExpression(type, _keyPropertyInfo, query, query1);
            _queryExpression = GetQueryExpression(type, query, query1);
        }
        private void InitializeDisplayPropertyNames()
        {
            _entityNames.Clear();
            _displayPropertyInfos.Clear();
            _headerToPropertyNames.Clear();
            var type = ActualEntityType;
            if (type != default)
            {
                var validKind = ValidKind;
                _entityNames.Add(type.Name);
                GenericNameAttribute entityAttr = null;
                foreach (var att in type.GetCustomAttributes(typeof(GenericNameAttribute)).OfType<GenericNameAttribute>().OrderByDescending(i => i.Kind))
                {
                    var name = att.Name;
                    var kind = att.Kind;
                    if (_entityNames.Contains(name))
                        throw new ArgumentException($"实体模型存在同名的标记![{name}]");
                    _entityNames.Add(name);
                    if (entityAttr == null)
                    {
                        if (kind == validKind)
                            entityAttr = att;
                        else if (string.IsNullOrEmpty(kind))
                            entityAttr = att;
                    }
                }
                _entityName = string.IsNullOrEmpty(entityAttr?.Name) == false ? entityAttr.Name : type.Name;
                ViewModel?.Initialize(_entityName);
                foreach (var ppty in _entitiyPropertys.Values)
                {
                    var propertyName = ppty.Name;
                    _headerToPropertyNames.Add(propertyName, propertyName);
                    GenericNameAttribute displayAttr = null;
                    foreach (var att in ppty.GetCustomAttributes(typeof(GenericNameAttribute)).OfType<GenericNameAttribute>().OrderByDescending(i => i.Kind))
                    {
                        var name = att.Name;
                        var kind = att.Kind;
                        if (_headerToPropertyNames.ContainsKey(name))
                        {
                            var registeredPropertyName = _headerToPropertyNames[name];
                            if (registeredPropertyName != propertyName)
                            {
                                throw new ArgumentException($"属性[{propertyName}]与属性[{registeredPropertyName}]存在同名的标记![{name}]");
                            }
                        }
                        else
                            _headerToPropertyNames.Add(att.Name, propertyName);
                        if (displayAttr == null)
                        {
                            if (kind == validKind)
                                displayAttr = att;
                            else if (string.IsNullOrEmpty(kind))
                                displayAttr = att;
                        }
                    }
                    if (displayAttr != null)
                        _displayPropertyInfos.Add(propertyName, new EFDisplayPropertyInfo(ppty, displayAttr, entityAttr));
                }
            }
            DisplayPropertyInfos = _displayPropertyInfos.Values.ToArray();
        }
        private void OnEdit(object sender, ExecutedRoutedEventArgs e)
        {
            var grid = e.Source as EFDataGrid;
            var item = e.Parameter;
            if (grid != null && item != null)
            {
                try
                {
                    if (ViewModel?.CanEditItem(item) != false)
                    {
                        var row = grid.ItemContainerGenerator.ContainerFromItem(item);
                        EFDataBoxAssist.SetIsRowEditable(row, true);
                    }
                }
                catch (Exception ex)
                {
                    //
                    ViewModel?.OnCatchedException(ex, "编辑异常");
                }
            }
        }
        private void OnCancel(object sender, ExecutedRoutedEventArgs e)
        {
            var grid = e.Source as EFDataGrid;
            var item = e.Parameter;
            if (grid != null && item != null)
            {
                try
                {
                    if (item == EFDataBoxAssist.GetAddedItem(this))
                        _itemsSource.Remove(_newKey);
                    else
                    {
                        var row = grid.ItemContainerGenerator.ContainerFromItem(item);
                        EFDataBoxAssist.SetIsRowEditable(row, false);
                    }
                }
                catch (Exception ex)
                {
                    //
                    ViewModel?.OnCatchedException(ex, "取消异常");
                }
            }
        }
        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            var grid = e.Source as EFDataGrid;
            var item = e.Parameter;
            var viewModel = ViewModel;
            if (grid != null && item != null)
            {
                try
                {
                    var isAddedItem = item == EFDataBoxAssist.GetAddedItem(this);
                    var changedProperties = new List<EFEditedPropertyInfo>();
                    var row = grid.ItemContainerGenerator.ContainerFromItem(item);
                    foreach (var valueEditor in row.FindChildren<EFValueEditor>())
                    {
                        var propertyName = valueEditor.PropertyName;
                        if (valueEditor.IsValueChanged&&_displayPropertyInfos.ContainsKey(propertyName))
                        {
                            var info = _displayPropertyInfos[propertyName];
                            changedProperties.Add(new EFEditedPropertyInfo(ActualEntityType,info,valueEditor.ValidValue));
                        }
                    }

                    if (viewModel?.CanSaveItem(isAddedItem, item, changedProperties) != false)
                    {
                        var dbContextType = ActualDbContextType;
                        if (dbContextType == default || typeof(DbContext).IsAssignableFrom(dbContextType) == false)
                            return;

                        //保存
                        using (var db = (DbContext)Activator.CreateInstance(dbContextType))
                        {
                            db.Entry(item).State = isAddedItem ? EntityState.Added : EntityState.Modified;
                            foreach (var property in changedProperties)
                            {
                                property.SetValue(item);
                            }
                            db.SaveChanges();
                        }

                        if (isAddedItem)
                            _itemsSource.Update(0, _getKey(item), item);

                        EFDataBoxAssist.SetIsRowEditable(row, false);

                        viewModel?.OnSavedItem(isAddedItem, item, changedProperties);
                        viewModel?.OnCatchedMessage("保存成功!");
                    }
                }
                catch (Exception ex)
                {
                    //
                    viewModel?.OnCatchedException(ex, "保存异常");
                }
            }
        }
        private void OnAdd(object sender, ExecutedRoutedEventArgs e)
        {
            var type = ActualEntityType;
            if (type == default) return;
            object addedItem;
            if (_itemsSource.ContainsKey(_newKey) == false)
            {
                addedItem = Activator.CreateInstance(type);
                _itemsSource.Insert(0, _newKey, addedItem);
            }
            else
            {
                addedItem = _itemsSource[_newKey];
                foreach (var item in this.FindChildren<EFDataGrid>())
                {
                    item.ScrollIntoView(addedItem);
                }
            }
        }
        private void OnDelete(object sender, ExecutedRoutedEventArgs e)
        {
            var grid = e.Source as EFDataGrid;
            var item = e.Parameter;
            var viewModel = ViewModel;
            //
            if (grid != null && item != null)
            {
                try
                {
                    if (viewModel?.CanDeleteItem(item) != false)
                    {
                        var dbContextType = ActualDbContextType;
                        if (dbContextType == default || typeof(DbContext).IsAssignableFrom(dbContextType) == false)
                            return;

                        //删除
                        using (var db = (DbContext)Activator.CreateInstance(dbContextType))
                        {
                            db.Entry(item).State = EntityState.Deleted;
                            db.SaveChanges();
                        }

                        Refresh();

                        var row = grid.ItemContainerGenerator.ContainerFromItem(item);
                        EFDataBoxAssist.SetIsRowEditable(row, false);

                        viewModel?.OnDeletedItem(item);
                        viewModel?.OnCatchedMessage("删除成功!");
                    }
                }
                catch (Exception ex)
                {
                    //
                    viewModel?.OnCatchedException(ex, "删除异常");
                }
            }
        }
        private void OnLoad(object sender, ExecutedRoutedEventArgs e)
        {
            Load();
        }
        private void OnRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            Refresh();
        }
        private async void OnExport(object sender, ExecutedRoutedEventArgs e)
        {
            var ret = await ExportWithFileDialog();
            if (ret == true)
                ViewModel?.OnCatchedMessage("导出成功");
            else if (ret == false)
                ViewModel?.OnCatchedMessage("导出失败");
        }
        private async void OnImport(object sender, ExecutedRoutedEventArgs e)
        {
            var result = await ImportWithFileDialog();
            if (result == false)
                ViewModel?.OnCatchedMessage("导入失败!");
            else if (result == true)
            {
                Refresh();
                ViewModel?.OnCatchedMessage("导入成功!");
            }
        }
        private async void Load()
        {
            if (_isLoading) return;
            var dbContextType = ActualDbContextType;
            if (dbContextType == default || typeof(DbContext).IsAssignableFrom(dbContextType) == false)
                return;
            InitializeQueryExpresion();
            if (_queryCountExpression == null || _queryPageExpression == null)
                return;
            lock (_loadingLocker)
            {
                if (_isLoading) return;
                _isLoading = true;
            }

            try
            {
                IsLoading = true;
                var count = await QueryCountAsync(dbContextType);
                var pageSize = PageSize;
                if (pageSize <= 0)
                {
                    pageSize = 50;
                    PageSize = 50;
                }
                int remain;
                var pageCount = Math.DivRem(count, pageSize, out remain);
                if (remain > 0) pageCount++;
                var pageIndex = pageCount > 0 ? 1 : 0;
                Count = count;
                PageCount = pageCount;
                PageIndex = pageIndex;
                await QueryPageAsync(dbContextType, pageSize, pageIndex);             
                IsLoading = false;
            }
            catch (Exception e)
            {
                //
            }
            finally
            {
                _isLoading = false;
            }
        }
        private void Refresh()
        {
            var pageIndex = PageIndex;
            LoadPage(pageIndex);
        }
        private async void LoadPage(int pageIndex)
        {
            if (_isLoading) return;
            var dbContextType = ActualDbContextType;
            if (dbContextType == default || typeof(DbContext).IsAssignableFrom(dbContextType) == false)
                return;
            if (_queryCountExpression == null || _queryPageExpression == null)
                return;
            lock (_loadingLocker)
            {
                if (_isLoading) return;
                _isLoading = true;
            }
            try
            {
                IsLoading = true;
                var pageSize = PageSize;
                if (pageSize <= 0)
                {
                    pageSize = 50;
                    PageSize = 50;
                }
                await QueryPageAsync(dbContextType, pageSize, pageIndex);
                IsLoading = false;
            }
            catch (Exception e)
            {
                //
            }
            finally
            {
                _isLoading = false;
            }
        }
        private Task<int> QueryCountAsync(Type contextType)
        {
            var viewModel = ViewModel;
            return Task.Run(() =>
            {
                try
                {
                    using (var db = (DbContext)Activator.CreateInstance(contextType))
                    {
                        var query = _queryCountExpression?.Compile();
                        return query?.Invoke(db) ?? 0;
                    }
                }
                catch (Exception e)
                {
                    //
                    viewModel?.OnCatchedException(e, "查询数据异常");
                }
                return 0;
            });
        }
        private Task QueryPageAsync(Type contextType, int pageSize, int pageIndex)
        {
            var viewModel = ViewModel;
            return Task.Run(() =>
            {
                try
                {
                    using (var db = (DbContext)Activator.CreateInstance(contextType))
                    {
                        if (pageSize <= 0) pageSize = 1;
                        if (pageIndex <= 0) pageIndex = 1;
                        var query = _queryPageExpression?.Compile();
                        _itemsSource.SetSourceAsync(query?.Invoke(db, pageSize * (pageIndex - 1), pageSize), _getKey);
                    }
                }
                catch (Exception e)
                {
                    //
                    viewModel?.OnCatchedException(e,"刷新数据异常");
                }
            });
        }

        #region linq
        private Expression<Func<object, object>> GetKeyFunction(Type entityType, string keyPropertyName)
        {
            if (entityType == default || string.IsNullOrEmpty(keyPropertyName)) return null;

            //i=>i.Key
            var objParam = Linq.Expressions.Expression.Parameter(typeof(object));
            var model = Linq.Expressions.Expression.Convert(objParam, entityType);
            var getKey = Linq.Expressions.Expression.Property(model, keyPropertyName);
            var objBordy = Linq.Expressions.Expression.Convert(getKey, typeof(object));
            return Linq.Expressions.Expression.Lambda<Func<object, object>>(objBordy, objParam);
        }
        private Expression<Func<DbContext, int>> GetQueryCountExpression(Type entityType, params Linq.Expressions.Expression[] querys)
        {
            if (entityType == default) return null;

            //完整表达式 return db.Set<T>().Where(i => i).Count();   
            var db = Linq.Expressions.Expression.Parameter(typeof(DbContext));
            var dbSet = Linq.Expressions.Expression.Call(db, nameof(DbContext.Set), new Type[] { entityType });//db.Set<T>()
            //
            Linq.Expressions.Expression body = dbSet;
            if (querys != null)
            {
                foreach (var query in querys)
                {
                    if (query != null)
                    {
                        body = Linq.Expressions.Expression.Invoke(query, body);
                    }
                }
            }
            body = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.Count), new Type[] { entityType }, body);
            return Linq.Expressions.Expression.Lambda<Func<DbContext, int>>(body, db);
        }
        private Expression<Func<DbContext, int, int, IEnumerable<object>>> GetQueryPageExpression(Type entityType, PropertyInfo keyPropertyInfo, params Linq.Expressions.Expression[] querys)
        {
            if (entityType == default || keyPropertyInfo == null) return null;

            //完整表达式 return db.Set<T>().Where().OrderBy().Skip().Take();   
            var db = Linq.Expressions.Expression.Parameter(typeof(DbContext));
            var skipCount = Linq.Expressions.Expression.Parameter(typeof(int));
            var takeCount = Linq.Expressions.Expression.Parameter(typeof(int));
            var dbSet = Linq.Expressions.Expression.Call(db, nameof(DbContext.Set), new Type[] { entityType });//db.Set<T>()
            bool hasOrderby = false;
            Linq.Expressions.Expression body = dbSet;
            if (querys != null)
            {
                foreach (var query in querys)
                {
                    if (query != null)
                    {
                        body = Linq.Expressions.Expression.Invoke(query, body);
                    }
                }
                hasOrderby = body.FindMethodCallExpressions(typeof(Queryable), nameof(Queryable.OrderBy)).FirstOrDefault() != null;
            }
            if (hasOrderby == false)
            {
                var model = Linq.Expressions.Expression.Parameter(entityType);
                var getKey = Linq.Expressions.Expression.Property(model, keyPropertyInfo.Name);
                var keyExp = Linq.Expressions.Expression.Lambda(getKey, model);
                body = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.OrderBy),
                   new Type[] { entityType, keyPropertyInfo.PropertyType },
                   body, keyExp);//db.Set<T>().OrderBy(i=>i.Key)
            }
            body = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.Skip), new Type[] { entityType }, body, skipCount);//db.Set<T>().Where().OrderBy(i=>i.Key).Skip();
            body = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.Take), new Type[] { entityType }, body, takeCount);//db.Set<T>().Where().OrderBy(i=>i.Key).Skip().Take();
            return Linq.Expressions.Expression.Lambda<Func<DbContext, int, int, IEnumerable<object>>>(body, db, skipCount, takeCount);
        }
        private Expression<Func<DbContext, IEnumerable<object>>> GetQueryExpression(Type entityType, params Linq.Expressions.Expression[] querys)
        {
            if (entityType == default) return null;

            //完整表达式 return db.Set<T>().Where();   
            var db = Linq.Expressions.Expression.Parameter(typeof(DbContext));
            var dbSet = Linq.Expressions.Expression.Call(db, nameof(DbContext.Set), new Type[] { entityType });//db.Set<T>()
            Linq.Expressions.Expression body = dbSet;
            if (querys != null)
            {
                foreach (var query in querys)
                {
                    if (query != null)
                    {
                        body = Linq.Expressions.Expression.Invoke(query, body);
                    }
                }
            }
            return Linq.Expressions.Expression.Lambda<Func<DbContext, IEnumerable<object>>>(body, db);
        }
        #endregion

        #region Excel 
        /// <summary>
        /// 导出，弹出文件路径选择窗体
        /// </summary>
        /// <param name="defaultFileName"></param>
        /// <param name="tableName"></param>
        /// <returns>null:取消 true:导表成功 false:导表失败</returns>
        protected async Task<bool?> ExportWithFileDialog()
        {
            var dbContextType = ActualDbContextType;
            if (dbContextType == default || typeof(DbContext).IsAssignableFrom(dbContextType) == false)
                return null;
            var qureExpression = _queryExpression;
            if (qureExpression == null)
                return null;

            var displayPropertyInfos = this.DisplayPropertyInfos;
            var viewModel = ViewModel;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = _entityName;
            dialog.Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls";
            if (dialog.ShowDialog() == true)
            {
                var fileName = dialog.FileName;
                var isXls = dialog.FilterIndex == 2;

                return await Task.Run(() =>
                {
                    try
                    {
                        IWorkbook workbook;
                        if (File.Exists(fileName) == false)
                            workbook = isXls ? new HSSFWorkbook() : (IWorkbook)new XSSFWorkbook();
                        else
                        {
                            try
                            {
                                using (var ms = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                                {
                                    workbook = isXls ? new HSSFWorkbook(ms) : (IWorkbook)new XSSFWorkbook(ms);
                                }
                            }
                            catch (Exception e)
                            {
                                viewModel?.OnCatchedException(e, "打开文件异常，请检查文件是否破损或者被占用!");
                                return false;
                            }
                        }

                        //删除表
                        var oldSheet = workbook.GetSheet(_entityName);
                        if (oldSheet != null)
                        {
                            var sheetIndex = workbook.GetSheetIndex(oldSheet);
                            workbook.RemoveSheetAt(sheetIndex);
                        }

                        //创建表
                        var sheet = workbook.CreateSheet(_entityName);
                        var rowIndex = 0;

                        //创建列
                        var columnRow = sheet.CreateRow(rowIndex);
                        var columnIndex = 0;
                        foreach (var item in displayPropertyInfos)
                        {
                            var headerText = item.GenericName;
                            var cell = columnRow.CreateCell(columnIndex);
                            cell.SetCellValue(headerText);
                            columnIndex++;
                        }

                        //表数据   

                        using (var db = (DbContext)Activator.CreateInstance(dbContextType))
                        {
                            foreach (var model in qureExpression.Compile().Invoke(db))
                            {
                                rowIndex++;
                                var row = sheet.CreateRow(rowIndex);
                                var colIndex = 0;
                                foreach (var item in displayPropertyInfos)
                                {
                                    var property = _entitiyPropertys[item.PropertyName];
                                    var propertyType = property.PropertyType;
                                    var propertyValue = property.GetValue(model);
                                    string valueText;
                                    if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                                        valueText = $"{propertyValue:yyyy-MM-dd HH:mm:ss.fff}";
                                    else if (propertyType == typeof(TimeSpan) || propertyType == typeof(TimeSpan?))
                                        valueText = $"{propertyValue:c}";
                                    else
                                        valueText = propertyValue?.ToString();
                                    var cell = row.CreateCell(colIndex);
                                    cell.SetCellValue(valueText);
                                    colIndex++;
                                }
                            }
                        }

                        var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                        workbook.Write(fileStream);
                        fileStream.Close();
                        return true;
                    }
                    catch (Exception e)
                    {
                        viewModel?.OnCatchedException(e, "导表异常");
                        return false;
                    }
                });
            }
            return null;
        }
        /// <summary>
        /// 导入
        /// </summary>
        /// <returns></returns>
        protected async Task<bool?> ImportWithFileDialog()
        {
            var dbContextType = ActualDbContextType;
            var entityType = ActualEntityType;
            if (dbContextType == default || typeof(DbContext).IsAssignableFrom(dbContextType) == false || entityType == default)
                return null;

            var viewModel = ViewModel;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls";
            if (dialog.ShowDialog() == true)
            {
                var fileName = dialog.FileName;
                var isXls = dialog.FilterIndex == 2;
                return await Task.Run(() =>
                {
                    try
                    {
                        IWorkbook workbook;
                        if (File.Exists(fileName) == false)
                        {
                            viewModel?.OnCatchedMessage("文件不存在!");
                            return false;
                        }
                        else
                        {
                            try
                            {
                                using (var ms = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                                {
                                    workbook = isXls ? new HSSFWorkbook(ms) : (IWorkbook)new XSSFWorkbook(ms);
                                }
                            }
                            catch (Exception e)
                            {
                                viewModel?.OnCatchedException(e, "打开文件失败，请检查文件是否破损或者被占用!");
                                return false;
                            }
                        }

                        //获取表
                        ISheet sheet = workbook.GetSheet(_entityName);
                        if (sheet == null)
                            sheet = workbook.GetSheetAt(0);
                        if (sheet == null)
                        {
                            viewModel?.OnCatchedMessage("文件中无数据!");
                            return false;
                        }

                        //读取列
                        var columnRow = sheet.GetRow(sheet.FirstRowNum);
                        var hasKeyColumn = false;
                        var keyColumnName = string.Empty;
                        var keyIndex = -1;
                        var columnIndex = 0;
                        var columns = new List<Tuple<string, string>>();
                        foreach (var item in columnRow)
                        {
                            var columnName = item?.ToString();
                            if (_headerToPropertyNames.ContainsKey(columnName))
                            {
                                var propertyName = _headerToPropertyNames[columnName];
                                if (propertyName == _keyName)
                                {
                                    if (hasKeyColumn == false)
                                    {
                                        hasKeyColumn = true;
                                        keyColumnName = columnName;
                                        keyIndex = columnIndex;
                                    }
                                    else
                                    {
                                        viewModel?.OnCatchedMessage($"存在多个主键列[{keyIndex}:{keyColumnName}][{columnIndex}:{columnName}]");
                                        return false;
                                    }
                                }
                                columns.Add(new Tuple<string, string>(columnName, propertyName));
                            }
                            columnIndex++;
                        }

                        //读取值
                        if (columns.Count == 0)
                        {
                            viewModel?.OnCatchedMessage("文件中未匹配到列!");
                            return false;
                        }
                        var updateItems = new List<object>();
                        var errors = new List<Tuple<int, string, object>>();
                        bool isSkipError = false;
                        using (var db = (DbContext)Activator.CreateInstance(dbContextType))
                        {
                            var dataSet = db.Set(entityType);
                            foreach (IRow item in sheet)
                            {
                                if (item != columnRow)
                                {
                                    object model = null;
                                    bool isNewItem = false;
                                    bool hasError = false;
                                    if (hasKeyColumn)
                                    {
                                        var readedValue = item.GetCell(keyIndex)?.ToString();
                                        object value;
                                        if (this.TryParse(_keyName, readedValue, out value))
                                        {
                                            model = dataSet.Find(value);
                                        }
                                        else if (string.IsNullOrEmpty(readedValue) == false)
                                        {
                                            if (isSkipError || viewModel?.IsImportIgnoreErrorItemsWhenImportedFirstErrorItem(item.RowNum, keyColumnName, readedValue) == true)
                                            {
                                                isSkipError = true;
                                                hasError = true;
                                                errors.Add(new Tuple<int, string, object>(item.RowNum, keyColumnName, readedValue));
                                                continue;
                                            }
                                        }
                                    }
                                    if (model == null)
                                    {
                                        isNewItem = true;
                                        model = Activator.CreateInstance(entityType);
                                    }
                                    for (int i = 0; i < columns.Count; i++)
                                    {
                                        if (isNewItem == false && hasKeyColumn && i == keyIndex)//已存在的列不允许修改主键列
                                            continue;

                                        var readedValue = item.GetCell(i)?.ToString();
                                        var pair = columns[i];
                                        var columnName = pair.Item1;
                                        var propertyName = pair.Item2;
                                        object value;
                                        if (this.TryParse(propertyName, readedValue, out value))
                                        {
                                            _entitiyPropertys[propertyName].SetValue(model, value);
                                        }
                                        else if (isSkipError || viewModel?.IsImportIgnoreErrorItemsWhenImportedFirstErrorItem(item.RowNum, columnName, readedValue) == true)
                                        {
                                            isSkipError = true;
                                            hasError = true;
                                            errors.Add(new Tuple<int, string, object>(item.RowNum, columnName, readedValue));
                                            break;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }

                                    if (hasError)
                                    {
                                        continue;
                                    }
                                    viewModel?.OnImporting(model,isNewItem);
                                    updateItems.Add(model);
                                    if (isNewItem)
                                    {
                                        dataSet.Add(model);
                                    }
                                }
                            }
                            //
                            db.SaveChanges();
                        }
                        //
                        viewModel?.OnImportedCompleted(updateItems, errors);
                        return true;
                    }
                    catch (Exception e)
                    {
                        viewModel?.OnCatchedException(e, "导入数据异常");
                    }

                    return false;
                });
            }
            return null;
        }
        private bool TryParse(string propertyName, object parseValue, out object value)
        {
            value = null;
            try
            {
                var property = _entitiyPropertys[propertyName];
                value = Convert.ChangeType(parseValue, property.PropertyType);
                return true;
            }
            catch { }
            return false;
        }
        #endregion
    }

    public delegate void EFDataBoxAddEventHandler(object sender,EFDataBoxAddEventArgs e);
    public class EFDataBoxAddEventArgs:RoutedEventArgs
    {
        public EFDataBoxAddEventArgs(RoutedEvent routedEvent, object source,object newItem):base(routedEvent,source)
        {
            NewItem = newItem;
        }
        public object NewItem { get; }
    }

    public static class EFDataBoxAssist
    {
        public static readonly DependencyProperty EntityTypeProperty =
                DependencyProperty.RegisterAttached("EntityType", typeof(Type), typeof(EFDataBoxAssist), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty DisplayPropertyInfosProperty =
                DependencyProperty.RegisterAttached("DisplayPropertyInfos", typeof(IEnumerable<EFDisplayPropertyInfo>), typeof(EFDataBoxAssist), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsRowMouseOverProperty =
                DependencyProperty.RegisterAttached("IsRowMouseOver", typeof(bool), typeof(EFDataBoxAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsRowSelectedProperty =
                DependencyProperty.RegisterAttached("IsRowSelected", typeof(bool), typeof(EFDataBoxAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsRowEditableProperty =
                DependencyProperty.RegisterAttached("IsRowEditable", typeof(bool), typeof(EFDataBoxAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty ItemsSourceProperty =
                DependencyProperty.RegisterAttached("ItemsSource", typeof(IEnumerable), typeof(EFDataBoxAssist), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsRowValueChangedProperty =
                DependencyProperty.RegisterAttached("IsRowValueChanged", typeof(bool), typeof(EFDataBoxAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty AddedItemProperty =
                DependencyProperty.RegisterAttached("AddedItem", typeof(object), typeof(EFDataBoxAssist), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty HasAddedItemProperty =
                DependencyProperty.RegisterAttached("HasAddedItem", typeof(bool), typeof(EFDataBoxAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsAddedItemProperty =
                DependencyProperty.RegisterAttached("IsAddedItem", typeof(bool), typeof(EFDataBoxAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static Type GetEntityType(DependencyObject obj)
        {
            return (Type)obj.GetValue(EntityTypeProperty);
        }
        public static void SetEntityType(DependencyObject obj, Type value)
        {
            obj.SetValue(EntityTypeProperty, value);
        }
        public static IEnumerable<EFDisplayPropertyInfo> GetDisplayPropertyInfos(DependencyObject obj)
        {
            return (IEnumerable<EFDisplayPropertyInfo>)obj.GetValue(DisplayPropertyInfosProperty);
        }
        public static void SetDisplayPropertyInfos(DependencyObject obj, IEnumerable<EFDisplayPropertyInfo> value)
        {
            obj.SetValue(DisplayPropertyInfosProperty, value);
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
        public static bool GetIsRowEditable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRowEditableProperty);
        }
        public static void SetIsRowEditable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRowEditableProperty, value);
        }
        public static bool GetIsRowValueChanged(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRowValueChangedProperty);
        }
        public static void SetIsRowValueChanged(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRowValueChangedProperty, value);
        }
        public static IEnumerable GetItemsSource(DependencyObject obj)
        {
            return (IEnumerable)obj.GetValue(ItemsSourceProperty);
        }
        public static void SetItemsSource(DependencyObject obj, IEnumerable value)
        {
            obj.SetValue(ItemsSourceProperty, value);
        }
        public static object GetAddedItem(DependencyObject obj)
        {
            return (object)obj.GetValue(AddedItemProperty);
        }
        public static void SetAddedItem(DependencyObject obj, object value)
        {
            obj.SetValue(AddedItemProperty, value);
        }
        public static bool GetHasAddedItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(HasAddedItemProperty);
        }
        public static void SetHasAddedItem(DependencyObject obj, bool value)
        {
            obj.SetValue(HasAddedItemProperty, value);
        }
        public static bool GetIsAddedItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsAddedItemProperty);
        }
        public static void SetIsAddedItem(DependencyObject obj, bool value)
        {
            obj.SetValue(IsAddedItemProperty, value);
        }
    }

}
