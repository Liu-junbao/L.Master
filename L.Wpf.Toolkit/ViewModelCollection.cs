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
        /// <typeparam name="TSource">数据模型</typeparam>
        /// <param name="keySources">数据源（键值对）</param>
        /// <param name="onCreateViewModel">创建视图方法</param>
        /// <param name="onUpdateExsitsVeiwModel">更新已经存在的数据</param>
        public void Change<TSource>(Dictionary<object, TSource> keySources, Func<TSource, TViewModel> onCreateViewModel, Action<TViewModel, TSource> onUpdateExsitsVeiwModel = null)
        {
            if (keySources == null) throw new ArgumentNullException(nameof(keySources));
            if (onCreateViewModel == null) throw new ArgumentNullException(nameof(onCreateViewModel));
            //olds
            var oldItems = _key_viewModels.Where(i => keySources.ContainsKey(i.Key) == false).ToList();
            foreach (var item in oldItems)
            {
                _key_viewModels.Remove(item.Key);
            }
            UIInvoke(() =>
            {
                foreach (var item in oldItems)
                {
                    _viewModels.Remove(item.Value);
                }
            });

            //news
            int sourceIndex = -1;
            foreach (var item in keySources)
            {
                sourceIndex++;
                var index = sourceIndex;
                if (_key_viewModels.ContainsKey(item.Key) == false)
                {
                    var model = item.Value;
                    var viewModel = onCreateViewModel(item.Value);
                    _key_viewModels.Add(item.Key, viewModel);
                    UIInvoke(() =>
                    {
                        if (_viewModels.Count > index)
                            _viewModels.Insert(index, viewModel);
                        else
                            _viewModels.Add(viewModel);
                    });
                }
                else
                {
                    var model = item.Value;
                    var viewModel = _key_viewModels[item.Key];
                    UIInvoke(() =>
                    {
                        onUpdateExsitsVeiwModel?.Invoke(viewModel, model);//必须UI线程
                        var oldIndex = _viewModels.IndexOf(viewModel);
                        if (index != oldIndex)
                        {
                            _viewModels.Move(oldIndex, index);
                        }
                    });
                }
            }
        }
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
