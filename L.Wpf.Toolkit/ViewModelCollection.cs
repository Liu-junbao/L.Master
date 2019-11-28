using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace System
{
    /// <summary>
    /// 数据视图集合
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public class ViewModelCollection<TViewModel> : IEnumerable, INotifyCollectionChanged
    {
        private ObservableCollection<TViewModel> _viewModels;
        private Dictionary<object, TViewModel> _key_viewModels;
        public ViewModelCollection()
        {
            _viewModels = new ObservableCollection<TViewModel>();
            _viewModels.CollectionChanged += ViewModels_CollectionChanged;
            _key_viewModels = new Dictionary<object, TViewModel>();
        }
        /// <summary>
        /// 改变数据
        /// </summary>
        /// <typeparam name="TKey">主键</typeparam>
        /// <typeparam name="TSource">数据Model</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="onCreateViewModel">通过数据ViewModel方法</param>
        /// <param name="onUpdateExists">便利所有已存在ViewModel,进行更新操作</param>
        public void Change<TKey, TSource>(Dictionary<TKey, TSource> source, Func<TSource, TViewModel> onCreateViewModel, Action<TSource, TViewModel> onUpdateExists = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (onCreateViewModel == null) throw new ArgumentNullException(nameof(onCreateViewModel));
            //olds
            var oldPairs = _key_viewModels.Where(i => (i.Key is TKey) == false || source.ContainsKey((TKey)i.Key) == false).ToList();
            foreach (var item in oldPairs)
            {
                _key_viewModels.Remove(item.Key);
            }
            //news
            var newPairs = source.Where(i => _key_viewModels.ContainsKey(i.Key) == false).ToList();
            var newItems = new List<TViewModel>();
            foreach (var item in newPairs)
            {
                var viewModel = onCreateViewModel(item.Value);
                _key_viewModels.Add(item.Key, viewModel);
                newItems.Add(viewModel);
            }
            //exist
            if (onUpdateExists != null)
            {
                foreach (var item in _key_viewModels)
                {
                    onUpdateExists(source[(TKey)item.Key], item.Value);
                }
            }

            //update viewModels
            UIInvoke(() =>
            {
                foreach (var item in oldPairs)
                {
                    _viewModels.Remove(item.Value);
                }
                foreach (var item in newItems)
                {
                    _viewModels.Add(item);
                }
            });
        }
        /// <summary>
        /// 改变数据
        /// 数据对象为主键
        /// </summary>
        /// <typeparam name="TSource">数据Model</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="onCreateViewModel">通过数据ViewModel方法</param>
        /// <param name="onUpdateExists">便利所有已存在ViewModel,进行更新操作</param>
        public void Change<TSource>(IEnumerable<TSource> source, Func<TSource, TViewModel> onCreateViewModel, Action<TSource, TViewModel> onUpdateExists = null) => Change(source.ToDictionary(i => i), onCreateViewModel, onUpdateExists);
        public void Clear()
        {
            _key_viewModels.Clear();
            UIInvoke(() => _viewModels.Clear());
        }
        private void UIInvoke(Action action)
        {
            if (Thread.CurrentThread != Application.Current.Dispatcher.Thread)
                Application.Current.Dispatcher.BeginInvoke(action);
            else
                action();
        }
        public IEnumerator GetEnumerator()
        {
            return _key_viewModels.Values.GetEnumerator();
        }
        private void ViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }   
}
