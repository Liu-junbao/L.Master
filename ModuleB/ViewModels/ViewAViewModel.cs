using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ModuleB.ViewModels
{
    public class ViewAViewModel :BindableBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public Expression<Func<IQueryable<Model>, IQueryable<Model>>> Query => q => q.Where(i => i.Name != null).OrderBy(i=>i.Name);
    }
}
