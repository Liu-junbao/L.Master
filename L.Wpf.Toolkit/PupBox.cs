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
    [TemplatePart(Name = PART_Popup)]
    public class PupBox : ListBox
    {
        public const string PART_Popup = nameof(PART_Popup);

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
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var pupContainter = this.Template.FindName(PART_Popup, this) as FrameworkElement;
            if (pupContainter != null)
                pupContainter.MouseUp += PupContainter_MouseUp;
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
        private void PupContainter_MouseUp(object sender, MouseButtonEventArgs e)
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
