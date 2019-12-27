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
        void Initialize(string enitityGenericName);
        bool CanEditItem(object item);
        bool CanDeleteItem(object item);
        void OnDeletedItem(object item);
        bool CanSaveItem(object oldItem, List<EFEditedPropertyInfo> editedPropertyInfos);
        void OnSavedItem(object newItem, List<EFEditedPropertyInfo> editedPropertyInfos);
        void OnCatchedException(Exception e, string message);
        void OnCatchedMessage(string message);
        bool IsImportIgnoreErrorItemsWhenImportedFirstErrorItem(int errorRowIndex, string errorColumnName, object value);
        void OnImportedCompleted(List<object> importedItems, List<Tuple<int, string, object>> errorItems);
    }
    public abstract class EFViewModel<TModel, TDbContext> : NotifyPropertyChanged, IEFViewModel
        where TDbContext : DbContext, new()
    {
        private Type _dbContextType;
        private Type _entityType;
        private string _entityGenericName;
        public EFViewModel()
        {
            _dbContextType = typeof(TDbContext);
            _entityType = typeof(TModel);
            OwnerWindow = Application.Current.MainWindow;
        }
        public Window OwnerWindow { get; protected set; }
        public string EntityGenericName
        {
            get { return _entityGenericName; }
            private set { SetProperty(ref _entityGenericName, value, OnEntityGenericNameChanged); }
        }
        public virtual Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> QueryExpression => i => i;
        protected virtual bool CanEditItem(TModel item) => true;
        protected virtual bool CanDeleteItem(TModel item) => MessageBox.Show(OwnerWindow,"确定删除该项?", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        protected virtual void OnDeletedItem(TModel item) { }
        protected virtual bool CanSaveItem(TModel oldItem, List<EFEditedPropertyInfo> editedPropertyInfos) => true;
        protected virtual void OnSavedItem(TModel newItem, List<EFEditedPropertyInfo> editedPropertyInfos) { }
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
            MessageBox.Show(OwnerWindow,$"{message}\r\n{e.Message}", "报警");
        }
        protected virtual void OnCatchedMessage(string message)
        {
            MessageBox.Show(OwnerWindow,$"{message}", "提示");
        }
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
        protected virtual void OnEntityGenericNameChanged(string oldEntityGenericName, string newEntityGenericName)
        {
            //
        }


        #region IEFViewModel
        Type IEFViewModel.DbContextType => _dbContextType;
        Type IEFViewModel.EntityType => _entityType;
        Linq.Expressions.Expression IEFViewModel.QueryExpression => this.QueryExpression;
        void IEFViewModel.Initialize(string enitityGenericName) => EntityGenericName = enitityGenericName;
        bool IEFViewModel.CanEditItem(object item) => CanEditItem((TModel)item);
        bool IEFViewModel.CanDeleteItem(object item) => CanDeleteItem((TModel)item);
        void IEFViewModel.OnDeletedItem(object item) => OnDeletedItem((TModel)item);
        bool IEFViewModel.CanSaveItem(object oldItem, List<EFEditedPropertyInfo> editedPropertyInfos) => CanSaveItem((TModel)oldItem, editedPropertyInfos);
        void IEFViewModel.OnSavedItem(object newItem, List<EFEditedPropertyInfo> editedPropertyInfos) => OnSavedItem((TModel)newItem, editedPropertyInfos);
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
        void IEFViewModel.OnImportedCompleted(List<object> importedItems, List<Tuple<int, string, object>> errorItems)
        {
            OnImportedComplated(importedItems.OfType<TModel>().ToList(), errorItems);
        }
        #endregion
    }

   
}
