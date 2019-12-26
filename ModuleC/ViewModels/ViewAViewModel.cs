using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ModuleC.ViewModels
{
    public class ViewAViewModel : NettyServer
    {
        public ViewAViewModel() : base(8730)
        {

        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        protected override void OnLogIn(Channel user)
        {
            base.OnLogIn(user);
        }
        protected override void OnLogOut(Channel user)
        {
            base.OnLogOut(user);
        }

        protected override void OnMessage(Channel user, object message)
        {
            base.OnMessage(user, message);
            Message = message?.ToString();
        }

       

        #region INotifyPropertyChanged
        protected void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            if (propertyNames != null)
            {
                foreach (var item in propertyNames)
                {
                    RaisePropertyChanged(item);
                }
            }
        }
        protected virtual bool SetProperty<TValue>(ref TValue storage, TValue newValue, [CallerMemberName] string propertyName = null, params string[] propertyNameArgs)
        {
            if (EqualityComparer<TValue>.Default.Equals(storage, newValue) == false)
            {
                TValue oldValue = storage;
                storage = newValue;
                OnPropertyChanged(propertyName, oldValue, newValue);
                this.RaisePropertyChanged(propertyName);
                this.RaisePropertyChanged(propertyNameArgs);
                return true;
            }
            return false;
        }
        protected virtual bool SetProperty<TValue>(ref TValue storage, TValue newValue, Action<TValue, TValue> onChanged, [CallerMemberName] string propertyName = null, params string[] propertyNameArgs)
        {
            if (EqualityComparer<TValue>.Default.Equals(storage, newValue) == false)
            {
                TValue oldValue = storage;
                storage = newValue;
                onChanged?.Invoke(oldValue, newValue);
                OnPropertyChanged(propertyName, oldValue, newValue);
                this.RaisePropertyChanged(propertyName);
                this.RaisePropertyChanged(propertyNameArgs);
                return true;
            }
            return false;
        }
        protected virtual void OnPropertyChanged(string propertyName, object oldValue, object newValue) { }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


    }
}
