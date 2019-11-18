using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace System.Windows
{
    public class CustomPresenter:ContentControl
    {
        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.Register(nameof(MouseOverBackground), typeof(Brush), typeof(CustomPresenter), new FrameworkPropertyMetadata(default(Brush)));
        public static readonly DependencyProperty MouseOverBackOpacityProperty =
            DependencyProperty.Register(nameof(MouseOverBackOpacity), typeof(double), typeof(CustomPresenter), new PropertyMetadata(0.5));

        static CustomPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomPresenter),new FrameworkPropertyMetadata(typeof(CustomPresenter)));
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
        public static readonly DependencyProperty MouseOverBackgroundProperty =
           DependencyProperty.RegisterAttached("MouseOverBackground", typeof(Brush), typeof(CustomPresenterAssit), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MouseOverBackOpacityProperty =
           DependencyProperty.RegisterAttached("MouseOverBackOpacity", typeof(double), typeof(CustomPresenterAssit), new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));


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
}
