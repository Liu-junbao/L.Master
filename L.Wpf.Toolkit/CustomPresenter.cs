using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace System.Windows
{
    public class CustomPresenter : ContentControl
    {
        public static readonly DependencyProperty BackgroundOpacityProperty =
            DependencyProperty.Register(nameof(BackgroundOpacity), typeof(double), typeof(CustomPresenter), new PropertyMetadata(1.0));
        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.Register(nameof(MouseOverBackground), typeof(Brush), typeof(CustomPresenter), new FrameworkPropertyMetadata(default(Brush)));
        public static readonly DependencyProperty MouseOverBackOpacityProperty =
            DependencyProperty.Register(nameof(MouseOverBackOpacity), typeof(double), typeof(CustomPresenter), new PropertyMetadata(0.5));

        static CustomPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomPresenter), new FrameworkPropertyMetadata(typeof(CustomPresenter)));
        }
        [Category("Appearance")]
        public double BackgroundOpacity
        {
            get { return (double)GetValue(BackgroundOpacityProperty); }
            set { SetValue(BackgroundOpacityProperty, value); }
        }
        [Category("Appearance")]
        public Brush MouseOverBackground
        {
            get { return (Brush)GetValue(MouseOverBackgroundProperty); }
            set { SetValue(MouseOverBackgroundProperty, value); }
        }
        [Category("Appearance")]
        public double MouseOverBackOpacity
        {
            get { return (double)GetValue(MouseOverBackOpacityProperty); }
            set { SetValue(MouseOverBackOpacityProperty, value); }
        }
    }

    public static class CustomPresenterAssit
    {
        public static readonly DependencyProperty BackgroundOpacityProperty =
          DependencyProperty.RegisterAttached("BackgroundOpacity", typeof(double), typeof(CustomPresenterAssit), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MouseOverBackgroundProperty =
           DependencyProperty.RegisterAttached("MouseOverBackground", typeof(Brush), typeof(CustomPresenterAssit), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.LightGray), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MouseOverBackOpacityProperty =
           DependencyProperty.RegisterAttached("MouseOverBackOpacity", typeof(double), typeof(CustomPresenterAssit), new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));
        public static double GetBackgroundOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(BackgroundOpacityProperty);
        }
        public static void SetBackgroundOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(BackgroundOpacityProperty, value);
        }
        public static Brush GetMouseOverBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(MouseOverBackgroundProperty);
        }
        public static void SetMouseOverBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(MouseOverBackgroundProperty, value);
        }
        public static double GetMouseOverBackOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(MouseOverBackOpacityProperty);
        }
        public static void SetMouseOverBackOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(MouseOverBackOpacityProperty, value);
        }
    }


    public class CustomSelectablePresenter : CustomPresenter
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(CustomSelectablePresenter), new PropertyMetadata(false));
        public static readonly DependencyProperty SelectedBackgroundProperty =
            DependencyProperty.Register(nameof(SelectedBackground), typeof(Brush), typeof(CustomSelectablePresenter), new PropertyMetadata(default(Brush)));
        static CustomSelectablePresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomSelectablePresenter), new FrameworkPropertyMetadata(typeof(CustomSelectablePresenter)));
        }
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        [Category("Appearance")]
        public Brush SelectedBackground
        {
            get { return (Brush)GetValue(SelectedBackgroundProperty); }
            set { SetValue(SelectedBackgroundProperty, value); }
        }
    }

    public static class CustomSelectablePresenterAssist
    {
        public static readonly DependencyProperty SelectedBackgroundProperty =
           DependencyProperty.RegisterAttached("SelectedBackground", typeof(Brush), typeof(CustomSelectablePresenterAssist), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IsSelectedProperty =
         DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(CustomSelectablePresenterAssist), new PropertyMetadata(false, OnIsSelectedChanged));

        public static Brush GetSelectedBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(SelectedBackgroundProperty);
        }
        public static void SetSelectedBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(SelectedBackgroundProperty, value);
        }
        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }
        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            element?.BringIntoView();
        }
    }

}
