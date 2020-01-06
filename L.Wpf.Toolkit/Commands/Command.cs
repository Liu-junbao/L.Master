using System.Linq.Expressions;
using System.Threading;
using System.Windows.Input;

namespace System
{
    public class Command:AbstractCommand
    {
        private Action _execute;
        private Func<bool> _canExecute;
        public Command(Action execute) : this(execute, null) { }
        public Command(Action execute,Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public override bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }
        public override void Execute(object parameter)
        {
            _execute?.Invoke();
        }
    }
    public class Command<T> : AbstractCommand
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
        public override bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }
        public override void Execute(object parameter)
        {
            _execute?.Invoke((T)parameter);
        }
    }
}
