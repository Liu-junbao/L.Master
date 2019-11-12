using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace System.Windows.Transitions
{
    public class CircleWipe : TransitionWipeBase
    {
        protected override Storyboard CreateStoryboardForOldPresenter(ITransitionContainer container)
        {
            var zeroKeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero);
            var midKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200));
            var endKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400));

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, zeroKeyTime));
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, midKeyTime));
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, endKeyTime));

            var storyboard = new Storyboard();

            storyboard.Children.Add(opacityAnimation);
            Storyboard.SetTargetProperty(opacityAnimation,new PropertyPath(UIElement.OpacityProperty));

            return storyboard;
        }
        protected override Storyboard CreateStoryboardForNewPresenter(ITransitionContainer container)
        {
            var zeroKeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero);
            var midKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200));
            var endKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400));

            var scaleAnimation = new DoubleAnimationUsingKeyFrames();
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0, zeroKeyTime));
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, endKeyTime));

            var storyboard = new Storyboard();

            storyboard.Children.Add(scaleAnimation);
            Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));
            Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

            return storyboard;
        }
        protected override Storyboard PrepareStoryboardForOldPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            return base.PrepareStoryboardForOldPresenter(oldPresenter, newPresenter, origin, container);
        }
        protected override Storyboard PrepareStoryboardForNewPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            var horizontalProportion = Math.Max(1.0 - origin.X, 1.0 * origin.X);
            var verticalProportion = Math.Max(1.0 - origin.Y, 1.0 * origin.Y);
            var radius = Math.Sqrt(Math.Pow(newPresenter.ActualWidth * horizontalProportion, 2) + Math.Pow(newPresenter.ActualHeight * verticalProportion, 2));
            var scaleTransform = new ScaleTransform(0, 0);
            var translateTransform = new TranslateTransform(newPresenter.ActualWidth * origin.X, newPresenter.ActualHeight * origin.Y);
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);
            var ellipseGeomotry = new EllipseGeometry()
            {
                RadiusX = radius,
                RadiusY = radius,
                Transform = transformGroup
            };
            newPresenter.SetCurrentValue(UIElement.ClipProperty, ellipseGeomotry);
            return base.PrepareStoryboardForNewPresenter(oldPresenter, newPresenter, origin, container);
        }

        protected override void OnCompletedTransition(Storyboard oldStoryboard, FrameworkElement oldPresenter, Storyboard newStoryboard, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            newPresenter.SetCurrentValue(UIElement.ClipProperty, null);
            oldPresenter.RenderTransform = null;
            newPresenter.RenderTransform = null;
            oldPresenter.Opacity = 1;
            base.OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
        }
    }
}