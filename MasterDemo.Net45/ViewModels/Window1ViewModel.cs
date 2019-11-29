using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace MasterDemo.ViewModels
{
    public class Window1ViewModel : DBViewModel<INF_Scanned, SCADAEntities>
    {
        public Window1ViewModel()
        {
            LoadData();
        }
        private async void LoadData()
        {
           LoadDataAsync();
        }
        protected override IQueryable<INF_Scanned> OnQuery(IQueryable<INF_Scanned> query)
        {
            return base.OnQuery(query).OrderBy(i=>i.ScannedTime);
        }
       
        protected override void OnCapturedException(Exception e, [CallerMemberName] string methodName = null)
        {
            base.OnCapturedException(e, methodName);
        }

        protected override object GetKey(INF_Scanned model) => model.ID;
    }
}
