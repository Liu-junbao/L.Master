using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
    /// 视图字典
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged, IList<TValue>, IEnumerable<TValue>
        where TValue : class
    {
        private InnerObservableDictionary<TKey, TValue> _inner;
        private SynchronizationContext _context;
        public ObservableDictionary()
        {
            _context = SynchronizationContext.Current ?? throw new Exception("只能UI主线程创建该对象!");
            _inner = new InnerObservableDictionary<TKey, TValue>();
            _inner.CollectionChanged += Inner_CollectionChanged;
            ((INotifyPropertyChanged)_inner).PropertyChanged += Inner_PropertyChanged;
        }

        /// <summary>
        /// 改变数据(必须UI线程)
        /// </summary>
        /// <typeparam name="TSource">数据模型</typeparam>
        /// <param name="sources">数据源（键值对）</param>
        /// <param name="getValue">创建视图方法</param>
        /// <param name="onUpdateExsitsVeiwModel">更新已经存在的数据</param>
        public void SetSource<TSource>(Dictionary<TKey, TSource> sources, Func<TSource, TValue, TValue> getValue)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            if (getValue == null) throw new ArgumentNullException(nameof(getValue));

            //olds
            var oldItems = ((IEnumerable<KeyValuePair<TKey, TValue>>)this).Where(i => sources.ContainsKey(i.Key) == false).ToList();
            foreach (var item in oldItems)
            {
                this.Remove(item.Key);
            }

            //update
            int sourceIndex = 0;
            foreach (var item in sources)
            {
                var key = item.Key;
                var source = item.Value;
                if (this.ContainsKey(key) == false)
                {
                    var value = getValue(source, null);
                    if (sourceIndex >= Count)
                        this.Add(key, value);
                    else
                        this.Insert(sourceIndex,key,value);
                }
                else
                {
                    var oldValue = this[key];
                    var value = getValue(source, oldValue);
                    this[key] = value;
                }
                sourceIndex++;
            }
        }
        /// <summary>
        /// 改变数据(UI线程安全的)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="sources"></param>
        /// <param name="getKey"></param>
        /// <param name="getValue"></param>
        public void SetSourceAsync<TSource>(IEnumerable<TSource> sources, Func<TSource, TKey> getKey, Func<TSource, TValue, TValue> getValue)
        {
            var existKeys = new List<TKey>();
            int sourceIndex = 0;
            var count = Count;
            foreach (var source in sources)
            {
                var key = getKey(source);
                if (this.ContainsKey(key) == false)
                {
                    var value = getValue(source, null);
                    if (sourceIndex >= count)
                        this.BeginInvoke(() => this.Add(key, value));
                    else
                        this.BeginInvoke(() => this.Insert(sourceIndex, key, value));
                }
                else
                {
                    var oldValue = this[key];
                    var value = getValue(source, oldValue);
                    this.BeginInvoke(() => this[key] = value);
                }
                sourceIndex++;
            }
        }
        private void BeginInvoke(Action action)
        {
            _context.Post(i => action(), null);
        }

        #region Dictonary
        public int Count => _inner.Count;
        public TValue this[TKey key]
        {
            get => _inner[key];
            set => _inner[key] = value;
        }
        public ICollection<TKey> Keys => _inner.Keys;
        public ICollection<TValue> Values => _inner.Values;
        public bool ContainsKey(TKey key) => _inner.ContainsKey(key);
        public void Add(TKey key, TValue value) => _inner.Add(key, value);
        public void Insert(int index, TKey key, TValue value) => _inner.Insert(index, key, value);
        public bool Remove(TKey key) => _inner.Remove(key);
        public bool TryGetValue(TKey key, out TValue value) => _inner.TryGetValue(key, out value);
        public void Clear() => _inner.ClearValues();
        #region 
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).Add(item);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).Contains(item);
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).CopyTo(array, arrayIndex);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).Remove(item);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
        #endregion

        #endregion

        #region list
        bool ICollection<TValue>.IsReadOnly => false;
        TValue IList<TValue>.this[int index]
        {
            get => _inner[index];
            set => _inner.SetIndexItem(index, value);
        }
        public int IndexOf(TValue item) => _inner.IndexOf(item);
        void IList<TValue>.Insert(int index, TValue item)
        {
            throw new Exception("不支持无键添加元素!");
        }
        public void RemoveAt(int index) => _inner.RemoveValueAt(index);
        void ICollection<TValue>.Add(TValue item)
        {
            throw new Exception("不支持无键添加元素!");
        }
        public bool Contains(TValue item) => _inner.Contains(item);

        void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
        {
            throw new Exception("不支持无键添加元素!");
        }
        public bool Remove(TValue item) => _inner.RemoveValue(item);
        public IEnumerator<TValue> GetEnumerator() => _inner.GetEnumerator();
        #endregion

        #region NotifyPropertyChanged
        private void Inner_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
        private void Inner_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
    class InnerObservableDictionary<TKey, TValue> : ObservableCollection<TValue>, IDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> _values;
        private List<TKey> _keys;
        public InnerObservableDictionary()
        {
            _values = new Dictionary<TKey, TValue>();
            _keys = new List<TKey>();
        }
        public TValue this[TKey key]
        {
            get => _values[key];
            set
            {
                var index = _keys.IndexOf(key);
                _values[key] = value;
                SetItem(index, value);
            }
        }
        public ICollection<TKey> Keys => _values.Keys;
        public ICollection<TValue> Values => _values.Values;
        public void SetIndexItem(int index, TValue value)
        {
            var key = _keys[index];
            _values[key] = value;
            SetItem(index, value);
        } 
        public void ClearValues()
        {
            _values.Clear();
            _keys.Clear();
            this.Clear();
        }
        public bool ContainsKey(TKey key) => _values.ContainsKey(key);
        public void Add(TKey key, TValue value)
        {
            _values.Add(key, value);
            _keys.Add(key);
            this.Add(value);
        }
        public void Insert(int index, TKey key, TValue value)
        {
            _values.Add(key,value);
            _keys.Insert(index,key);
            this.Add(value);
        }
        public bool Remove(TKey key)
        {
            if (_values.Remove(key))
            {
                var index = _keys.IndexOf(key);
                RemoveAt(index);
                return true;
            }
            return false;
        }
        public bool RemoveValueAt(int index)
        {
            if (_keys.Count > index)
            {
                var key = _keys[index];
                _keys.RemoveAt(index);
                _values.Remove(key);
                RemoveAt(index);
                return true;
            }
            return false;
        }
        public bool RemoveValue(TValue value)
        {
            var index = IndexOf(value);
            return RemoveValueAt(index);
        }
        public bool TryGetValue(TKey key, out TValue value) => _values.TryGetValue(key, out value);       
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_values).Add(item);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_values).Contains(item);
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_values).CopyTo(array, arrayIndex);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_values).Remove(item);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => ((ICollection<KeyValuePair<TKey, TValue>>)_values).GetEnumerator();
    }
}
