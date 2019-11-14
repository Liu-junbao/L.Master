using System;
using System.Windows;

namespace System.Windows.Transitions
{
    public interface ITransitionContainer
    {
        double ActualWidth { get; }
        double ActualHeight { get; }
        void SetZIndexOrderBy(params FrameworkElement[] presenters);
        void OnCompletedTransition();
        int Indexof(FrameworkElement presenter);
        object ItemFromContainer(FrameworkElement presenter);
        event EventHandler TransitionChanged;
    }
}