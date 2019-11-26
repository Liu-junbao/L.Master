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
    public class EditableCollection<TModel> : IEnumerable, INotifyCollectionChanged
    {
        private ObservableCollection<EditableViewModel> _viewModels;
        private Dictionary<object, EditableViewModel> _key_viewModels;
        private Func<TModel, List<Tuple<string, object>>, Task> _saveAction;
        public EditableCollection(Func<TModel, List<Tuple<string, object>>, Task> saveAction = null)
        {
            _viewModels = new ObservableCollection<EditableViewModel>();
            _viewModels.CollectionChanged += ViewModels_CollectionChanged;
            _key_viewModels = new Dictionary<object, EditableViewModel>();
            _saveAction = saveAction;
        }
        public void Change<TKey>(Dictionary<TKey, TModel> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            //olds
            var oldPairs = _key_viewModels.Where(i => (i.Key is TKey) == false || source.ContainsKey((TKey)i.Key) == false).ToList();
            foreach (var item in oldPairs)
            {
                _key_viewModels.Remove(item.Key);
            }
            //news
            var newKeys = source.Where(i => _key_viewModels.ContainsKey(i.Key) == false).ToList();
            var newItems = new List<EditableViewModel>();
            foreach (var item in newKeys)
            {
                var viewModel = new EditableViewModel(OnSaveItem);
                _key_viewModels.Add(item.Key, viewModel);
                newItems.Add(viewModel);
            }
            //exist
            foreach (var item in _key_viewModels)
            {
                item.Value.Source = source[(TKey)item.Key];
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
        public void Change(IEnumerable<TModel> models) => Change(models.ToDictionary(i => i));
        private Task OnSaveItem(object source, List<Tuple<string, object>> changeValues)
        {
            return _saveAction?.Invoke((TModel)source, changeValues);
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
    public class EditableViewModel : NotifyPropertyChanged
    {
        private object _source;
        private bool _isEnabled;
        private bool _isEditing;
        private bool _isSelected;
        private Func<object, List<Tuple<string, object>>, Task> _saveAction;
        public EditableViewModel(Func<object, List<Tuple<string, object>>, Task> saveAction)
        {
            _isEnabled = true;
            _saveAction = saveAction;
        }
        public object Source
        {
            get { return _source; }
            set { SetProperty(ref _source, value); }
        }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }
        public bool IsEditing
        {
            get { return _isEditing; }
            set { SetProperty(ref _isEditing, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value, OnIsSelectedChanged); }
        }
        protected virtual void OnIsSelectedChanged(bool oldIsSelected, bool newIsSelected)
        {
            //
            if (newIsSelected == false)
            {
                IsEditing = false;
            }
        }
        internal async Task RaiseSaveAsync()
        {
            var arg = new EditableViewModelEditedEventArgs(new List<Tuple<string, object>>());
            OnEditedEvent?.Invoke(this, arg);
            if (_saveAction != null)
            {
                IsEnabled = false;
                try
                {
                    await _saveAction.Invoke(this.Source, arg.EditedValues);
                }
                finally
                {
                    IsEnabled = true;
                    IsEditing = false;
                }
            }
        }
        internal event EventHandler<EditableViewModelEditedEventArgs> OnEditedEvent;
    }
    internal class EditableViewModelEditedEventArgs : EventArgs
    {
        public EditableViewModelEditedEventArgs(List<Tuple<string, object>> editedValues)
        {
            EditedValues = editedValues;
        }
        public List<Tuple<string, object>> EditedValues { get; }
    }
}
