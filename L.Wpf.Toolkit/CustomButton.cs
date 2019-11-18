using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
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
}
