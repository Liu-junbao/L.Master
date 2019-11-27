using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MasterDemo.ViewModels
{
    public class Window1ViewModel : DBViewModel<STA_Alarm, SCADAEntities>
    {
        public Window1ViewModel()
        {

            Loading();
        }
        public override void OnCaptureErrorEditedValue(string propertyName, object editedValue)
        {
            
        }

        protected override IQueryable<STA_Alarm> OnQuery(IQueryable<STA_Alarm> query)
        {
            return query.OrderBy(i => i.DeviceName);
        }
    }
}
