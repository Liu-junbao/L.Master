using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace System.Windows.Transitions
{
    /// <summary>
    /// The transitioner provides an easy way to move between content with a default in-place circular transition.
    /// </summary>
    public class TransitionerSlideBox : Selector, ITransitionContainer
    {
        private Point? _nextTransitionOrigin;

        static TransitionerSlideBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitionerSlideBox), new FrameworkPropertyMetadata(typeof(TransitionerSlideBox)));
        }

        /// <summary>
        /// Causes the the next slide to be displayed (affectively increments <see cref="Selector.SelectedIndex"/>).
        /// </summary>
        public static RoutedCommand MoveNextCommand = new RoutedCommand();

        /// <summary>
        /// Causes the the previous slide to be displayed (affectively decrements <see cref="Selector.SelectedIndex"/>).
        /// </summary>
        public static RoutedCommand MovePreviousCommand = new RoutedCommand();

        /// <summary>
        /// Moves to the first slide.
        /// </summary>
        public static RoutedCommand MoveFirstCommand = new RoutedCommand();

        /// <summary>
        /// Moves to the last slide.
        /// </summary>
        public static RoutedCommand MoveLastCommand = new RoutedCommand();

        public static readonly DependencyProperty AutoApplyTransitionOriginsProperty = DependencyProperty.Register(
            nameof(AutoApplyTransitionOrigins), typeof(bool), typeof(TransitionerSlideBox), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty MaskContentProperty =
            DependencyProperty.Register(nameof(MaskContent), typeof(object), typeof(TransitionerSlideBox), new PropertyMetadata(null));
        public static readonly DependencyProperty DefaultTransitionOriginProperty = DependencyProperty.Register(
            nameof(DefaultTransitionOrigin), typeof(Point), typeof(TransitionerSlideBox), new PropertyMetadata(new Point(0.5, 0.5)));
        public static readonly DependencyProperty TransitionWipeSelectorProperty =
          DependencyProperty.Register(nameof(TransitionWipeSelector), typeof(ITransitionWipeSelector), typeof(TransitionerSlideBox), new PropertyMetadata(null));

        public Point DefaultTransitionOrigin
        {
            get => (Point)GetValue(DefaultTransitionOriginProperty);
            set => SetValue(DefaultTransitionOriginProperty, value);
        }

        public TransitionerSlideBox()
        {
            CommandBindings.Add(new CommandBinding(MoveNextCommand, MoveNextHandler));
            CommandBindings.Add(new CommandBinding(MovePreviousCommand, MovePreviousHandler));
            CommandBindings.Add(new CommandBinding(MoveFirstCommand, MoveFirstHandler));
            CommandBindings.Add(new CommandBinding(MoveLastCommand, MoveLastHandler));
            AddHandler(TransitionerSlide.InTransitionFinished, new RoutedEventHandler(IsTransitionFinishedHandler));
            Loaded += (sender, args) =>
            {
                if (SelectedIndex != -1)
                    ActivateFrame(SelectedIndex, -1);
            };
        }
        public object MaskContent
        {
            get { return (object)GetValue(MaskContentProperty); }
            set { SetValue(MaskContentProperty, value); }
        }
        /// <summary>
        /// If enabled, transition origins will be applied to wipes, according to where a transition was triggered from.  For example, the mouse point where a user clicks a button.
        /// </summary>
        public bool AutoApplyTransitionOrigins
        {
            get => (bool)GetValue(AutoApplyTransitionOriginsProperty);
            set => SetValue(AutoApplyTransitionOriginsProperty, value);
        }
        public ITransitionWipeSelector TransitionWipeSelector
        {
            get { return (ITransitionWipeSelector)GetValue(TransitionWipeSelectorProperty); }
            set { SetValue(TransitionWipeSelectorProperty, value); }
        }
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TransitionerSlide;
        }
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TransitionerSlide();
        }
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (AutoApplyTransitionOrigins)
                _nextTransitionOrigin = GetNavigationSourcePoint(e);
            base.OnPreviewMouseLeftButtonDown(e);
        }
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            var unselectedIndex = -1;
            if (e.RemovedItems.Count == 1)
            {
                unselectedIndex = Items.IndexOf(e.RemovedItems[0]);
            }
            var selectedIndex = 1;
            if (e.AddedItems.Count == 1)
            {
                selectedIndex = Items.IndexOf(e.AddedItems[0]);
            }

            TransitionChanged?.Invoke(this, null);
            ActivateFrame(selectedIndex, unselectedIndex);
            base.OnSelectionChanged(e);
        }
        private void IsTransitionFinishedHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            foreach (var slide in Items.OfType<object>().Select(GetSlide).Where(s => s.State == TransitionerSlideState.Previous))
            {
                slide.SetCurrentValue(TransitionerSlide.StateProperty, TransitionerSlideState.None);
            }
        }
        private void MoveNextHandler(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            if (AutoApplyTransitionOrigins)
                _nextTransitionOrigin = GetNavigationSourcePoint(executedRoutedEventArgs);
            SetCurrentValue(SelectedIndexProperty, Math.Min(Items.Count - 1, SelectedIndex + 1));
        }
        private void MovePreviousHandler(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            if (AutoApplyTransitionOrigins)
                _nextTransitionOrigin = GetNavigationSourcePoint(executedRoutedEventArgs);
            SetCurrentValue(SelectedIndexProperty, Math.Max(0, SelectedIndex - 1));
        }
        private void MoveFirstHandler(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            if (AutoApplyTransitionOrigins)
                _nextTransitionOrigin = GetNavigationSourcePoint(executedRoutedEventArgs);
            SetCurrentValue(SelectedIndexProperty, 0);
        }
        private void MoveLastHandler(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            if (AutoApplyTransitionOrigins)
                _nextTransitionOrigin = GetNavigationSourcePoint(executedRoutedEventArgs);
            SetCurrentValue(SelectedIndexProperty, Items.Count - 1);
        }
        private Point? GetNavigationSourcePoint(RoutedEventArgs executedRoutedEventArgs)
        {
            var sourceElement = executedRoutedEventArgs.OriginalSource as FrameworkElement;
            if (sourceElement == null || !IsAncestorOf(sourceElement) || !IsSafePositive(ActualWidth) ||
                !IsSafePositive(ActualHeight) || !IsSafePositive(sourceElement.ActualWidth) ||
                !IsSafePositive(sourceElement.ActualHeight)) return null;

            var transitionOrigin = sourceElement.TranslatePoint(new Point(sourceElement.ActualWidth / 2, sourceElement.ActualHeight), this);
            transitionOrigin = new Point(transitionOrigin.X / ActualWidth, transitionOrigin.Y / ActualHeight);
            return transitionOrigin;
        }
        private static bool IsSafePositive(double @double)
        {
            return !double.IsNaN(@double) && !double.IsInfinity(@double) && @double > 0.0;
        }
        private TransitionerSlide GetSlide(object item)
        {
            if (IsItemItsOwnContainer(item))
                return (TransitionerSlide)item;

            return (TransitionerSlide)ItemContainerGenerator.ContainerFromItem(item);
        }
        private void ActivateFrame(int selectedIndex, int unselectedIndex)
        {
            if (!IsLoaded) return;

            TransitionerSlide oldSlide = null, newSlide = null;
            int count = Items.Count;
            for (var index = 0; index < count; index++)
            {
                var slide = GetSlide(Items[index]);
                if (slide == null) continue;
                if (index == selectedIndex)
                {
                    newSlide = slide;
                    slide.SetCurrentValue(TransitionerSlide.StateProperty, TransitionerSlideState.Current);
                }
                else if (index == unselectedIndex)
                {
                    oldSlide = slide;
                    slide.SetCurrentValue(TransitionerSlide.StateProperty, TransitionerSlideState.Previous);
                }
                else
                {
                    slide.SetCurrentValue(TransitionerSlide.StateProperty, TransitionerSlideState.None);
                }
                Panel.SetZIndex(slide, 0);
            }
            if (newSlide != null) newSlide.Visibility = Visibility.Visible;

            if (oldSlide != null && newSlide != null)
            {
                ITransitionWipe wipe = selectedIndex > unselectedIndex ? oldSlide.ForwardWipe : oldSlide.BackwardWipe;
                if (wipe == null)
                    wipe = this.TransitionWipeSelector.ProviderTransitionWipeFrom(oldSlide, newSlide, this);
                if (wipe != null)
                {
                    wipe.Wipe(oldSlide, newSlide, GetTransitionOrigin(newSlide), this);
                }
                else
                {
                    DoStack(newSlide, oldSlide);
                    if (oldSlide != null)
                        oldSlide.Visibility = Visibility.Hidden;
                }
            }
            else if (oldSlide != null || newSlide != null)
            {
                DoStack(oldSlide ?? newSlide);
                if (oldSlide != null) oldSlide.Visibility = Visibility.Hidden;
            }

            _nextTransitionOrigin = null;
        }
        private Point GetTransitionOrigin(TransitionerSlide slide)
        {
            if (_nextTransitionOrigin != null)
            {
                return _nextTransitionOrigin.Value;
            }

            if (slide.ReadLocalValue(TransitionerSlide.TransitionOriginProperty) != DependencyProperty.UnsetValue)
            {
                return slide.TransitionOrigin;
            }

            return DefaultTransitionOrigin;
        }
        void ITransitionContainer.SetZIndexOrderBy(params FrameworkElement[] presenters)
        {

            DoStack(presenters);
        }
        void ITransitionContainer.OnCompletedTransition() { }
        private static void DoStack(params FrameworkElement[] presenters)
        {
            if (presenters == null) return;

            var pos = presenters.Length;
            foreach (var slide in presenters.Where(s => s != null))
            {
                Panel.SetZIndex(slide, pos--);
            }
        }
        public int Indexof(FrameworkElement presenter)
        {
            return this.ItemContainerGenerator.IndexFromContainer(presenter);
        }
        public object ItemFromContainer(FrameworkElement presenter)
        {
            return this.ItemContainerGenerator.ItemFromContainer(presenter);
        }
        public event EventHandler TransitionChanged;
    }
}
