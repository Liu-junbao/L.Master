using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace System.Windows
{
    public class CustomButton : Button
    {
        public static readonly DependencyProperty PressedBackgroundProperty =
          DependencyProperty.Register(nameof(PressedBackground), typeof(Brush), typeof(CustomButton), new PropertyMetadata(default(Brush)));
        static CustomButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(typeof(CustomButton)));
        }
        [Category("Appearance")]
        public Brush PressedBackground
        {
            get { return (Brush)GetValue(PressedBackgroundProperty); }
            set { SetValue(PressedBackgroundProperty, value); }
        }
    }

    public class CustomToggleButton:ToggleButton
    {
        public static readonly DependencyProperty PressedBackgroundProperty =
         DependencyProperty.Register(nameof(PressedBackground), typeof(Brush), typeof(CustomToggleButton), new PropertyMetadata(default(Brush)));
        static CustomToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomToggleButton), new FrameworkPropertyMetadata(typeof(CustomToggleButton)));
        }
        [Category("Appearance")]
        public Brush PressedBackground
        {
            get { return (Brush)GetValue(PressedBackgroundProperty); }
            set { SetValue(PressedBackgroundProperty, value); }
        }
    }
}
