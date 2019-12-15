using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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
        #endregion

        public static readonly DependencyProperty DbContextTypeProperty =
            DependencyProperty.Register(nameof(DbContextType), typeof(Type), typeof(EFDataGrid), new PropertyMetadata(default, OnPropertyChanged));
        public static readonly DependencyProperty EntityTypeProperty =
            DependencyProperty.Register(nameof(EntityType), typeof(Type), typeof(EFDataGrid), new PropertyMetadata(default, OnPropertyChanged));
        public static readonly DependencyProperty ValidKindProperty =
            DependencyProperty.Register(nameof(ValidKind), typeof(string), typeof(EFDataGrid), new PropertyMetadata(null, OnPropertyChanged));
        private static readonly DependencyPropertyKey EntityDisplayPropertyNamesPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(EntityDisplayPropertyNames), typeof(IEnumerable<DisplayPropertyInfo>), typeof(EFDataBox), new PropertyMetadata(null));
        public static readonly DependencyProperty EntityDisplayPropertyNamesProperty = EntityDisplayPropertyNamesPropertyKey.DependencyProperty;
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
            DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(EFDataBox), new PropertyMetadata(0,OnPropertyChanged));
        public static readonly DependencyProperty QueryExpressionProperty =
            DependencyProperty.Register(nameof(QueryExpression), typeof(Linq.Expressions.Expression), typeof(EFDataBox), new PropertyMetadata(null));
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
                if (dbContextType == null || typeof(DbContext).IsAssignableFrom(dbContextType) == false)
                {
                    throw new Exception("必须是数据库入口类DbContext");
                }
            }
            else if (e.Property == EntityTypeProperty)
            {
                box.InitializeEntityType();
            }
            else if (e.Property == QueryExpressionProperty)
            {
                box.InitializeQueryExpresion();
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
        private ObservableDictionary<object, object> _itemsSource;
        private Dictionary<string, PropertyInfo> _entitiyPropertys;
        private Dictionary<string, string> _headerToPropertyNames;
        private PropertyInfo _keyPropertyInfo;
        private string _keyName;
        private Func<object, object> _getKey;
        private Expression<Func<DbContext, int>> _queryCountExpression;
        private Expression<Func<DbContext, int, int, IEnumerable<object>>> _queryPageExpression;
        private object _loadingLocker;
        private bool _isLoading;
        public EFDataBox()
        {
            _loadingLocker = new object();
            _isLoading = false;
            _entitiyPropertys = new Dictionary<string, PropertyInfo>();
            _headerToPropertyNames = new Dictionary<string, string>();
            _itemsSource = new ObservableDictionary<object, object>();
            EFDataGridAssist.SetItemsSource(this, _itemsSource);
            DisplayItemsSource = _itemsSource;
            this.SetBinding(EFDataGridAssist.DisplayPropertyNamesProperty, new Binding(nameof(EntityDisplayPropertyNames)) { Source = this, Mode = BindingMode.OneWay });
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
            this.Loaded += EFDataBox_Loaded;
        }
        public Type DbContextType
        {
            get { return (Type)GetValue(DbContextTypeProperty); }
            set { SetValue(DbContextTypeProperty, value); }
        }
        public Type EntityType
        {
            get { return (Type)GetValue(EntityTypeProperty); }
            set { SetValue(EntityTypeProperty, value); }
        }
        public string ValidKind
        {
            get { return (string)GetValue(ValidKindProperty); }
            set { SetValue(ValidKindProperty, value); }
        }
        public IEnumerable<DisplayPropertyInfo> EntityDisplayPropertyNames
        {
            get { return (IEnumerable<DisplayPropertyInfo>)GetValue(EntityDisplayPropertyNamesProperty); }
            protected set { SetValue(EntityDisplayPropertyNamesPropertyKey, value); }
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
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            protected set { SetValue(IsLoadingPropertyKey, value); }
        }
        private void EFDataBox_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitializeEntityType();
        }
        private void InitializeEntityType()
        {
            var type = EntityType;
            _entitiyPropertys.Clear();
            foreach (var item in type.GetProperties())
            {
                _entitiyPropertys.Add(item.Name, item);
            }
            //默认第一个属性为主键
            _keyPropertyInfo = _entitiyPropertys.Values.FirstOrDefault();
            _keyName = _keyPropertyInfo?.Name;
            _getKey = GetKeyFunction(type, _keyPropertyInfo)?.Compile();
            InitializeQueryExpresion();
            InitializeDisplayPropertyNames();
        }
        private void InitializeQueryExpresion()
        {
            var type = EntityType;
            var query = QueryExpression;
            _queryCountExpression = GetQueryCountExpression(type, query);
            _queryPageExpression = GetQueryPageExpression(type, _keyPropertyInfo, query);
        }
        private void InitializeDisplayPropertyNames()
        {
            _headerToPropertyNames.Clear();
            var validKind = ValidKind;
            var genericeNames = new List<DisplayPropertyInfo>();
            foreach (var ppt in _entitiyPropertys.Values)
            {
                _headerToPropertyNames.Add(ppt.Name, ppt.Name);
                GenericNameAttribute displayAttr = null;
                foreach (var att in ppt.GetCustomAttributes(typeof(GenericNameAttribute)).OfType<GenericNameAttribute>().OrderByDescending(i => i.Kind))
                {
                    var name = att.Name;
                    var kind = att.Kind;
                    if (_headerToPropertyNames.ContainsKey(name))
                        throw new ArgumentException($"存在同名的属性![{name}]");
                    _headerToPropertyNames.Add(att.Name, ppt.Name);
                    if (displayAttr == null)
                    {
                        if (kind == validKind)
                            displayAttr = att;
                        else if (string.IsNullOrEmpty(kind))
                            displayAttr = att;
                    }
                }
                if (displayAttr != null)
                    genericeNames.Add(new DisplayPropertyInfo(ppt, displayAttr));
            }
            EntityDisplayPropertyNames = genericeNames;
        }
        private void OnEdit(object sender, ExecutedRoutedEventArgs e)
        {
            var grid = e.Source as EFDataGrid;
            var item = e.Parameter;
            if (grid != null && item != null)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item);
                EFDataGridAssist.SetIsRowEditing(row, true);
            }
        }
        private void OnCancel(object sender, ExecutedRoutedEventArgs e)
        {
            var grid = e.Source as EFDataGrid;
            var item = e.Parameter;
            if (grid != null && item != null)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item);
                EFDataGridAssist.SetIsRowEditing(row, false);
            }
        }
        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            var grid = e.Source as EFDataGrid;
            var item = e.Parameter;
            if (grid != null && item != null)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item);
                foreach (var valueEditor in row.FindChildren<EFValueEditor>())
                {
                    if (valueEditor.IsValueChanged)
                    {
                        _entitiyPropertys[valueEditor.PropertyName].SetValue(item, valueEditor.ValidValue);
                        //


                    }
                }
                EFDataGridAssist.SetIsRowEditing(row, false);
            }
        }
        private void OnDelete(object sender, ExecutedRoutedEventArgs e)
        {
            var grid = e.Source as EFDataGrid;
            var item = e.Parameter;
            //
            if (grid != null && item != null)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item);
                EFDataGridAssist.SetIsRowEditing(row, false);
            }
        }
        private void OnLoad(object sender, ExecutedRoutedEventArgs e)
        {
            LoadData();
        }
        private void OnRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            
        }
        private void OnExport(object sender, ExecutedRoutedEventArgs e)
        {

        }
        private void OnImport(object sender, ExecutedRoutedEventArgs e)
        {
            //var result = await ImportWithFileDialog();
            //if (result == false)
            //    OnCapturedMessage("导入失败!");
            //else if (result == true)
            //{
            //    LoadDataAsync();
            //    OnCapturedMessage("导入成功!");
            //}
        }
        private async void LoadData()
        {
            if (_isLoading) return;
            var dbContextType = DbContextType;
            if (dbContextType == null || typeof(DbContext).IsAssignableFrom(dbContextType) == false)
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
        private async void LoadPage(int pageIndex)
        {
            if (_isLoading) return;
            var dbContextType = DbContextType;
            if (dbContextType == null || typeof(DbContext).IsAssignableFrom(dbContextType) == false)
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
        private void Refresh()
        {
            var pageIndex = PageIndex;
            LoadPage(pageIndex);
        }
        private Task<int> QueryCountAsync(Type contextType)
        {
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
                }
                return 0;
            });
        }
        private Task QueryPageAsync(Type contextType,int pageSize, int pageIndex)
        {
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
                }
            });
        }

        #region linq
        private Expression<Func<object, object>> GetKeyFunction(Type entityType, PropertyInfo keyPropertyInfo)
        {
            if (entityType == default || keyPropertyInfo == null) return null;

            //i=>i.Key
            var objParam = Linq.Expressions.Expression.Parameter(typeof(object));
            var model = Linq.Expressions.Expression.Convert(objParam, entityType);
            var getKey = Linq.Expressions.Expression.Property(model, keyPropertyInfo);
            var objBordy = Linq.Expressions.Expression.Convert(getKey, typeof(object));
            return Linq.Expressions.Expression.Lambda<Func<object, object>>(objBordy, objParam);
        }
        private Expression<Func<DbContext, int>> GetQueryCountExpression(Type entityType, Linq.Expressions.Expression query = null)
        {
            if (entityType == default) return null;

            //完整表达式 return db.Set<T>().Where(i => i).Count();   
            var db = Linq.Expressions.Expression.Parameter(typeof(DbContext));
            var dbSet = Linq.Expressions.Expression.Call(db, nameof(DbContext.Set), new Type[] { entityType });//db.Set<T>()
            //
            Linq.Expressions.Expression body = dbSet;
            if (query != null)
                body = Linq.Expressions.Expression.Invoke(query, dbSet);
            body = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.Count), new Type[] { entityType }, body);
            return Linq.Expressions.Expression.Lambda<Func<DbContext, int>>(body,db);
        }
        private Expression<Func<DbContext, int, int, IEnumerable<object>>> GetQueryPageExpression(Type entityType, PropertyInfo keyPropertyInfo, Linq.Expressions.Expression query = null)
        {
            if (entityType == default || keyPropertyInfo == null) return null;

            //完整表达式 return db.Set<T>().Where().OrderBy().Skip().Take();   
            var db = Linq.Expressions.Expression.Parameter(typeof(DbContext));
            var skipCount = Linq.Expressions.Expression.Parameter(typeof(int));
            var takeCount = Linq.Expressions.Expression.Parameter(typeof(int));
            var dbSet = Linq.Expressions.Expression.Call(db, nameof(DbContext.Set), new Type[] { entityType });//db.Set<T>()
            bool hasOrderby = false;
            Linq.Expressions.Expression body = dbSet;
            if (query != null)
            {
                body = Linq.Expressions.Expression.Invoke(query, dbSet);
                hasOrderby = IsExpresionContainsMethodName(query, nameof(Queryable.OrderBy));
            }
            if (hasOrderby == false)
            {
                var model = Linq.Expressions.Expression.Parameter(entityType);
                var getKey = Linq.Expressions.Expression.Property(model, keyPropertyInfo);
                var keyExp = Linq.Expressions.Expression.Lambda(getKey, model);
                body = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.OrderBy),
                   new Type[] { entityType, keyPropertyInfo.PropertyType },
                   body, keyExp);//db.Set<T>().OrderBy(i=>i.Key)
            }
            body = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.Skip), new Type[] { entityType }, body, skipCount);//db.Set<T>().Where().OrderBy(i=>i.Key).Skip();
            body = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.Take), new Type[] { entityType }, body, takeCount);//db.Set<T>().Where().OrderBy(i=>i.Key).Skip().Take();
            return Linq.Expressions.Expression.Lambda<Func<DbContext, int, int, IEnumerable<object>>>(body, db, skipCount, takeCount);
        }
        private bool IsExpresionContainsMethodName(Linq.Expressions.Expression expression, string methodName)
        {
            if (expression is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression)expression;
                if (methodExpression.Method.Name == methodName)
                    return true;
                foreach (var item in methodExpression.Arguments)
                {
                    if (IsExpresionContainsMethodName(item, methodName))
                    {
                        return true;
                    }
                }
            }
            else if (expression is LambdaExpression)
            {
                if (IsExpresionContainsMethodName(((LambdaExpression)expression).Body, methodName))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Excel 
        ///// <summary>
        ///// 导出，弹出文件路径选择窗体
        ///// </summary>
        ///// <param name="defaultFileName"></param>
        ///// <param name="tableName"></param>
        ///// <returns>null:取消 true:导表成功 false:导表失败</returns>
        //protected async Task<bool?> ExportWithFileDialog(string defaultFileName = null)
        //{
        //    SaveFileDialog dialog = new SaveFileDialog();
        //    dialog.FileName = string.IsNullOrEmpty(defaultFileName) ? _sheetName : defaultFileName;
        //    dialog.Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls";
        //    if (dialog.ShowDialog() == true)
        //        return await Export(dialog.FileName, dialog.FilterIndex == 2);
        //    return null;
        //}

        ///// <summary>
        ///// 导出
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <param name="tableName"></param>
        ///// <param name="isXls">xls:Excel2003 xlsx:Excel2007</param>
        ///// <returns></returns>
        //protected Task<bool> Export(string fileName, bool isXls = false)
        //{
        //    return Task.Run(() =>
        //    {
        //        try
        //        {
        //            IWorkbook workbook;
        //            if (File.Exists(fileName) == false)
        //                workbook = isXls ? new HSSFWorkbook() : (IWorkbook)new XSSFWorkbook();
        //            else
        //            {
        //                try
        //                {
        //                    using (var ms = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        //                    {
        //                        workbook = isXls ? new HSSFWorkbook(ms) : (IWorkbook)new XSSFWorkbook(ms);
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    OnCapturedException(e, "打开文件异常，请检查文件是否破损或者被占用!");
        //                    return false;
        //                }
        //            }

        //            //删除表
        //            var oldSheet = workbook.GetSheet(_sheetName);
        //            if (oldSheet != null)
        //            {
        //                var sheetIndex = workbook.GetSheetIndex(oldSheet);
        //                workbook.RemoveSheetAt(sheetIndex);
        //            }

        //            //创建表
        //            var sheet = workbook.CreateSheet(_sheetName);
        //            var rowIndex = 0;

        //            //创建列
        //            var columnRow = sheet.CreateRow(rowIndex);
        //            var columnPropertys = new List<PropertyInfo>();
        //            var columnIndex = 0;
        //            foreach (var item in GetExportPropertyNames())
        //            {
        //                if (_propertyInfos.ContainsKey(item) == false)
        //                    throw new Exception($"属性{item}不存在!");
        //                columnPropertys.Add(_propertyInfos[item]);
        //                var headerText = GetHeader(item);
        //                var cell = columnRow.CreateCell(columnIndex);
        //                cell.SetCellValue(headerText);
        //                columnIndex++;
        //            }

        //            //表数据                    
        //            foreach (var model in GetExportModels())
        //            {
        //                rowIndex++;
        //                var row = sheet.CreateRow(rowIndex);
        //                for (int i = 0; i < columnPropertys.Count; i++)
        //                {
        //                    var property = columnPropertys[i];
        //                    var propertyType = property.PropertyType;
        //                    var propertyValue = property.GetValue(model);
        //                    string valueText;
        //                    if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
        //                        valueText = $"{propertyValue:yyyy-MM-dd HH:mm:ss.fff}";
        //                    else if (propertyType == typeof(TimeSpan) || propertyType == typeof(TimeSpan?))
        //                        valueText = $"{propertyValue:c}";
        //                    else
        //                        valueText = propertyValue?.ToString();
        //                    var cell = row.CreateCell(i);
        //                    cell.SetCellValue(valueText);
        //                }
        //                OnExporting(rowIndex, model);
        //            }

        //            var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        //            workbook.Write(fileStream);
        //            fileStream.Close();
        //            return true;
        //        }
        //        catch (Exception e)
        //        {
        //            OnCapturedException(e, "导表异常");
        //            return false;
        //        }
        //    });
        //}

        ///// <summary>
        ///// 获取导出数据
        ///// </summary>
        ///// <returns></returns>
        //protected virtual IEnumerable<TModel> GetExportModels() => LoadAll();

        ///// <summary>
        ///// 获取导出属性
        ///// </summary>
        ///// <returns></returns>
        //protected virtual IEnumerable<string> GetExportPropertyNames() => _propertyInfos.Keys;

        ///// <summary>
        ///// 导出过程
        ///// </summary>
        ///// <param name="index"></param>
        //protected virtual void OnExporting(int index, TModel model) { }

        ///// <summary>
        ///// 导入
        ///// </summary>
        ///// <returns></returns>
        //protected async Task<bool?> ImportWithFileDialog()
        //{
        //    OpenFileDialog dialog = new OpenFileDialog();
        //    dialog.Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls";
        //    if (dialog.ShowDialog() == true)
        //        return await Import(dialog.FileName, dialog.FilterIndex == 2);
        //    return null;
        //}
        ///// <summary>
        ///// 导入
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <param name="isXls"></param>
        ///// <returns></returns>
        //protected Task<bool> Import(string fileName, bool isXls = false)
        //{
        //    return Task.Run(() =>
        //    {
        //        try
        //        {
        //            IWorkbook workbook;
        //            if (File.Exists(fileName) == false)
        //            {
        //                OnCapturedMessage("文件不存在!");
        //                return false;
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    using (var ms = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
        //                    {
        //                        workbook = isXls ? new HSSFWorkbook(ms) : (IWorkbook)new XSSFWorkbook(ms);
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    OnCapturedException(e, "打开文件失败，请检查文件是否破损或者被占用!");
        //                    return false;
        //                }
        //            }

        //            //获取表
        //            ISheet sheet = workbook.GetSheet(_sheetName);
        //            if (sheet == null)
        //                sheet = workbook.GetSheetAt(0);
        //            if (sheet == null)
        //            {
        //                OnCapturedMessage("文件中无数据!");
        //                return false;
        //            }

        //            //读取列
        //            var columnRow = sheet.GetRow(sheet.FirstRowNum);
        //            var hasKeyColumn = false;
        //            var keyColumnName = string.Empty;
        //            var keyIndex = -1;
        //            var columnIndex = 0;
        //            var columns = new List<Tuple<string, string>>();
        //            foreach (var item in columnRow)
        //            {
        //                var columnName = item?.ToString();
        //                if (_headerToPropertyNameDictionary.ContainsKey(columnName))
        //                {
        //                    var propertyName = _headerToPropertyNameDictionary[columnName];
        //                    if (propertyName == KeyPropertyName)
        //                    {
        //                        if (hasKeyColumn == false)
        //                        {
        //                            hasKeyColumn = true;
        //                            keyColumnName = columnName;
        //                            keyIndex = columnIndex;
        //                        }
        //                        else
        //                        {
        //                            OnCapturedMessage($"存在多个主键列[{keyIndex}:{keyColumnName}][{columnIndex}:{columnName}]");
        //                            return false;
        //                        }
        //                    }
        //                    columns.Add(new Tuple<string, string>(columnName, propertyName));
        //                }
        //                columnIndex++;
        //            }

        //            //读取值
        //            if (columns.Count == 0)
        //            {
        //                OnCapturedMessage("文件中未匹配到列!");
        //                return false;
        //            }
        //            var updateItems = new List<TModel>();
        //            var errors = new List<Tuple<int, string, object>>();
        //            bool isSkipError = false;
        //            using (var db = new TDbContext())
        //            {
        //                var dataSet = db.Set<TModel>();
        //                foreach (IRow item in sheet)
        //                {
        //                    if (item != columnRow)
        //                    {
        //                        TModel model = null;
        //                        bool isNewItem = false;
        //                        bool hasError = false;
        //                        if (hasKeyColumn)
        //                        {
        //                            var readedValue = item.GetCell(keyIndex)?.ToString();
        //                            object value;
        //                            if (this.TryParse(KeyPropertyName, readedValue, out value))
        //                            {
        //                                model = dataSet.Find(value);
        //                            }
        //                            else if (string.IsNullOrEmpty(readedValue) == false)
        //                            {
        //                                if (isSkipError || OnImportedFirstErrorItem(item.RowNum, KeyPropertyName, readedValue))
        //                                {
        //                                    isSkipError = true;
        //                                    hasError = true;
        //                                    errors.Add(new Tuple<int, string, object>(item.RowNum, KeyPropertyName, readedValue));
        //                                    continue;
        //                                }
        //                            }
        //                        }
        //                        if (model == null)
        //                        {
        //                            isNewItem = true;
        //                            model = new TModel();
        //                        }
        //                        for (int i = 0; i < columns.Count; i++)
        //                        {
        //                            if (isNewItem == false && hasKeyColumn && i == keyIndex)//已存在的列不允许修改主键列
        //                                continue;

        //                            var readedValue = item.GetCell(i)?.ToString();
        //                            var pair = columns[i];
        //                            var columnName = pair.Item1;
        //                            var propertyName = pair.Item2;
        //                            object value;
        //                            if (this.TryParse(propertyName, readedValue, out value))
        //                            {
        //                                OnImporting(item.RowNum, model);
        //                                _propertyInfos[propertyName].SetValue(model, value);
        //                            }
        //                            else if (isSkipError || OnImportedFirstErrorItem(item.RowNum, columnName, readedValue))
        //                            {
        //                                isSkipError = true;
        //                                hasError = true;
        //                                errors.Add(new Tuple<int, string, object>(item.RowNum, columnName, readedValue));
        //                                break;
        //                            }
        //                            else
        //                            {
        //                                return false;
        //                            }
        //                        }

        //                        if (hasError)
        //                        {
        //                            continue;
        //                        }

        //                        updateItems.Add(model);
        //                        if (isNewItem)
        //                        {
        //                            dataSet.Add(model);
        //                        }
        //                    }
        //                }
        //                //
        //                db.SaveChanges();
        //            }
        //            //
        //            OnImportedCompleted(updateItems, errors);
        //            return true;
        //        }
        //        catch (Exception e)
        //        {
        //            OnCapturedException(e, "导入数据异常");
        //        }

        //        return false;
        //    });
        //}

        //protected virtual void OnImporting(int rowIndex, TModel model) { }
        //protected virtual bool OnImportedFirstErrorItem(int rowIndex, string columnName, object value)
        //{
        //    if (MessageBox.Show($"数据格式不正确，是否忽略所有错误行？ [行：{rowIndex}   列：{columnName}   值：{value}]", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //protected virtual void OnImportedCompleted(List<TModel> importeds, List<Tuple<int, string, object>> errors)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    builder.AppendLine($"更新成功数量：{importeds.Count}  失败数量：{errors.Count}");
        //    if (errors.Count > 0)
        //    {
        //        builder.AppendLine("失败明细列表：");
        //        foreach (var item in errors)
        //        {
        //            builder.AppendLine($"[行：{item.Item1}   列：{item.Item2}   值：{item.Item3}]");
        //        }
        //    }
        //    MessageBox.Show(builder.ToString(), "导入结果", MessageBoxButton.OK);
        //}
        #endregion
    }


    public struct DisplayPropertyInfo
    {
        public DisplayPropertyInfo(PropertyInfo info,GenericNameAttribute attribute)
        {
            PropertyName = info.Name;
            PropertyType = info.PropertyType;
            GenericName = string.IsNullOrEmpty(attribute.Name) ? PropertyName : attribute.Name;
            IsReadOnly = attribute.IsReadOnly;
        }
        public DisplayPropertyInfo(string propertyName,Type propertyType,string genericName,bool isReadOnly)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            GenericName = genericName;
            IsReadOnly = isReadOnly;
        }
        public string PropertyName { get; }
        public Type PropertyType { get; }
        public string GenericName { get; }
        public bool IsReadOnly { get; }
    }
}
