using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace System.Windows.Transitions
{
    public interface ITransitionWipeSelector
    {
        ITransitionWipe ProviderTransitionWipeFrom(FrameworkElement oldPresenter,FrameworkElement newPresenter,ITransitionContainer container);
    }
}
