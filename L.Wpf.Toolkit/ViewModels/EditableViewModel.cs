using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace System
{
    public class EditableViewModel:NotifyPropertyChanged
    {
        private bool _isEnabled;
        private bool _isEditing;
        private bool _isSelected;
        private Func<object, List<Tuple<string, object>>,Task> _saveAction;
        public EditableViewModel(object source, Func<object, List<Tuple<string, object>>,Task> saveAction)
        {
            _isEnabled = true;
            Source = source;
            _saveAction = saveAction;
        }
        public EditableViewModel() { }
        public object Source { get; }
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
            if (newIsSelected==false)
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
                }
            }
        }
        internal event EventHandler<EditableViewModelEditedEventArgs> OnEditedEvent;
    }
    public class EditableViewModel<T> : EditableViewModel
    {
        public EditableViewModel(T source, Func<T, List<Tuple<string, object>>, Task> saveAction) : base(source, (i, j) => saveAction?.Invoke((T)i, j)) { }
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
