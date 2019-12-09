using Microsoft.Win32;
using Prism;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.Entity.Migrations;

namespace System
{
    public abstract class DBViewModel<TModel, TDbContext> : NotifyPropertyChanged, IActiveAware, ISourceService
        where TModel : class,new()
        where TDbContext : DbContext, new()
    {
        private bool _isActive;
        private bool _isLoading;
        private Task _loadingTask;
        private int _displayCount;
        private int _Count;
        private int _pageCount;
        private int _pageIndex;
        private EditableViewModel _selectedItem;
        private Dictionary<string, PropertyInfo> _propertyInfos;
        private Dictionary<string, string> _propertyNameToHeaderDictionary;
        private Dictionary<string, string> _headerToPropertyNameDictionary;
        private Dictionary<string, string> _headerToPropertyNameExtraDictionary;
        public DBViewModel()
        {
            _displayCount = 50;
            _propertyInfos = typeof(TModel).GetProperties().ToDictionary(i => i.Name);
            _propertyNameToHeaderDictionary = CreatePropertyNameToHeaderDictionary();
            _headerToPropertyNameDictionary = new Dictionary<string, string>();
            KeyPropertyName = (KeyExpression.Body as MemberExpression)?.Member.Name;
            if (string.IsNullOrEmpty(KeyPropertyName) || _propertyInfos.ContainsKey(KeyPropertyName) == false)
                throw new Exception("主键表达式不正确!");
            foreach (var item in _propertyInfos.Keys)
            {
                if (_propertyNameToHeaderDictionary.ContainsKey(item) == false)
                    _propertyNameToHeaderDictionary.Add(item, item);
            }
            foreach (var item in _propertyNameToHeaderDictionary)
            {
                if (_propertyInfos.ContainsKey(item.Key) == false)
                    throw new Exception($"指定属性{item.Key}不存在!");
                if (string.IsNullOrEmpty(item.Value))
                    throw new Exception("标题不可为空!");
                if (_headerToPropertyNameDictionary.ContainsKey(item.Value))
                    throw new Exception("存在重复标题!");
                _headerToPropertyNameDictionary.Add(item.Value, item.Key);
            }
            Items = new ViewModelCollection<EditableViewModel>();
        }
        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value, OnIsActiveChanged); }
        }
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }
        public ViewModelCollection<EditableViewModel> Items { get; }
        public EditableViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }
        public EditableViewModel LastlyCreatedViewModel { get; private set; }
        public int DisplayCount
        {
            get { return _displayCount; }
            set { SetProperty(ref _displayCount, value); }
        }
        public int Count
        {
            get { return _Count; }
            set { SetProperty(ref _Count, value); }
        }
        public int PageCount
        {
            get { return _pageCount; }
            set { SetProperty(ref _pageCount, value); }
        }
        public int PageIndex
        {
            get { return _pageIndex; }
            set { SetProperty(ref _pageIndex, value, OnPageIndexChanged); }
        }
        protected abstract Expression<Func<TModel, object>> KeyExpression { get; }
        public string KeyPropertyName { get; }
        public Dictionary<string, string> HeaderToPropertyNameDictionary
        {
            get
            {
                if (_headerToPropertyNameExtraDictionary != null)
                    return _headerToPropertyNameExtraDictionary;
                return _headerToPropertyNameDictionary;
            }
            set => _headerToPropertyNameExtraDictionary = value;
        }
        protected virtual void OnIsActiveChanged(bool oldIsActive, bool newIsActive)
        {
            if (newIsActive)
            {
                LoadPageAsync();
            }
            IsActiveChanged?.Invoke(this, null);
        }
        protected virtual void OnPageIndexChanged(int oldPageIndex, int newPageIndex)
        {
            //
            LoadPageAsync();
        }
        protected virtual Dictionary<string, string> CreatePropertyNameToHeaderDictionary() => _propertyInfos.Keys.ToDictionary(i => i);
     
        #region load data
        /// <summary>
        /// 重新加载数据,会统计记录数量
        /// </summary>
        protected void LoadDataAsync()
        {
            var task = _loadingTask;
            if (task == null || task.IsCompleted)
            {
                task = Task.Run(() =>
                {
                    IsLoading = true;
                    try
                    {
                        using (var db = new TDbContext())
                        {
                            //计数
                            var count = OnQuery(db.Set<TModel>()).Count();
                            Count = count;
                            var pageSize = DisplayCount;
                            if (pageSize <= 0) pageSize = 1;
                            DisplayCount = pageSize;
                            int remain;
                            var pageCount = Math.DivRem(Count, DisplayCount, out remain);
                            if (remain > 0) pageCount++;
                            PageCount = pageCount;
                            PageIndex = 0;
                            if (count > 0)
                            {
                                var pageIndex = PageIndex;
                                if (pageIndex < 1) pageIndex = 1;
                                PageIndex = pageIndex;
                                ChangeItems(OnQuery(db.Set<TModel>()).Take(pageSize));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        OnCapturedException(e, "加载数据异常");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                });
                _loadingTask = task;
            }
        }
        /// <summary>
        /// 加载当前页数据
        /// </summary>
        /// <returns></returns>
        protected void LoadPageAsync()
        {
            var pageIndex = PageIndex;
            var pageSize = DisplayCount;
            if (pageIndex <= 0 || pageIndex > PageCount || pageSize <= 0) return;
            var task = _loadingTask;
            if (task == null || task.IsCompleted)
            {
                task = Task.Run(() =>
                {
                    IsLoading = true;
                    try
                    {
                        ChangeItems(LoadPage(pageIndex));
                    }
                    catch (Exception e)
                    {
                        OnCapturedException(e, "加载页数据异常");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                    return null;
                });
                _loadingTask = task;
            }
        }
        protected virtual void OnLoaded(int count) { }
        protected virtual IQueryable<TModel> OnQuery(IQueryable<TModel> query)
        {
            return query.OrderBy(KeyExpression);
        }
        protected IEnumerable<TModel> LoadPage(int pageIndex)
        {
            var pageSize = DisplayCount;
            if (pageIndex <= 0 || pageIndex > PageCount || pageSize <= 0) yield break;
            using (var db = new TDbContext())
            {
                foreach (var item in OnQuery(db.Set<TModel>()).Skip((pageIndex - 1) * pageSize).Take(pageSize))
                {
                    yield return item;
                }
            }
            yield break;
        }
        protected IEnumerable<TModel> LoadAll()
        {
            using (var db = new TDbContext())
            {
                foreach (var item in OnQuery(db.Set<TModel>()))
                {
                    yield return item;
                }
            }
        }
        #endregion

        #region collection
        private void ChangeItems(IEnumerable<TModel> models)
        {
            var getKey = KeyExpression.Compile();
            Items.Change(models.ToDictionary(i => getKey(i)), CreateViewModelFrom, UpdateViewModelFrom);
        }
        protected virtual EditableViewModel CreateViewModelFrom(TModel model)
        {
            var viewModel = new EditableViewModel(model);
            LastlyCreatedViewModel = viewModel;
            return viewModel;
        }
        protected virtual void UpdateViewModelFrom(EditableViewModel viewModel, TModel model)
        {
            viewModel.Source = model;
        }
        #endregion

        #region ISourceService
        public string GetHeader(string propertyName) => _propertyNameToHeaderDictionary[propertyName];
        public bool TryParse(string propertyName, object editedValue, out object value)
        {
            value = null;
            try
            {
                if (_propertyInfos.ContainsKey(propertyName))
                {
                    value = Convert.ChangeType(value, _propertyInfos[propertyName].PropertyType);
                    return true;
                }
            }
            catch { }
            return false;
        }
        protected virtual void OnCapturedException(Exception e,string message, [CallerMemberName] string methodName = null) { }
        public async virtual Task SaveChangedPropertys(EditableViewModel viewModel, object source, Dictionary<string, object> changedPropertys)
        {
            TModel model = (TModel)source;
            await OnSaveChangedPropertys(viewModel, model, changedPropertys);
        }
        protected async virtual Task<bool> OnSaveChangedPropertys(EditableViewModel viewModel, TModel model, Dictionary<string, object> changedPropertys)
        {
            foreach (var item in changedPropertys)
            {
                _propertyInfos[item.Key].SetValue(model, item.Value);
            }
            var result = await Task.Run(() =>
            {
                try
                {
                    using (var db = new TDbContext())
                    {
                        db.Entry(model).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    return true;
                }
                catch (Exception e)
                {
                    OnCapturedException(e,"保存数据异常");
                }
                return false;
            });
            LoadPageAsync();
            return result;
        }
        public async Task Delete(EditableViewModel viewModel, object source)
        {
            TModel model = (TModel)source;
            await OnDelete(viewModel, model);
        }
        protected async virtual Task<bool> OnDelete(EditableViewModel viewModel, TModel model)
        {
            var result = await Task.Run(() =>
            {
                try
                {
                    using (var db = new TDbContext())
                    {
                        db.Entry(model).State = EntityState.Deleted;
                        db.SaveChanges();
                    }
                    return true;
                }
                catch (Exception e)
                {
                    OnCapturedException(e,"删除数据异常");
                }
                return false;
            });
            LoadDataAsync();
            return result;
        }
        public void OnCaptureErrorEditedValue(EditableViewModel viewModel, object source, string propertyName, object editedValue)
        {
            OnEditedValueWithError(viewModel, (TModel)source, propertyName, editedValue);
        }
        protected virtual void OnEditedValueWithError(EditableViewModel viewModel, TModel model, string propertyName, object editedValue) { }
        #endregion

        #region Excel
        /// <summary>
        /// 导出，弹出文件路径选择窗体
        /// </summary>
        /// <param name="defaultFileName"></param>
        /// <param name="tableName"></param>
        /// <returns>null:取消 true:导表成功 false:导表失败</returns>
        protected async Task<bool?> ExportWithFileDialog(string defaultFileName = null, string tableName = null)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (string.IsNullOrEmpty(defaultFileName) == false)
                dialog.FileName = defaultFileName;
            dialog.Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls";
            if (dialog.ShowDialog() == true)
                return await Export(dialog.FileName, tableName, dialog.FilterIndex == 2);
            return null;
        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected Task<bool> Export(string fileName, string tableName = null, bool isExcel2003 = false)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (File.Exists(fileName) == false)
                        File.Create(fileName).Close();
                    if (string.IsNullOrEmpty(tableName))
                        tableName = "Sheet";
                    string connectionString;
                    string hdr = "YES";//是否第一行是列名 YES/NO
                    int imex = 1;//  /// IMEX 三种模式。IMEX是用来告诉驱动程序使用Excel文件的模式，其值有0、1、2三种，分别代表只读、只写、混合模式。
                    if (isExcel2003)//Excel 版本 2003
                        connectionString = $"Provider=Microsoft.Jet.OleDb.4.0; data source={fileName};Extended Properties='Excel 8.0; HDR={hdr}; IMEX={imex}'";
                    else//Excel 版本 2007
                        connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0; data source={fileName};Extended Properties='Excel 12.0 Xml; HDR={hdr}; IMEX={imex}'";
                    using (var cnn = new OleDbConnection(connectionString))
                    {
                        var cmd = new OleDbCommand();
                        cmd.Connection = cnn;
                        OleDbTransaction transaction = null;
                        try
                        {
                            cnn.Open();
                            transaction = cnn.BeginTransaction(IsolationLevel.ReadCommitted);
                            cmd.Connection = cnn;
                            cmd.Transaction = transaction;

                            string sql;
                            StringBuilder sqlBuilder = new StringBuilder();

                            //删除表
                            sql = $"DROP TABLE IF EXISTS {tableName}";
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();

                            //创建表
                            List<string> columns = new List<string>();
                            foreach (var item in GetExportPropertyNames())
                            {
                                if (_propertyInfos.ContainsKey(item) == false)
                                    throw new Exception($"属性{item}不存在!");
                                columns.Add(item);
                                if (sqlBuilder.Length == 0)
                                    sqlBuilder.AppendFormat("[{0}] TEXT(200)", item);
                                else
                                    sqlBuilder.AppendFormat(",[{0}] TEXT(200)", item);
                            }
                            sql = $"CREATE TABLE {tableName}({sqlBuilder}) ";
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();

                            if (columns.Count == 0) 
                                throw new Exception("导出表列为0!");

                            //添加数据
                            var index = 0;
                            var models = GetExportModels();
                            foreach (var model in models)
                            {
                                sqlBuilder.Clear();
                                foreach (var item in columns)
                                {
                                    var property = _propertyInfos[item];
                                    var propertyType = property.PropertyType;
                                    var propertyValue = property.GetValue(model);
                                    string valueText;
                                    if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                                        valueText = $"'{propertyValue:yyyy-MM-dd HH:mm:ss.fff}'";
                                    else if (propertyType == typeof(TimeSpan) || propertyType == typeof(TimeSpan?))
                                        valueText = $"'{propertyValue:c}'";
                                    else
                                        valueText = $"'{propertyValue}'";
                                    if (sqlBuilder.Length == 0)
                                        sqlBuilder.Append(valueText);
                                    else
                                        sqlBuilder.Append($"{valueText},");
                                }
                                sql = $"INSERT INTO {tableName} VALUES({sqlBuilder})";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();
                                OnExporting(index,model);
                                index++;
                            }

                        }
                        catch (Exception e)
                        {
                            OnCapturedException(e,"导出数据异常");
                            try
                            {
                                transaction?.Rollback();
                            }
                            catch { }
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    OnCapturedException(e, "导表异常");
                    return false;
                }
            });
        }
        /// <summary>
        /// 获取导出数据
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<TModel> GetExportModels() => LoadAll();
        /// <summary>
        /// 获取导出属性
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<string> GetExportPropertyNames() => _propertyInfos.Keys;
        /// <summary>
        /// 导出过程
        /// </summary>
        /// <param name="index"></param>
        protected virtual void OnExporting(int index, TModel model) { }

        /// <summary>
        /// 导入
        /// </summary>
        /// <returns></returns>
        protected async Task<bool?> ImportWithFileDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls";
            if (dialog.ShowDialog() == true)
                return await Import(dialog.FileName, isExcel2003: dialog.FilterIndex == 2);
            return null;
        }
        protected Task<bool> Import(string fileName, string tableName = null, bool isExcel2003 = false)
        {
            return Task.Run(() =>
            {
                string connectionString;
                string hdr = "YES";//是否第一行是列名 YES/NO
                int imex = 0;//  /// IMEX 三种模式。IMEX是用来告诉驱动程序使用Excel文件的模式，其值有0、1、2三种，分别代表只读、只写、混合模式。
                if (isExcel2003)//Excel 版本 2003
                    connectionString = $"Provider=Microsoft.Jet.OleDb.4.0; data source={fileName};Extended Properties='Excel 8.0; HDR={hdr}; IMEX={imex}'";
                else//Excel 版本 2007
                    connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0; data source={fileName};Extended Properties='Excel 12.0 Xml; HDR={hdr}; IMEX={imex}'";
                using (OleDbConnection cnn = new OleDbConnection(connectionString))
                {
                    var cmd = new OleDbCommand();
                    cmd.Connection = cnn;
                    try
                    {
                        cnn.Open();

                        //获取表名
                        if (string.IsNullOrEmpty(tableName))
                        {
                            DataTable dt = cnn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                tableName = dt.Rows[0][2].ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(tableName))
                        {
                            OnCapturedException(null, "指定Excel中不存在数据!");
                            return false;
                        }

                        //导入数据
                        var sql = $"SELECT * FROM [{tableName}]";
                        cmd.CommandText = sql;
                        var reader = cmd.ExecuteReader();
                        var errors = new List<Tuple<int, string, object>>();
                        var updateItems = new List<TModel>();
                        using (var db = new TDbContext())
                        {
                            int index = -1;
                            while (reader.Read())
                            {
                                index++;
                                var model = new TModel();
                                for (int i = 0; i < reader.VisibleFieldCount; i++)
                                {
                                    var name = reader.GetName(i);
                                    var readedValue = reader.GetValue(i);
                                    var headerToPropertyNameDictionary = HeaderToPropertyNameDictionary;
                                    if (headerToPropertyNameDictionary.ContainsKey(name))
                                    {
                                        var propertyName = headerToPropertyNameDictionary[name];
                                        object value;
                                        if (this.TryParse(propertyName, readedValue, out value))
                                        {
                                            _propertyInfos[propertyName].SetValue(model, value);
                                        }
                                        else if (OnImportedFirstErrorItem(index, name, readedValue))
                                        {
                                            model = null;
                                            errors.Add(new Tuple<int, string, object>(index, name, readedValue));
                                            break;
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                                if (model != null)
                                    updateItems.Add(model);
                            }
                            if (updateItems.Count > 0)
                            {
                                db.Set<TModel>().AddOrUpdate(updateItems.ToArray());
                                db.SaveChanges();
                            }
                        }
                        OnImportedCompleted(updateItems, errors);
                        return true;
                    }
                    catch (Exception e)
                    {
                        OnCapturedException(e, "导入数据异常");
                    }
                }
                return false;
            });
        }
        protected virtual void OnImporting(int index,TModel model) { }
        protected virtual bool OnImportedFirstErrorItem(int index, string columnName, object value) => true;
        protected virtual void OnImportedCompleted(List<TModel> importeds,List<Tuple<int,string,object>> errors) { }
        #endregion

        public event EventHandler IsActiveChanged;
    }
}
