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
        #endregion

        public static readonly DependencyProperty DbContextTypeProperty =
            DependencyProperty.Register(nameof(DbContextType), typeof(Type), typeof(EFDataGrid), new PropertyMetadata(default, OnPropertyChanged));
        public static readonly DependencyProperty EntityTypeProperty =
            DependencyProperty.Register(nameof(EntityType), typeof(Type), typeof(EFDataGrid), new PropertyMetadata(default, OnPropertyChanged));
        public static readonly DependencyProperty ValidKindProperty =
            DependencyProperty.Register(nameof(ValidKind), typeof(string), typeof(EFDataGrid), new PropertyMetadata(null, OnPropertyChanged));
        private static readonly DependencyPropertyKey EntityDisplayPropertyNamesPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(EntityDisplayPropertyNames), typeof(Dictionary<string, string>), typeof(EFDataBox), new PropertyMetadata(null));
        public static readonly DependencyProperty EntityDisplayPropertyNamesProperty = EntityDisplayPropertyNamesPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey DisplayItemsSourcePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(DisplayItemsSource), typeof(IEnumerable), typeof(EFDataBox), new PropertyMetadata(null));
        public static readonly DependencyProperty DisplayCountProperty =
            DependencyProperty.Register(nameof(DisplayCount), typeof(int), typeof(EFDataBox), new PropertyMetadata(50, null, OnCoereDisplayCount));
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
                box.RefreshEntityType();
            }
            else if (e.Property == ValidKindProperty)
            {
                box.RefresDisplayPropertyNames();
            }
        }
        private static object OnCoereDisplayCount(DependencyObject d, object baseValue)
        {
            if ((int)baseValue <= 0)
                return 50;
            return baseValue;
        }

        private string _entityKeyPropertyName;
        private Dictionary<string, PropertyInfo> _entitiyPropertys;
        private ObservableDictionary<object, object> _itemsSource;
        private object _loadingLocker;
        private bool _isLoading;
        public EFDataBox()
        {
            _loadingLocker = new object();
            _isLoading = false;
            _entitiyPropertys = new Dictionary<string, PropertyInfo>();
            _itemsSource = new ObservableDictionary<object, object>();
            EFDataGridAssist.SetItemsSource(this, _itemsSource);
            DisplayItemsSource = _itemsSource;
            this.SetBinding(EFDataGridAssist.DisplayPropertyNamesProperty, new Binding(nameof(EntityDisplayPropertyNames)) { Source = this, Mode = BindingMode.OneWay });
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, new ExecutedRoutedEventHandler(OnPen)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, new ExecutedRoutedEventHandler(OnClose)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, new ExecutedRoutedEventHandler(OnSave)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, new ExecutedRoutedEventHandler(OnDelete)));
            this.CommandBindings.Add(new CommandBinding(LoadCommand, new ExecutedRoutedEventHandler(OnLoad)));
            this.CommandBindings.Add(new CommandBinding(RefreshCommand, new ExecutedRoutedEventHandler(OnRefresh)));
            this.CommandBindings.Add(new CommandBinding(ExportCommand, new ExecutedRoutedEventHandler(OnExport)));
            this.CommandBindings.Add(new CommandBinding(ImportCommand, new ExecutedRoutedEventHandler(OnImport)));
            this.SetBinding(EFDataGridBarAssist.DisplayCountProperty, new Binding(nameof(DisplayCount)) { Source = this, Mode = BindingMode.OneWay });
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
        public Dictionary<string, string> EntityDisplayPropertyNames
        {
            get { return (Dictionary<string, string>)GetValue(EntityDisplayPropertyNamesProperty); }
            protected set { SetValue(EntityDisplayPropertyNamesPropertyKey, value); }
        }
        public IEnumerable DisplayItemsSource
        {
            get { return (IEnumerable)GetValue(DisplayItemsSourceProperty); }
            protected set { SetValue(DisplayItemsSourcePropertyKey, value); }
        }
        public int DisplayCount
        {
            get { return (int)GetValue(DisplayCountProperty); }
            set { SetValue(DisplayCountProperty, value); }
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
            RefreshEntityType();
        }

        private void RefreshEntityType()
        {
            var type = EntityType;
            _entitiyPropertys.Clear();
            foreach (var item in type.GetProperties())
            {
                _entitiyPropertys.Add(item.Name, item);
            }
            //默认第一个属性为主键
            _entityKeyPropertyName = _entitiyPropertys.Keys.FirstOrDefault();
            RefresDisplayPropertyNames();
        }
        private void RefresDisplayPropertyNames()
        {
            var validKind = ValidKind;
            var genericeNames = new Dictionary<string, string>();
            foreach (var item in _entitiyPropertys.Values)
            {
                var genericName = item.GetCustomAttributes(typeof(GenericNameAttribute)).OfType<GenericNameAttribute>().OrderBy(i => i.Kind).FirstOrDefault(i => i.Kind == validKind || string.IsNullOrEmpty(i.Kind))?.Name;
                if (string.IsNullOrEmpty(genericName))
                    genericName = item.Name;
                genericeNames.Add(item.Name, genericName);
            }
            EntityDisplayPropertyNames = genericeNames;
        }
        private void OnPen(object sender, ExecutedRoutedEventArgs e)
        {
            var editor = e.Source as EFEditorBase;
            var item = editor.DataContext;
            var row = editor.RowOwner;
            EFDataGridAssist.SetIsRowEditing(row, true);
        }
        private void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            var editor = e.Source as EFEditorBase;
            var item = editor.DataContext;
            var row = editor.RowOwner;
            EFDataGridAssist.SetIsRowEditing(row, false);
        }
        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            var editor = e.Source as EFEditorBase;
            var item = editor.DataContext;
            var row = editor.RowOwner;
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
        private void OnDelete(object sender, ExecutedRoutedEventArgs e)
        {
            var editor = e.Source as EFEditorBase;
            var item = editor.DataContext;
            var row = editor.RowOwner;
            //
            EFDataGridAssist.SetIsRowEditing(row, false);
        }
        private void OnLoad(object sender, ExecutedRoutedEventArgs e)
        {
            LoadData();
        }
        private void OnRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            LoadData();
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
            var entityType = EntityType;
            if (entityType == null || entityType.IsClass == false)
                return;
            lock (_loadingLocker)
            {
                if (_isLoading) return;
                _isLoading = true;
            }
            IsLoading = true;

            var queryExpression = QueryExpression;
            var count = await QueryCountAsync(dbContextType, entityType, queryExpression);
            var pageSize = DisplayCount;
            if (pageSize <= 0)
            {
                pageSize = 50;
                DisplayCount = 50;
            }
            int remain;
            var pageCount = Math.DivRem(count, pageSize, out remain);
            if (remain > 0) pageCount++;
            var pageIndex = 0;
            if (pageCount > 0)
                pageIndex = 1;
            EFDataGridBarAssist.SetCount(this, count);
            EFDataGridBarAssist.SetPageCount(this, pageCount);
            EFDataGridBarAssist.SetPageIndex(this, pageIndex);
            await QueryPageAsync(dbContextType, entityType, queryExpression, pageSize, pageIndex);

            IsLoading = false;
        }
        private Task<int> QueryCountAsync(Type contextType, Type entityType, Linq.Expressions.Expression qury)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var db = (DbContext)Activator.CreateInstance(contextType))
                    {
                        //完整表达式 return db.Set<T>().Where(i => i).Count();   
                        var exp = GetDbSetExpression(db, entityType);//db.Set<T>()
                        var keyLam = GetKeyExpression(entityType, _entityKeyPropertyName);//i=>i.Key
                        bool isOrdered = false;
                        if (qury != null && qury is LambdaExpression)
                        {
                            exp = Linq.Expressions.Expression.Invoke(qury, exp);
                            isOrdered = IsContainsMethodName(qury, nameof(Queryable.OrderBy));
                        }
                        if (isOrdered == false)
                            exp = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.OrderBy), new Type[] { entityType, _entitiyPropertys[_entityKeyPropertyName].PropertyType }, exp, keyLam);//db.Set<T>().OrderBy(i=>i.Key)
                        exp = Linq.Expressions.Expression.Call(typeof(Queryable), nameof(Queryable.Count), new Type[] { entityType }, exp);//db.Set<T>().OrderBy(i=>i.Key)...Count();
                        var lambda = Linq.Expressions.Expression.Lambda<Func<int>>(exp);
                        return lambda.Compile().Invoke();

                    }
                }
                catch (Exception e)
                {
                    //
                }
                return 0;
            });
        }

        private bool IsContainsMethodName(Linq.Expressions.Expression expression, string methodName)
        {
            if (expression is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression)expression;
                if (methodExpression.Method.Name == methodName)
                    return true;
                foreach (var item in methodExpression.Arguments)
                {
                    if (IsContainsMethodName(item, methodName))
                    {
                        return true;
                    }
                }
            }
            else if (expression is LambdaExpression)
            {
                if (IsContainsMethodName(((LambdaExpression)expression).Body, methodName))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 获取DbSet<Entity>表达式
        /// </summary>
        /// <param name="db"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private Linq.Expressions.Expression GetDbSetExpression(DbContext db,Type entityType)
        {
            var instance = Linq.Expressions.Expression.Constant(db);//db
            return Linq.Expressions.Expression.Call(instance, nameof(DbContext.Set), new Type[] { entityType });//db.Set<T>
        }

      

        /// <summary>
        /// 获取主键的lambda表达式 i=>i.Key
        /// </summary>
        /// <returns></returns>
        private Linq.Expressions.Expression GetKeyExpression(Type entityType, string keyName)
        {
            //构建获取主键的表达式 i=>i.Key;
            var param = Linq.Expressions.Expression.Parameter(entityType);//参数i
            var propertyCall = Linq.Expressions.Expression.Property(param, entityType.GetProperty(keyName));//i.Key;
            return Linq.Expressions.Expression.Lambda(propertyCall, new ParameterExpression[] { param });// i=>i.Key;
        }

        private Task QueryPageAsync(Type contextType, Type entityType, Linq.Expressions.Expression qury, int pageSize, int pageIndex)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var db = (DbContext)Activator.CreateInstance(contextType))
                    {
                        //完整表达式 return db.Set<T>().Where(i => i).Orderby(i=>i.Key).Skip(pageSize*page).Take(pageSize);




                        //IQueryable queryable;

                        ////构建获取主键的表达式 i=>i.Key;
                        //var param = Linq.Expressions.Expression.Parameter(entityType);//参数i
                        //var propertyCall = Linq.Expressions.Expression.Call(param, entityType.GetMethod(_entityKeyPropertyName));//i.Key;
                        //var getKeyLambda = Linq.Expressions.Expression.Lambda<Func<object, object>>(propertyCall,new ParameterExpression[] { param });// i=>i.Key;
                        //_itemsSource.SetSourceAsync(queryable, getKeyLambda.Compile());
                    }
                }
                catch (Exception e)
                {

                }
                return 0;
            });
        }

      


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
}
