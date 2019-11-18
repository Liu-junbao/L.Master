using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;

namespace System.Windows
{
    public class CustomSelectablePresenter:CustomPresenter
    {
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(CustomSelectablePresenter), new PropertyMetadata(false));
        public static readonly DependencyProperty SelectedBackgroundProperty =
            DependencyProperty.Register(nameof(SelectedBackground), typeof(Brush), typeof(CustomSelectablePresenter), new PropertyMetadata(default(Brush)));
        static CustomSelectablePresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomSelectablePresenter),new FrameworkPropertyMetadata(typeof(CustomSelectablePresenter)));
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
