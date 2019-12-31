using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModuleB.ViewModels
{
    public class ViewAViewModel : EFViewModel<Model,DB>
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        public override Expression<Func<IQueryable<Model>, IQueryable<Model>>> QueryExpression => q => q.OrderBy(i => i.Name);
    }
}
