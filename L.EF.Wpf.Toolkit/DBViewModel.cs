﻿using Microsoft.Win32;
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
using System.ComponentModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace System
{
    public abstract class DBViewModel<TModel,TKey,TDbContext> : NotifyPropertyChanged, IActiveAware, ISourceService
        where TModel : class, new()
        where TDbContext : DbContext, new()
    {
        private readonly Threading.SynchronizationContext _context;

        private bool _isActive;
        private bool _isLoading;
        private Task _loadingTask;
        private int _displayCount;
        private int _Count;
        private int _pageCount;
        private int _pageIndex;
        private Expression<Func<TModel, TKey>> _keyExpression;
        private EditableViewModel _selectedItem;
        private Dictionary<string, PropertyInfo> _propertyInfos;
        private string _sheetName;
        private Dictionary<string, string> _propertyNameToHeaderDictionary;
        private Dictionary<string, string> _headerToPropertyNameDictionary;
        public DBViewModel(Expression<Func<TModel,TKey>> keyExpression)
        {
            _keyExpression = keyExpression ?? throw new Exception("主键表述不可为空!");
            _context = Threading.SynchronizationContext.Current ?? throw new Exception("模型只能在UI单线程中初始化!");
            _displayCount = 50;
            _propertyInfos = typeof(TModel).GetProperties().ToDictionary(i => i.Name);
            _sheetName = typeof(TModel).GetCustomAttributes(typeof(DescriptionAttribute)).OfType<DescriptionAttribute>().FirstOrDefault()?.Description ?? typeof(TModel).Name;
            _propertyNameToHeaderDictionary = new Dictionary<string, string>();
            _headerToPropertyNameDictionary = new Dictionary<string, string>();
            foreach (var property in _propertyInfos.Values)
            {
                var propertyName = property.Name;
                foreach (DescriptionAttribute item in property.GetCustomAttributes(typeof(DescriptionAttribute)))
                {
                    var description = item.Description;
                    if (string.IsNullOrEmpty(description) == false && _propertyNameToHeaderDictionary.ContainsKey(propertyName) == false)
                    {
                        if (_propertyNameToHeaderDictionary.ContainsKey(propertyName) == false)
                            _propertyNameToHeaderDictionary.Add(propertyName, description);
                        if (_headerToPropertyNameDictionary.ContainsKey(description) == false)
                            _headerToPropertyNameDictionary.Add(description, propertyName);
                    }
                }
                if (_propertyNameToHeaderDictionary.ContainsKey(propertyName) == false)
                    _propertyNameToHeaderDictionary.Add(propertyName, propertyName);
                if (_headerToPropertyNameDictionary.ContainsKey(propertyName) == false)
                    _headerToPropertyNameDictionary.Add(propertyName, propertyName);
            }
            KeyPropertyName = (keyExpression.Body as MemberExpression)?.Member.Name
                ?? ((keyExpression.Body as UnaryExpression)?.Operand as MemberExpression)?.Member.Name
                ?? throw new Exception("获取主键名失败!");
            if (string.IsNullOrEmpty(KeyPropertyName) || _propertyInfos.ContainsKey(KeyPropertyName) == false)
                throw new Exception("主键表达式不正确!");
            Items = new ViewModelCollection<EditableViewModel>();
            ImportCommand = new AsyncCommand(OnImport);
            ExportCommand = new AsyncCommand(OnExport);
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
        public string KeyPropertyName { get; }
        public AsyncCommand ImportCommand { get; }
        public AsyncCommand ExportCommand { get; }
        protected PropertyInfo Property(string propertyName) => _propertyInfos[propertyName];
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
                            if (count > 0)
                            {
                                var pageIndex = PageIndex;
                                if (pageIndex < 1) pageIndex = 1;
                                PageIndex = pageIndex;
                                ChangeItems(OnQuery(db.Set<TModel>()).Take(pageSize));
                            }
                            else
                            {
                                PageIndex = 0;
                                OnCapturedMessage("当前未检索到数据!");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        OnCapturedException(e, "加载数据异常!");
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
                        OnCapturedException(e, "加载页数据异常!");
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
        protected virtual IQueryable<TModel> OnQuery(IQueryable<TModel> query)
        {
            return query.OrderBy(_keyExpression);
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
            var getKey = _keyExpression.Compile();
            Items.Change(models.ToDictionary(i => (object)getKey(i)), CreateViewModelFrom, UpdateViewModelFrom);
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
                    value = Convert.ChangeType(editedValue, _propertyInfos[propertyName].PropertyType);
                    return true;
                }
            }
            catch { }
            return false;
        }
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
                    OnCapturedException(e, "保存数据异常");
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
                    OnCapturedException(e, "删除数据异常");
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
        protected virtual async Task OnImport()
        {
            var result = await ImportWithFileDialog();
            if (result == false)
                OnCapturedMessage("导入失败!");
            else if (result == true)
            {
                LoadDataAsync();
                OnCapturedMessage("导入成功!");
            }
        }
        protected virtual async Task OnExport()
        {
            var result = await ExportWithFileDialog();
            if (result == false)
                OnCapturedMessage("导出失败!");
            else if (result == true)
                OnCapturedMessage("导出成功!");
        }

        /// <summary>
        /// 导出，弹出文件路径选择窗体
        /// </summary>
        /// <param name="defaultFileName"></param>
        /// <param name="tableName"></param>
        /// <returns>null:取消 true:导表成功 false:导表失败</returns>
        protected async Task<bool?> ExportWithFileDialog(string defaultFileName = null)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = string.IsNullOrEmpty(defaultFileName) ? _sheetName : defaultFileName;
            dialog.Filter = "Excel2007文件|*.xlsx|Excel2003文件|*.xls";
            if (dialog.ShowDialog() == true)
                return await Export(dialog.FileName, dialog.FilterIndex == 2);
            return null;
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="tableName"></param>
        /// <param name="isXls">xls:Excel2003 xlsx:Excel2007</param>
        /// <returns></returns>
        protected Task<bool> Export(string fileName, bool isXls = false)
        {
            return Task.Run(() =>
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
                            OnCapturedException(e, "打开文件异常，请检查文件是否破损或者被占用!");
                            return false;
                        }
                    }

                    //删除表
                    var oldSheet = workbook.GetSheet(_sheetName);
                    if (oldSheet != null)
                    {
                        var sheetIndex = workbook.GetSheetIndex(oldSheet);
                        workbook.RemoveSheetAt(sheetIndex);
                    }

                    //创建表
                    var sheet = workbook.CreateSheet(_sheetName);
                    var rowIndex = 0;

                    //创建列
                    var columnRow = sheet.CreateRow(rowIndex);
                    var columnPropertys = new List<PropertyInfo>();
                    var columnIndex = 0;
                    foreach (var item in GetExportPropertyNames())
                    {
                        if (_propertyInfos.ContainsKey(item) == false)
                            throw new Exception($"属性{item}不存在!");
                        columnPropertys.Add(_propertyInfos[item]);
                        var headerText = GetHeader(item);
                        var cell = columnRow.CreateCell(columnIndex);
                        cell.SetCellValue(headerText);
                        columnIndex++;
                    }

                    //表数据                    
                    foreach (var model in GetExportModels())
                    {
                        rowIndex++;
                        var row = sheet.CreateRow(rowIndex);
                        for (int i = 0; i < columnPropertys.Count; i++)
                        {
                            var property = columnPropertys[i];
                            var propertyType = property.PropertyType;
                            var propertyValue = property.GetValue(model);
                            string valueText;
                            if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                                valueText = $"{propertyValue:yyyy-MM-dd HH:mm:ss.fff}";
                            else if (propertyType == typeof(TimeSpan) || propertyType == typeof(TimeSpan?))
                                valueText = $"{propertyValue:c}";
                            else
                                valueText = propertyValue?.ToString();
                            var cell = row.CreateCell(i);
                            cell.SetCellValue(valueText);
                        }
                        OnExporting(rowIndex, model);
                    }

                    var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    workbook.Write(fileStream);
                    fileStream.Close();
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
                return await Import(dialog.FileName, dialog.FilterIndex == 2);
            return null;
        }
        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="isXls"></param>
        /// <returns></returns>
        protected Task<bool> Import(string fileName, bool isXls = false)
        {
            return Task.Run(() =>
            {
                try
                {
                    IWorkbook workbook;
                    if (File.Exists(fileName) == false)
                    {
                        OnCapturedMessage("文件不存在!");
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
                            OnCapturedException(e, "打开文件失败，请检查文件是否破损或者被占用!");
                            return false;
                        }
                    }

                    //获取表
                    ISheet sheet = workbook.GetSheet(_sheetName);
                    if (sheet == null)
                        sheet = workbook.GetSheetAt(0);
                    if (sheet == null)
                    {
                        OnCapturedMessage("文件中无数据!");
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
                        if (_headerToPropertyNameDictionary.ContainsKey(columnName))
                        {
                            var propertyName = _headerToPropertyNameDictionary[columnName];
                            if (propertyName == KeyPropertyName)
                            {
                                if (hasKeyColumn == false)
                                {
                                    hasKeyColumn = true;
                                    keyColumnName = columnName;
                                    keyIndex = columnIndex;
                                }
                                else
                                {
                                    OnCapturedMessage($"存在多个主键列[{keyIndex}:{keyColumnName}][{columnIndex}:{columnName}]");
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
                        OnCapturedMessage("文件中未匹配到列!");
                        return false;
                    }
                    var updateItems = new List<TModel>();
                    var errors = new List<Tuple<int, string, object>>();
                    bool isSkipError = false;
                    using (var db = new TDbContext())
                    {
                        var dataSet = db.Set<TModel>();
                        foreach (IRow item in sheet)
                        {
                            if (item != columnRow)
                            {
                                TModel model = null;
                                bool isNewItem = false;
                                bool hasError = false;
                                if (hasKeyColumn)
                                {
                                    var readedValue = item.GetCell(keyIndex)?.ToString();
                                    object value;
                                    if (this.TryParse(KeyPropertyName, readedValue, out value))
                                    {
                                        model = dataSet.Find(value);
                                    }
                                    else if (string.IsNullOrEmpty(readedValue) == false)
                                    {
                                        if (isSkipError || OnImportedFirstErrorItem(item.RowNum,KeyPropertyName, readedValue))
                                        {
                                            isSkipError = true;
                                            hasError = true;
                                            errors.Add(new Tuple<int, string, object>(item.RowNum, KeyPropertyName, readedValue));
                                            continue;
                                        }
                                    }
                                }
                                if (model == null)
                                {
                                    isNewItem = true;
                                    model = new TModel();
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
                                        OnImporting(item.RowNum, model);
                                        _propertyInfos[propertyName].SetValue(model, value);
                                    }
                                    else if (isSkipError || OnImportedFirstErrorItem(item.RowNum, columnName, readedValue))
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
                    OnImportedCompleted(updateItems, errors);
                    return true;
                }
                catch (Exception e)
                {
                    OnCapturedException(e, "导入数据异常");
                }

                return false;
            });
        }
       
        protected virtual void OnImporting(int rowIndex, TModel model) { }
        protected virtual bool OnImportedFirstErrorItem(int rowIndex, string columnName, object value)
        {
            if (MessageBox.Show($"数据格式不正确，是否忽略所有错误行？ [行：{rowIndex}   列：{columnName}   值：{value}]", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                return true;
            }
            return false;
        }
        protected virtual void OnImportedCompleted(List<TModel> importeds, List<Tuple<int, string, object>> errors)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"更新成功数量：{importeds.Count}  失败数量：{errors.Count}");
            if (errors.Count > 0)
            {
                builder.AppendLine("失败明细列表：");
                foreach (var item in errors)
                {
                    builder.AppendLine($"[行：{item.Item1}   列：{item.Item2}   值：{item.Item3}]");
                }
            }
            MessageBox.Show(builder.ToString(), "导入结果", MessageBoxButton.OK);
        }
        #endregion

        protected virtual void OnCapturedException(Exception e, string message, [CallerMemberName] string methodName = null)
        {
            this.BeginInvoke(() => MessageBox.Show($"{message}:\r\n{e}","异常", MessageBoxButton.OK));
        }
        protected virtual void OnCapturedMessage(string message, [CallerMemberName] string methodName = null)
        {
            this.BeginInvoke(()=>MessageBox.Show(message, "提示", MessageBoxButton.OK));
        }
        protected void BeginInvoke(Action action)
        {
            _context.Post(i => action?.Invoke(), null);
        }
        public event EventHandler IsActiveChanged;
    }
}
