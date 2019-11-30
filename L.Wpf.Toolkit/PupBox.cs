using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows
{
    [ContentProperty(nameof(Header))]
    public class PupBox : ItemsControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(PupBox), new PropertyMetadata(null));
        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(PupBox), new PropertyMetadata(PlacementMode.Bottom));
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(PupBox), new PropertyMetadata(false));
        static PupBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PupBox), new FrameworkPropertyMetadata(typeof(PupBox)));
        }
        public PupBox()
        {
            EventManager.RegisterClassHandler(typeof(PupBox),Mouse.MouseUpEvent,new RoutedEventHandler(OnMouseUpHandler));
            EventManager.RegisterClassHandler(typeof(PupBox),ButtonBase.ClickEvent, new RoutedEventHandler(OnClickHandler));
        }

     
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public PlacementMode Placement
        {
            get { return (PlacementMode)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            IsDropDownOpen = false;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            IsDropDownOpen = true;
        }
        private void OnMouseUpHandler(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = false;
        }
        private void OnClickHandler(object sender, RoutedEventArgs e)
        {
            IsDropDownOpen = false;
        }

    }

    public static class PupBoxAssist
    {
        public static readonly DependencyProperty PupBackgroundProperty =
                  DependencyProperty.RegisterAttached("PupBackground", typeof(Brush), typeof(PupBoxAssist), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.LightGray), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

        public static Brush GetPupBackground(DependencyObject obj)
        {
            return (Brush)obj.GetValue(PupBackgroundProperty);
        }
        public static void SetPupBackground(DependencyObject obj, Brush value)
        {
            obj.SetValue(PupBackgroundProperty, value);
        }
    }

}
