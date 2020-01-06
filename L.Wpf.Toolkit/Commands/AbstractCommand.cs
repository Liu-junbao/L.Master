using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System
{
    public abstract class AbstractCommand : ICommand
    {
        private SynchronizationContext _context;
        public event EventHandler CanExecuteChanged;
        public AbstractCommand()
        {
            _context = SynchronizationContext.Current ?? throw new Exception("命令只支持UI主线程创建!");
        }
        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);
        public void RaiseCanExecuteChanged()
        {
            _context.Post(i => CanExecuteChanged?.Invoke(this, null),null);
        }
    }
}
