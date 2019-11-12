using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleE.ViewModels
{
    public class ViewAViewModel : BindableBase,IActiveAware
    {
        private string _message;

        public event EventHandler IsActiveChanged;

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value,OnActiveChanged); }
        }

        private void OnActiveChanged()
        {
           
        }

        public ViewAViewModel()
        {
            Message = "View A from your Prism Module";
        }
    }
}
