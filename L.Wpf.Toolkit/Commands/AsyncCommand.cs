using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System
{
    public class AsyncCommand : AbstractCommand
    {
        private Func<Task> _execute;
        private Func<bool> _canExecute;
        private bool _isBusy;
        private int _timeoutMilliseconds;
        private Action _timeout;
        public AsyncCommand(Func<Task> execute, int timeoutMilliseconds = 3000, Action timeout = null)
        {
            _timeoutMilliseconds = timeoutMilliseconds;
            _timeout = timeout;
            _execute = execute;
        }
        public AsyncCommand(Func<Task> execute, Func<bool> canExecute, int timeoutMilliseconds = 3000, Action timeout = null)
        {
            _timeoutMilliseconds = timeoutMilliseconds;
            _timeout = timeout;
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
                    var delay = Task.Delay(_timeoutMilliseconds);
                    var comp = await Task.WhenAny(delay, _execute.Invoke());
                    if (comp == delay)
                        _timeout?.Invoke();
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
        private Func<T, Task> _execute;
        private Func<T, bool> _canExecute;
        private bool _isBusy;
        private int _timeoutMilliseconds;
        private Action<T> _timeout;
        public AsyncCommand(Func<T, Task> execute, int timeoutMilliseconds = 3000, Action<T> timeout = null)
        {
            _timeoutMilliseconds = timeoutMilliseconds;
            _timeout = timeout;
            _execute = execute;
        }
        public AsyncCommand(Func<T, Task> execute, Func<T, bool> canExecute, int timeoutMilliseconds = 3000, Action<T> timeout = null)
        {
            _timeoutMilliseconds = timeoutMilliseconds;
            _timeout = timeout;
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
                    var model = (T)parameter;
                    var delay = Task.Delay(_timeoutMilliseconds);
                    var comp = await Task.WhenAny(delay, _execute.Invoke(model));
                    if (comp == delay)
                        _timeout?.Invoke(model);
                }
                finally
                {
                    _isBusy = false;
                }
            }
        }
    }
}
