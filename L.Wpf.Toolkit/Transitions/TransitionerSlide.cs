namespace System.Windows.Transitions
{
    /// <summary>
    /// Content control to host the content of an individual page within a <see cref="TransitionerSlideBox"/>.
    /// </summary>
    public class TransitionerSlide : TransitioningContentBase
    {
        public static RoutedEvent InTransitionFinished =
            EventManager.RegisterRoutedEvent(nameof(InTransitionFinished), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TransitionerSlide));
        public static readonly DependencyProperty ForwardWipeProperty =
            DependencyProperty.Register(nameof(ForwardWipe), typeof(ITransitionWipe), typeof(TransitionerSlide), new PropertyMetadata(null));
        public static readonly DependencyProperty BackwardWipeProperty =
            DependencyProperty.Register(nameof(BackwardWipe), typeof(ITransitionWipe), typeof(TransitionerSlide), new PropertyMetadata(null));
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(nameof(State), typeof(TransitionerSlideState), typeof(TransitionerSlide), new PropertyMetadata(default(TransitionerSlideState), new PropertyChangedCallback(StatePropertyChangedCallback)));
        public static readonly DependencyProperty TransitionOriginProperty =
            DependencyProperty.Register(nameof(TransitionOrigin), typeof(Point), typeof(TransitionerSlideBox), new PropertyMetadata(new Point(0.5, 0.5)));

        static TransitionerSlide()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitionerSlide), new FrameworkPropertyMetadata(typeof(TransitionerSlide)));
        }
        public Point TransitionOrigin
        {
            get => (Point)GetValue(TransitionOriginProperty);
            set => SetValue(TransitionOriginProperty, value);
        }
        protected void OnInTransitionFinished(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        private static void StatePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitionerSlide)d).AnimateToState();
        }
        public TransitionerSlideState State
        {
            get => (TransitionerSlideState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }
        public ITransitionWipe ForwardWipe
        {
            get => (ITransitionWipe)GetValue(ForwardWipeProperty);
            set => SetValue(ForwardWipeProperty, value);
        }
        public ITransitionWipe BackwardWipe
        {
            get => (ITransitionWipe)GetValue(BackwardWipeProperty);
            set => SetValue(BackwardWipeProperty, value);
        }
        private void AnimateToState()
        {
            if (State != TransitionerSlideState.Current) return;

            RunOpeningEffects();
        }
    }
}
