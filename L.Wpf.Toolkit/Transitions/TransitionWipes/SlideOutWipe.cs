using System.Windows.Media;
using System.Windows.Media.Animation;

namespace System.Windows.Transitions
{
    public class SlideOutWipe : TransitionWipeBase
    {
        private readonly SineEase _sineEase = new SineEase();
        protected override Storyboard CreateStoryboardForOldPresenter(ITransitionContainer container)
        {
            var zeroKeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero);
            var midishKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200));
            var endKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400));

            var scaleAnimation = new DoubleAnimationUsingKeyFrames();
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, zeroKeyTime));
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(.8, endKeyTime));

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, zeroKeyTime));
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, endKeyTime));

            Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(scaleAnimation);
            Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
            Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));
            storyboard.Children.Add(opacityAnimation);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));

            return storyboard;
        }

        protected override Storyboard CreateStoryboardForNewPresenter(ITransitionContainer container)
        {
            var zeroKeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero);
            var midishKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200));
            var endKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400));

            var slideAnimation = new DoubleAnimationUsingKeyFrames();
            slideAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(container.ActualHeight, zeroKeyTime));
            slideAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(container.ActualHeight, midishKeyTime) { EasingFunction = _sineEase });
            slideAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, endKeyTime) { EasingFunction = _sineEase });

            Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(slideAnimation);
            Storyboard.SetTargetProperty(slideAnimation, new PropertyPath(TranslateTransform.YProperty));

            return storyboard;
        }
        protected override Storyboard PrepareStoryboardForOldPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            var scaleTransform = new ScaleTransform(1, 1);
            oldPresenter.RenderTransform = scaleTransform;
            return base.PrepareStoryboardForOldPresenter(oldPresenter, newPresenter, origin, container);
        }
        protected override Storyboard PrepareStoryboardForNewPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            var translateTransform = new TranslateTransform(0, container.ActualHeight);
            newPresenter.RenderTransform = translateTransform;
            return base.PrepareStoryboardForNewPresenter(oldPresenter, newPresenter, origin, container);
        }
        protected override void OnCompletedTransition(Storyboard oldStoryboard, FrameworkElement oldPresenter, Storyboard newStoryboard, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            oldPresenter.Opacity = 1;
            oldPresenter.RenderTransform = null;
            newPresenter.RenderTransform = null;
            base.OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
        }
    }
}