﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace MasterDemo.ViewModels
{
    public class Window1ViewModel : DBViewModel<INF_Scanned, SCADAEntities>
    {
        public Window1ViewModel()
        {
            Loading();
        }
        
        protected override IQueryable<INF_Scanned> OnQuery(IQueryable<INF_Scanned> query)
        {
            return base.OnQuery(query).OrderBy(i=>i.ScannedTime);
        }
        public override void OnCaptureErrorEditedValue(string propertyName, object editedValue)
        {

        }
        protected override void OnCapturedException(Exception e, [CallerMemberName] string methodName = null)
        {
            base.OnCapturedException(e, methodName);
        }
    }
}
