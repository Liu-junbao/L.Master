using Prism;
using System;
using System.Collections.Generic;
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
        private List<TModel> _items;
        private int _displayCount;
        private int _Count;
        private int _pageCount;
        private int _pageIndex;
        public DBViewModel()
        {
            _displayCount = 50;
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
        public List<TModel> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }
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
            set { SetProperty(ref _pageIndex, value); }
        }
        protected virtual void OnIsActiveChanged(bool oldIsActive, bool newIsActive)
        {
            if (newIsActive)
            {
                Loading();
            }
            IsActiveChanged?.Invoke(this, null);
        }
        /// <summary>
        /// 加载数据
        /// </summary>
        protected void Loading()
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
        private async Task LoadAsync()
        {
            Items = await Task.Run(() =>
            {
                try
                {
                    using (var db = new TDbContext())
                    {
                        //计数
                        var count = OnQuery(db.Set<TModel>()).Count();
                        Count = count;
                        var pageSize = DisplayCount;
                        if (pageSize < 0) pageSize = 1;
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
                            return OnQuery(db.Set<TModel>()).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
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
        protected abstract IQueryable<TModel> OnQuery(IQueryable<TModel> query);
        protected virtual void OnCapturedException(Exception e, [CallerMemberName] string methodName = null) { }
        protected virtual void OnLoaded(int count) { }
        public async virtual Task SaveChangedPropertys(object source, Dictionary<PropertyInfo, object> changedPropertys)
        {
            TModel model = (TModel)source;
            await OnSaveChangedPropertys(model, changedPropertys);
        }
        protected async virtual Task<bool> OnSaveChangedPropertys(TModel model, Dictionary<PropertyInfo, object> changedPropertys)
        {
            foreach (var item in changedPropertys)
            {
                item.Key.SetValue(model, item.Value);
            }
            return await Task.Run(() =>
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
        }
        public abstract void OnCaptureErrorEditedValue(string propertyName, object editedValue);
        public event EventHandler IsActiveChanged;
    }
}
