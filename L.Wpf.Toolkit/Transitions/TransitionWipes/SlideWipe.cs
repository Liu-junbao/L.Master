using System.Windows.Media;
using System.Windows.Media.Animation;

namespace System.Windows.Transitions
{
    public class SlideWipe : TransitionWipeBase
    {
        private readonly SineEase _sineEase = new SineEase();

        /// <summary>
        /// Direction of the slide wipe
        /// </summary>
        public SlideDirection Direction { get; set; }
        protected override Storyboard CreateStoryboardForOldPresenter(ITransitionContainer container)
        {
            double fromStartX = 0, fromEndX = 0;
            double fromStartY = 0, fromEndY = 0;
            switch (Direction)
            {
                case SlideDirection.Left:
                    fromEndX = -container.ActualWidth - 1;
                    break;
                case SlideDirection.Right:
                    fromEndX = container.ActualWidth + 1;
                    break;
                case SlideDirection.Up:
                    fromEndY = -container.ActualHeight - 1;
                    break;
                case SlideDirection.Down:
                    fromEndY = container.ActualHeight + 1;
                    break;
                default:
                    break;
            }

            return GetStoryboard(fromStartX, fromEndX, fromStartY, fromEndY);
        }
        protected override Storyboard CreateStoryboardForNewPresenter(ITransitionContainer container)
        {
            double toStartX = 0, toEndX = 0;
            double toStartY = 0, toEndY = 0;
            switch (Direction)
            {
                case SlideDirection.Left:
                    toStartX = container.ActualWidth;
                    break;
                case SlideDirection.Right:
                    toStartX = -container.ActualWidth;
                    break;
                case SlideDirection.Up:
                    toStartY = container.ActualHeight;
                    break;
                case SlideDirection.Down:
                    toStartY = -container.ActualHeight;
                    break;
                default:
                    break;
            }

            return GetStoryboard(toStartX, toEndX, toStartY, toEndY);
        }
        protected override Storyboard PrepareStoryboardForOldPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            var fromTransform = new TranslateTransform(0, 0);
            oldPresenter.RenderTransform = fromTransform;
            return base.PrepareStoryboardForOldPresenter(oldPresenter, newPresenter, origin, container);
        }
        protected override Storyboard PrepareStoryboardForNewPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            var toTransform = new TranslateTransform(0, 0);
            newPresenter.RenderTransform = toTransform;
            return base.PrepareStoryboardForNewPresenter(oldPresenter, newPresenter, origin, container);
        }

        private Storyboard GetStoryboard(double fromX, double toX, double fromY, double toY)
        {
            var zeroKeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero);
            var endKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.7));

            var fromXAnimation = new DoubleAnimationUsingKeyFrames();
            fromXAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(fromX, zeroKeyTime));
            fromXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(toX, endKeyTime, _sineEase));


            var fromYAnimation = new DoubleAnimationUsingKeyFrames();
            fromYAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(fromY, zeroKeyTime));
            fromYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(toY, endKeyTime, _sineEase));


            Storyboard.SetTargetProperty(fromXAnimation, new PropertyPath($"({nameof(UIElement)}.{nameof(UIElement.RenderTransform)}).({nameof(TranslateTransform)}.{nameof(TranslateTransform.X)})"));
            Storyboard.SetTargetProperty(fromYAnimation, new PropertyPath($"({nameof(UIElement)}.{nameof(UIElement.RenderTransform)}).({nameof(TranslateTransform)}.{nameof(TranslateTransform.Y)})"));

            Storyboard storyboard = new Storyboard();
            storyboard.Duration = TimeSpan.FromSeconds(1.5);

            storyboard.Children.Add(fromXAnimation);
            storyboard.Children.Add(fromYAnimation);

            return storyboard;
        }

        protected override void OnCompletedTransition(Storyboard oldStoryboard, FrameworkElement oldPresenter, Storyboard newStoryboard, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            oldPresenter.RenderTransform = null;
            newPresenter.RenderTransform = null;
            base.OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
        }
    }

    public enum SlideDirection { Left, Right, Up, Down }
}