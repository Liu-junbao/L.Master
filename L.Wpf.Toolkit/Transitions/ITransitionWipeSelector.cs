namespace System.Windows.Transitions
{
    public interface ITransitionWipeSelector
    {
        ITransitionWipe ProviderTransitionWipeFrom(FrameworkElement oldPresenter, FrameworkElement newPresenter, ITransitionContainer container);
    }
}
