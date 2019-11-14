using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;

namespace System.Windows
{
    public class NewMarkupExtension : MarkupExtension
    {
        public NewMarkupExtension() { }
        public NewMarkupExtension(Type type)
        {
            Type = type;
        }
        [ConstructorArgument("type")]
        public Type Type { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Activator.CreateInstance(Type);
        }
    }
    public abstract class NewMarkupExtension<TValue> : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var args = CreateArgs();
            if (args != null)
                return Activator.CreateInstance(typeof(TValue), args);
            else
                return Activator.CreateInstance(typeof(TValue));
        }
        protected virtual object[] CreateArgs() => null;
    }
}
