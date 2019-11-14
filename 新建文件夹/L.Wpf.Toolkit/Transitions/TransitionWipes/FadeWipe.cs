using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace System.Windows.Transitions
{
    public class FadeWipe :TransitionWipeBase
    {
        private readonly SineEase _sineEase = new SineEase();
        public FadeWipe()
        {
            TransitionOrder = TransitionOrder.OldFirst;
        }
        protected override Storyboard CreateStoryboardForOldPresenter(ITransitionContainer container)
        {
            var zeroKeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero);
            var endKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5));

            var fromAnimation = new DoubleAnimationUsingKeyFrames();
            fromAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(1, zeroKeyTime));
            fromAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, endKeyTime, _sineEase));

            var storyboard = new Storyboard();
            storyboard.Duration= TimeSpan.FromSeconds(3);
            storyboard.Children.Add(fromAnimation);
            Storyboard.SetTargetProperty(fromAnimation,new PropertyPath(UIElement.OpacityProperty));

            return storyboard;
        }

        protected override Storyboard CreateStoryboardForNewPresenter(ITransitionContainer container)
        {
            var zeroKeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero);
            var endKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5));

            var toAnimation = new DoubleAnimationUsingKeyFrames();
            toAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0, zeroKeyTime));
            toAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, endKeyTime, _sineEase));

            var storyboard = new Storyboard();
            storyboard.Duration = TimeSpan.FromSeconds(1);
            storyboard.Children.Add(toAnimation);
            Storyboard.SetTargetProperty(toAnimation, new PropertyPath(UIElement.OpacityProperty));

            return storyboard;
        }
        protected override Storyboard PrepareStoryboardForOldPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            oldPresenter.Opacity = 1;
            return base.PrepareStoryboardForOldPresenter(oldPresenter, newPresenter, origin, container);
        }
        protected override Storyboard PrepareStoryboardForNewPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            newPresenter.Opacity = 0;
            return base.PrepareStoryboardForNewPresenter(oldPresenter, newPresenter, origin, container);
        }
        protected override void OnCompletedTransition(Storyboard oldStoryboard, FrameworkElement oldPresenter, Storyboard newStoryboard, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            oldPresenter.Opacity = 1;
            newPresenter.Opacity = 1;
            base.OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
        }
    }
}