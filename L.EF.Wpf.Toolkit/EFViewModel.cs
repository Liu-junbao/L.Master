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
        bool CanEditItem(object item);
        bool CanDeleteItem(object item);
        bool CanSaveItem(object item, Dictionary<string, object> changedProperties);
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
        public EFViewModel()
        {
            _dbContextType = typeof(TDbContext);
            _entityType = typeof(TModel);
        }
        Type IEFViewModel.DbContextType => _dbContextType;
        Type IEFViewModel.EntityType => _entityType;
        Linq.Expressions.Expression IEFViewModel.QueryExpression => this.QueryExpression;
        public virtual Expression<Func<IQueryable<TModel>, IQueryable<TModel>>> QueryExpression => i => i;
        protected virtual bool CanEditItem(TModel item) => true;
        protected virtual bool CanDeleteItem(TModel item) => MessageBox.Show("确定删除该项?", "提示", MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        protected virtual bool CanSaveItem(TModel item, Dictionary<string, object> changedProperties) => true;
        bool IEFViewModel.CanEditItem(object item) => CanEditItem((TModel)item);
        bool IEFViewModel.CanDeleteItem(object item) => CanDeleteItem((TModel)item);
        bool IEFViewModel.CanSaveItem(object item, Dictionary<string, object> changedProperties) => CanSaveItem((TModel)item, changedProperties);
        bool IEFViewModel.IsImportIgnoreErrorItemsWhenImportedFirstErrorItem(int errorRowIndex, string errorColumnName, object errorValue)
        {
            if (MessageBox.Show($"数据格式不正确，是否忽略所有错误行？ [行：{errorRowIndex}   列：{errorColumnName}   值：{errorValue}]", "警告", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                return true;
            }
            return false;
        }
        protected virtual bool IsImportIgnoreErrorItemsWhenImportedFirstErrorItem(int errorRowIndex, string errorColumnName, object errorValue)
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
        protected virtual void OnCatchedException(Exception e, string message)
        {
            MessageBox.Show($"{message}\\r\\n{e.Message}");
        }
        void IEFViewModel.OnCatchedMessage(string message)
        {
            OnCatchedMessage(message);
        }
        protected virtual void OnCatchedMessage(string message)
        {
            MessageBox.Show($"{message}");
        }
        void IEFViewModel.OnImportedCompleted(List<object> importedItems, List<Tuple<int, string, object>> errorItems)
        {
            OnImportedComplated(importedItems.OfType<TModel>().ToList(), errorItems);
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
    }
}
