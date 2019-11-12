using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Prism
{
    public class InstanceExtenions<TValue> : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var value = this.GetInstance<TValue>();
            if(value!=null)
                OnInitialize(value);
            return value;
        }
        protected virtual void OnInitialize(TValue value) { }
    }
}
