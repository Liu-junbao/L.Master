using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows
{
    [ContentProperty(nameof(Header))]
    [TemplatePart(Name =PART_Header)]
    public class PupBox:ComboBox
    {
        public const string PART_Header = nameof(PART_Header);

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(PupBox), new PropertyMetadata(null));
        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(PupBox), new PropertyMetadata(PlacementMode.Bottom));
        static PupBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PupBox),new FrameworkPropertyMetadata(typeof(PupBox)));
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
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var headerContainter = this.Template.FindName(PART_Header, this) as FrameworkElement;
            headerContainter.MouseMove += HeaderContainter_MouseMove;
        }
        private void HeaderContainter_MouseMove(object sender, Input.MouseEventArgs e)
        {
            IsDropDownOpen = true;
        }
    }

    public static class PupBoxAssist
    {
        public static readonly DependencyProperty PupBackgroundProperty =
                  DependencyProperty.RegisterAttached("PupBackground", typeof(Brush), typeof(PupBoxAssist), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.LightGray), FrameworkPropertyMetadataOptions.Inherits| FrameworkPropertyMetadataOptions.AffectsRender));

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
