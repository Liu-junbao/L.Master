using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows
{
    public interface ISourceService
    {
        void OnCaptureErrorEditedValue(EditableViewModel viewModel, object source, string sourcePropertyName, object editedValue);
        Task SaveChangedPropertys(EditableViewModel viewModel, object source, Dictionary<PropertyInfo, object> changedPropertys);
        Task Delete(EditableViewModel viewModel, object source);
    }

    /// <summary>
    /// 可编辑视图
    /// </summary>
    public class EditableViewModel : NotifyPropertyChanged
    {
        private object _source;
        private bool _isEnabled;
        private bool _isEditing;
        private bool _isEditable;
        private ISourceService _sourceService;
        public EditableViewModel()
        {
            _isEnabled = true;
        }
        public EditableViewModel(object source)
        {
            _isEnabled = true;
            _source = source;
        }
        public object Source
        {
            get { return _source; }
            set { SetProperty(ref _source, value); }
        }
        public ISourceService SourceService
        {
            get { return _sourceService; }
            set { SetProperty(ref _sourceService, value); }
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
        public bool IsEditable
        {
            get { return _isEditable; }
            set { SetProperty(ref _isEditable, value, OnIsEditableChanged); }
        }
        protected virtual void OnIsEditableChanged(bool oldIsSelected, bool newIsSelected)
        {
            //
            if (newIsSelected == false)
            {
                IsEditing = false;
            }
        }
        public async Task RaiseSaveAsync()
        {
            var sourceService = SourceService;
            if (sourceService != null)
            {
                var arg = new EditableViewModelSavingEventArgs(new Dictionary<string, object>());
                SavingEvent?.Invoke(this, arg);
                IsEnabled = false;
                try
                {
                    var source = this.Source;
                    Dictionary<PropertyInfo, object> changedPropertyValues;
                    if (CheckValues(sourceService, source, arg.EditedValues, out changedPropertyValues))
                    {
                        await sourceService.SaveChangedPropertys(this, source, changedPropertyValues);
                    }
                }
                finally
                {
                    IsEnabled = true;
                    IsEditing = false;
                }
                SavedEvent?.Invoke(this, null);
            }
        }     
        public async Task RaiseDelecteAsync()
        {
            var sourceService = SourceService;
            if (sourceService != null)
            {
                IsEnabled = false;
                try
                {
                    await sourceService.Delete(this,this.Source);
                }
                finally
                {
                    IsEnabled = true;
                    IsEditing = false;
                }
            }
        }
        private bool CheckValues(ISourceService sourceService, object source, Dictionary<string, object> changedValues, out Dictionary<PropertyInfo, object> changedPropertyValues)
        {
            changedPropertyValues = null;
            if (source == null) return false;
            var type = source.GetType();
            changedPropertyValues = new Dictionary<PropertyInfo, object>();
            foreach (var item in changedValues)
            {
                var property = type.GetProperty(item.Key);
                if (property != null)
                {
                    object value;
                    if (TryConvert(item.Value, property.PropertyType, out value))
                    {
                        changedPropertyValues.Add(property, value);
                    }
                    else
                    {
                        sourceService.OnCaptureErrorEditedValue(this, source, item.Key, item.Value);
                        return false;
                    }
                }
            }
            return changedPropertyValues.Count > 0;
        }
        private bool TryConvert(object value, Type type, out object result)
        {
            result = null;
            try
            {
                result = Convert.ChangeType(value, type);
                return true;
            }
            catch { }
            return false;
        }
        public override string ToString()
        {
            return $"IsEnabled:{_isEnabled} IsEditing:{_isEditing} IsEditable:{_isEditable} Source:{Source}";
        }
        public event EventHandler<EditableViewModelSavingEventArgs> SavingEvent;
        public event EventHandler SavedEvent;
    }
    public class EditableViewModelSavingEventArgs : EventArgs
    {
        public EditableViewModelSavingEventArgs(Dictionary<string, object> editedValues)
        {
            EditedValues = editedValues;
        }
        public Dictionary<string, object> EditedValues { get; }
    }
   
    public static class EditableViewModelAssist
    {
        internal static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.RegisterAttached("IsEditable", typeof(bool), typeof(EditableViewModelAssist), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty SourceServiceProperty =
            DependencyProperty.RegisterAttached("SourceService", typeof(ISourceService), typeof(EditableViewModelAssist), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        internal static bool GetIsEditable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEditableProperty);
        }
        internal static void SetIsEditable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEditableProperty, value);
        }

        public static ISourceService GetSourceService(DependencyObject obj)
        {
            return (ISourceService)obj.GetValue(SourceServiceProperty);
        }
        public static void SetSourceService(DependencyObject obj, ISourceService value)
        {
            obj.SetValue(SourceServiceProperty, value);
        }
    }
}
