using System;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace System.Windows.Transitions
{
    [StyleTypedProperty(Property = "OldStyle", StyleTargetType = typeof(TransitionerSlide))]
    [StyleTypedProperty(Property = "OldStyle", StyleTargetType = typeof(TransitionerSlide))]
    public class CustomWipe : TransitionWipeBase
    {
        private static ResourceDictionary _customWipeResources = Application.LoadComponent(new Uri($"/L.Wpf.Toolkit;component/Themes/RS.CustomWipe.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;
        private static TValue GetValue<TValue>(string name) where TValue : class => _customWipeResources[name] as TValue;
        public static CustomWipe FindLocalCustomWipe(CustomKind kind)
        {
            return GetValue<CustomWipe>(Enum.GetName(typeof(CustomKind), kind));
        }
        public static CustomWipe[] GetLocalCustomWipes()
        {
            return Enum.GetNames(typeof(CustomKind)).Select(i => GetValue<CustomWipe>(i)).ToArray();
        }

        public Style OldPresenterStyle { get; set; }
        public Style NewPresenterStyle { get; set; }
        public Storyboard OldStoryboard { get; set; }
        public Storyboard NewStoryboard { get; set; }
        protected override Storyboard CreateStoryboardForOldPresenter(ITransitionContainer container) => OldStoryboard?.Clone();
        protected override Storyboard CreateStoryboardForNewPresenter(ITransitionContainer container) => NewStoryboard?.Clone();
        protected override Storyboard PrepareStoryboardForOldPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            if (OldPresenterStyle != null)ApplyStyle(oldPresenter, OldPresenterStyle);
            return base.PrepareStoryboardForOldPresenter(oldPresenter, newPresenter, origin, container);
        }
        protected override Storyboard PrepareStoryboardForNewPresenter(FrameworkElement oldPresenter, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            if (NewPresenterStyle != null)ApplyStyle(newPresenter, NewPresenterStyle);
            return base.PrepareStoryboardForNewPresenter(oldPresenter, newPresenter, origin, container);
        }
        protected override void OnCompletedTransition(Storyboard oldStoryboard, FrameworkElement oldPresenter, Storyboard newStoryboard, FrameworkElement newPresenter, Point origin, ITransitionContainer container)
        {
            RemoveStyle(oldPresenter);
            RemoveStyle(newPresenter);
            base.OnCompletedTransition(oldStoryboard, oldPresenter, newStoryboard, newPresenter, origin, container);
        }
        private void ApplyStyle(FrameworkElement element, Style style)
        {
            if (element != null && style != null)
            {
                var settings = style.Setters.Where(i => i is Setter).Select(i => (Setter)i);
                var origin = settings.FirstOrDefault(i => i.Property == FrameworkElement.RenderTransformOriginProperty)?.Value;
                if (origin != null) element.RenderTransformOrigin = (Point)origin;
                element.RenderTransform = (Transform)settings?.FirstOrDefault(i => i.Property == FrameworkElement.RenderTransformProperty)?.Value;
                element.OpacityMask = (Brush)settings?.FirstOrDefault(i => i.Property == FrameworkElement.OpacityMaskProperty)?.Value;
                element.BitmapEffect = (BitmapEffect)settings?.FirstOrDefault(i => i.Property == FrameworkElement.BitmapEffectProperty)?.Value;
                element.Effect = (Effect)settings?.FirstOrDefault(i => i.Property == FrameworkElement.EffectProperty)?.Value;
            }
        }
        private void RemoveStyle(FrameworkElement element)
        {
            if (element != null)
            {
                element.RenderTransformOrigin = new Point();
                element.RenderTransform = null;
                element.OpacityMask = null;
                element.BitmapEffect = null;
                element.Effect = null;
            }
        }
    }

    public enum CustomKind
    {
        FadeInWipe,
        CheckerboardWipe,
        DiagonalWipe,
        DiamondsWipe,
        DotsWipe,
        DoubleRotateWipe,
        FadeAndBlurWipe,
        FadeAndGrowWipe,
        HorizontalBlindsWipe,
        HorizontalWipe,
        MeltWipe,
        RollWipe,
        RotateWipe,
        StarWipe,
        VerticalBlindsWipe,
        VerticalWipe
    }

    [MarkupExtensionReturnType(typeof(CustomWipe))]
    public class LocalCustomWipeExtension : MarkupExtension
    {
        public LocalCustomWipeExtension() { }
        public LocalCustomWipeExtension(CustomKind kind)
        {
            Kind = kind;
        }
        [ConstructorArgument("kind")]
        public CustomKind Kind { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return CustomWipe.FindLocalCustomWipe(Kind);
        }
    }
}
