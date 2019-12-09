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
    public class ViewAViewModel : DBViewModel<Model, DB>
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        protected override Expression<Func<Model, object>> KeyExpression => i => i.Name;

        public ViewAViewModel()
        {
            Message = "View B from your Prism Module";
            LoadDataAsync();
        }
        protected override IQueryable<Model> OnQuery(IQueryable<Model> query)
        {
            return base.OnQuery(query).OrderBy(i => i.Name);
        }
        protected override void OnCapturedException(Exception e, string message, [CallerMemberName] string methodName = null)
        {
            base.OnCapturedException(e, message, methodName);
        }

    }
}
