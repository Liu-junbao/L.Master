using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterDemo.ViewModels
{
    public class Window1ViewModel : BindableBase
    {
        private List<EditableViewModel> _items;
        public Window1ViewModel()
        {

        }
        public List<EditableViewModel> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }
    }
}
