using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModuleA.ViewModels
{
    public class ViewAViewModel : BindableBase
    {
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public ViewAViewModel()
        {
            Message = "View A from your Prism Module";


            
        }


        public bool And(bool a, bool b)
        {
            bool result = true;
            if (a == false)
            {
                result = false;
            }
            if (b == true)
            {
                result = false;
            }
            return result;
        }
        public bool AndAlso(bool a, bool b)
        {
            if (a == true)
            {
                if (b == true)
                {
                    return true;
                }
            }
            return false;
        }
        public bool OrElse(bool a, bool b)
        {
            if (a == true)
            {
                return true;
            }
            else if (b == true)
            {
                return true;
            }
            return false;
        }
        public bool Or(bool a, bool b)
        {
            bool result = false;
            if (a == true)
            {
                result = true;
            }
            if (b == true)
            {
                result = true;
            }
            return result;
        }
    }


}
