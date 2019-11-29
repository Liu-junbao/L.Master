using Prism;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System
{
    public abstract class DBViewModel<TModel, TDbContext> : NotifyPropertyChanged, IActiveAware, ISourceService
        where TModel : class
        where TDbContext : DbContext, new()
    {
        private bool _isActive;
        private bool _isLoading;
        private Task _loadTask;
        private int _displayCount;
        private int _Count;
        private int _pageCount;
        private int _pageIndex;
        private EditableViewModel _selectedItem;
        public DBViewModel()
        {
            _displayCount = 50;
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
        /// <summary>
        /// 重新加载数据,会统计记录数量
        /// </summary>
        protected void LoadDataAsync()
        {
            if (_loadTask == null || _loadTask.IsCompleted)
            {
                IsLoading = true;
                try
                {
                    _loadTask = LoadAsync();
                    
                }
                catch (Exception e)
                {
                    OnCapturedException(e);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
        /// <summary>
        /// 加载当前页数据
        /// </summary>
        /// <returns></returns>
        protected void LoadPageAsync()
        {
            var pageIndex = PageIndex;
            if (_loadTask == null || _loadTask.IsCompleted)
            {
                IsLoading = true;
                try
                {
                    _loadTask = LoadPageAsync(pageIndex);
                }
                catch (Exception e)
                {
                    OnCapturedException(e);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
        private async Task LoadAsync()
        {
            await Task.Run(() =>
            {
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
                    OnCapturedException(e);
                }
                return null;
            });
            OnLoaded(Count);
        }
        private async Task LoadPageAsync(int pageIndex)
        {
            var pageSize = DisplayCount;
            if (pageIndex <= 0 || pageIndex > PageCount || pageSize <= 0) return;
            await Task.Run(() =>
            {
                try
                {
                    using (var db = new TDbContext())
                    {
                        ChangeItems(OnQuery(db.Set<TModel>()).Skip((pageIndex - 1) * pageSize).Take(pageSize));
                    }
                }
                catch (Exception e)
                {
                    OnCapturedException(e);
                }
                return null;
            });
        }
        private void ChangeItems(IEnumerable<TModel> models)
        {
            Items.Change(models.ToDictionary(i => GetKey(i)), CreateViewModelFrom, UpdateViewModelFrom);
        }
        protected virtual EditableViewModel CreateViewModelFrom(TModel model)
        {
            var viewModel =new EditableViewModel(model);
            LastlyCreatedViewModel = viewModel;
            return viewModel;
        }
        protected virtual void UpdateViewModelFrom(EditableViewModel viewModel,TModel model)
        {
            viewModel.Source = model;
        }
        protected abstract object GetKey(TModel model);
        protected virtual IQueryable<TModel> OnQuery(IQueryable<TModel> query) => query;
        protected virtual void OnCapturedException(Exception e, [CallerMemberName] string methodName = null) { }
        protected virtual void OnLoaded(int count) { }
        public async virtual Task SaveChangedPropertys(EditableViewModel viewModel, object source, Dictionary<PropertyInfo, object> changedPropertys)
        {
            TModel model = (TModel)source;
            await OnSaveChangedPropertys(viewModel, model, changedPropertys);
        }
        protected async virtual Task<bool> OnSaveChangedPropertys(EditableViewModel viewModel,TModel model, Dictionary<PropertyInfo, object> changedPropertys)
        {
            foreach (var item in changedPropertys)
            {
                item.Key.SetValue(model, item.Value);
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
                    OnCapturedException(e);
                }
                return false;
            });
            LoadPageAsync();         
            return result;
        }
        public async Task Delete(EditableViewModel viewModel, object source)
        {
            TModel model = (TModel)source;
            await OnDelete(viewModel,model);
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
                    OnCapturedException(e);
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
        public event EventHandler IsActiveChanged;
    }
}
