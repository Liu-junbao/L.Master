using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;

namespace System
{
    public interface IEFViewModel
    {
        Type DbContextType { get; }
        Type EntityType { get; }
        Linq.Expressions.Expression QueryExpression { get; }
        Action RefreshAction { set; }
        void Initialize(string enitityGenericName);
        bool CanEditItem(object item);
        bool CanDeleteItem(object item);
        void OnDeletedItem(object item);
        bool CanSaveItem(bool isAdded, object oldItem, List<EFEditedPropertyInfo> editedPropertyInfos);
        void OnSavedItem(bool isAdded, object newItem, List<EFEditedPropertyInfo> editedPropertyInfos);
        void OnCatchedException(Exception e, string message);
        void OnCatchedMessage(string message);
        bool IsImportIgnoreErrorItemsWhenImportedFirstErrorItem(int errorRowIndex, string errorColumnName, object value);
        void OnImporting(object item, bool isNewItem);
        void OnImportedCompleted(List<object> importedItems, List<Tuple<int, string, object>> errorItems);
    }
    public abstract class EFViewModel<TModel, TDbContext> : NotifyPropertyChanged, IEFViewModel
        where TDbContext : DbContext, new()
    {
        private Type _dbContextType;
        private Type _entityType;
        private string _entityGenericName;
        private readonly System.Threading.SynchronizationContext _context;
        private Action _refreshAction;
        public EFViewModel()
        {
            _dbContextType = typeof(TDbContext);
            _entityType = typeof(TModel);
            _context = Threading.SynchronizationContext.Current ?? throw new Exception("ViewModel只能在UI单线程中初始化!");
            OwnerWindow = Application.Current.MainWindow;
        }
        public Window OwnerWindow { get; protected set; }  
        public string EntityGenericName
        {
            get { return _entityGenericName; }
            private set { SetProperty(ref _entityGenericName, value, OnEntityGenericNameChanged); }
        }
        public virtual Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> QueryExpression => i => i;
        protected void Refresh()
        {
            _refreshAction?.Invoke();
        }
        protected virtual bool CanEditItem(TModel item) => true;
        protected virtual bool CanDeleteItem(TModel item) => MessageBox.Show(OwnerWindow, "确定删除该项?", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        protected virtual void OnDeletedItem(TModel item) { }
        protected virtual bool CanSaveItem(bool isAdded, TModel oldItem, List<EFEditedPropertyInfo> editedPropertyInfos) => true;
        protected virtual void OnSavedItem(bool isAdded, TModel newItem, List<EFEditedPropertyInfo> editedPropertyInfos) { }
        protected virtual bool IsImportIgnoreErrorItemsWhenImportedFirstErrorItem(int errorRowIndex, string errorColumnName, object errorValue)
        {
            if (MessageBox.Show($"数据格式不正确，是否忽略所有错误行？ [行：{errorRowIndex}   列：{errorColumnName}   值：{errorValue}]", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                return true;
            }
            return false;
        }
        protected virtual void OnCatchedException(Exception e, string message)
        {
            BeginInvoke(() => MessageBox.Show(OwnerWindow, $"{message}\r\n{e.Message}", "报警"));
        }
        protected virtual void OnCatchedMessage(string message)
        {
            BeginInvoke(() => MessageBox.Show(OwnerWindow, $"{message}", "提示"));
        }
        protected virtual void OnImporting(TModel model, bool isNewItem) { }
        protected virtual void OnImportedComplated(List<TModel> importedItems, List<Tuple<int, string, object>> errorItems)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"更新成功数量：{importedItems.Count}  失败数量：{errorItems.Count}");
            if (errorItems.Count > 0)
            {
                builder.AppendLine("失败明细列表：");
                foreach (var item in errorItems)
                {
                    builder.AppendLine($"[行：{item.Item1}   列：{item.Item2}   值：{item.Item3}]");
                }
            }
            MessageBox.Show(builder.ToString(), "导入结果", MessageBoxButton.OK);
        }
        protected virtual void OnEntityGenericNameChanged(string oldEntityGenericName, string newEntityGenericName) { }
        protected void BeginInvoke(Action action)
        {
            if (action == null) return;
            _context.Post(i => action.Invoke(), null);
        }

        #region IEFViewModel
        Type IEFViewModel.DbContextType => _dbContextType;
        Type IEFViewModel.EntityType => _entityType;
        Action IEFViewModel.RefreshAction { set => _refreshAction = value; }
        Linq.Expressions.Expression IEFViewModel.QueryExpression => this.QueryExpression;
        void IEFViewModel.Initialize(string enitityGenericName) => EntityGenericName = enitityGenericName;
        bool IEFViewModel.CanEditItem(object item) => CanEditItem((TModel)item);
        bool IEFViewModel.CanDeleteItem(object item) => CanDeleteItem((TModel)item);
        void IEFViewModel.OnDeletedItem(object item) => OnDeletedItem((TModel)item);
        bool IEFViewModel.CanSaveItem(bool isAdded, object oldItem, List<EFEditedPropertyInfo> editedPropertyInfos) => CanSaveItem(isAdded, (TModel)oldItem, editedPropertyInfos);
        void IEFViewModel.OnSavedItem(bool isAdded, object newItem, List<EFEditedPropertyInfo> editedPropertyInfos) => OnSavedItem(isAdded, (TModel)newItem, editedPropertyInfos);
        bool IEFViewModel.IsImportIgnoreErrorItemsWhenImportedFirstErrorItem(int errorRowIndex, string errorColumnName, object errorValue)
        {
            if (MessageBox.Show($"数据格式不正确，是否忽略所有错误行？ [行：{errorRowIndex}   列：{errorColumnName}   值：{errorValue}]", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                return true;
            }
            return false;
        }
        void IEFViewModel.OnCatchedException(Exception e, string message)
        {
            OnCatchedException(e, message);
        }
        void IEFViewModel.OnCatchedMessage(string message)
        {
            OnCatchedMessage(message);
        }
        void IEFViewModel.OnImporting(object item, bool isNewItem) => OnImporting((TModel)item, isNewItem);
        void IEFViewModel.OnImportedCompleted(List<object> importedItems, List<Tuple<int, string, object>> errorItems)
        {
            OnImportedComplated(importedItems.OfType<TModel>().ToList(), errorItems);
        }
        #endregion
    }

   
}
