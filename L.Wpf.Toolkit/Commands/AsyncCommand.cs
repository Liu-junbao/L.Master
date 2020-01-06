using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System
{
    public class AsyncCommand:AbstractCommand
    {
        private Func<Task> _execute;
        private Func<bool> _canExecute;
        private bool _isBusy;
        public AsyncCommand(Func<Task> execute)
        {
            _execute = execute;
        }
        public AsyncCommand(Func<Task> execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public override bool CanExecute(object parameter)
        {
            return (_canExecute?.Invoke() ?? true) && _isBusy == false;
        }
        public override async void Execute(object parameter)
        {
            if (_execute != null)
            {
                _isBusy = true;
                try
                {
                    await _execute.Invoke();
                }
                finally
                {
                    _isBusy = false;
                }
            }
        }
    }

    public class AsyncCommand<T> : AbstractCommand
    {
        private Func<T,Task> _execute;
        private Func<T,bool> _canExecute;
        private bool _isBusy;
        public AsyncCommand(Func<T,Task> execute)
        {
            _execute = execute;
        }
        public AsyncCommand(Func<T,Task> execute, Func<T,bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public override bool CanExecute(object parameter)
        {
            return (_canExecute?.Invoke((T)parameter) ?? true) && _isBusy == false;
        }
        public async override void Execute(object parameter)
        {
            if (_execute != null)
            {
                _isBusy = true;
                try
                {
                    await _execute.Invoke((T)parameter);
                }
                finally
                {
                    _isBusy = false;
                }
            }
        }
    }
}
