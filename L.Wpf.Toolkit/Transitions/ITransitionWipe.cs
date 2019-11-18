namespace System.Windows.Transitions
{
    public interface ITransitionWipe
    {
        void Wipe(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container);
    }
}