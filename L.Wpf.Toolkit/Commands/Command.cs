using System.Windows.Input;

namespace System
{
    public class Command:ICommand
    {
        private Action _execute;
        private Func<bool> _canExecute;
        public Command(Action execute)
        {
            _execute = execute;
        }
        public Command(Action execute,Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke();
        }
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this,null);
        }
        public event EventHandler CanExecuteChanged;
    }
    public class Command<T>:ICommand
    {
        private Action<T> _execute;
        private Func<T, bool> _canExecute;
        public Command(Action<T> execute)
        {
            _execute = execute;
        }
        public Command(Action<T> execute,Func<T,bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }
        public void Execute(object parameter)
        {
            _execute?.Invoke((T)parameter);
        }
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, null);
        }
        public event EventHandler CanExecuteChanged;
    }
}
