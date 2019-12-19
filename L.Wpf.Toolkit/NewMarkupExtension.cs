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
        where TValue : new()
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            TValue value;
            var args = CreateArgs();
            if (args != null)
                value = (TValue)Activator.CreateInstance(typeof(TValue), args);
            else
                value = new TValue();
            OnInitialize(value);
            return value;
        }
        protected virtual object[] CreateArgs() => null;
        protected virtual void OnInitialize(TValue value) { }
    }
}
