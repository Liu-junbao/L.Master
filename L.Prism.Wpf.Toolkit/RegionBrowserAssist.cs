using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Prism
{
    public static class RegionBrowserAssist
    {
        public static readonly DependencyProperty CanCloseProperty =
            DependencyProperty.RegisterAttached("CanClose", typeof(bool), typeof(RegionBrowserAssist), new PropertyMetadata(true));
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(ImageSource), typeof(RegionBrowserAssist), new PropertyMetadata(null));
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.RegisterAttached("Data", typeof(Geometry), typeof(RegionBrowserAssist), new PropertyMetadata(null));
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.RegisterAttached("Header", typeof(string), typeof(RegionBrowserAssist), new PropertyMetadata(null));
        public static readonly DependencyProperty ToolTipProperty =
            DependencyProperty.RegisterAttached("ToolTip", typeof(object), typeof(RegionBrowserAssist), new PropertyMetadata(null));
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.RegisterAttached("Fill", typeof(Brush), typeof(RegionBrowserAssist), new PropertyMetadata(null));
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.RegisterAttached("Stroke", typeof(Brush), typeof(RegionBrowserAssist), new PropertyMetadata(SystemColors.WindowTextBrush));
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.RegisterAttached("StrokeThickness", typeof(double), typeof(RegionBrowserAssist), new PropertyMetadata(0.3));

        public static bool GetCanClose(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanCloseProperty);
        }
        public static void SetCanClose(DependencyObject obj, bool value)
        {
            obj.SetValue(CanCloseProperty, value);
        }

        public static ImageSource GetIcon(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(IconProperty);
        }
        public static void SetIcon(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(IconProperty, value);
        }
        public static Geometry GetData(DependencyObject obj)
        {
            return (Geometry)obj.GetValue(DataProperty);
        }
        public static void SetData(DependencyObject obj, Geometry value)
        {
            obj.SetValue(DataProperty, value);
        }
        public static string GetHeader(DependencyObject obj)
        {
            return (string)obj.GetValue(HeaderProperty);
        }
        public static void SetHeader(DependencyObject obj, string value)
        {
            obj.SetValue(HeaderProperty, value);
        }
        public static object GetToolTip(DependencyObject obj)
        {
            return (object)obj.GetValue(ToolTipProperty);
        }
        public static void SetToolTip(DependencyObject obj, object value)
        {
            obj.SetValue(ToolTipProperty, value);
        }
        public static Brush GetFill(DependencyObject obj)
        {
            return (Brush)obj.GetValue(FillProperty);
        }
        public static void SetFill(DependencyObject obj, Brush value)
        {
            obj.SetValue(FillProperty, value);
        }
        public static Brush GetStroke(DependencyObject obj)
        {
            return (Brush)obj.GetValue(StrokeProperty);
        }
        public static void SetStroke(DependencyObject obj, Brush value)
        {
            obj.SetValue(StrokeProperty, value);
        }
        public static double GetStrokeThickness(DependencyObject obj)
        {
            return (double)obj.GetValue(StrokeThicknessProperty);
        }
        public static void SetStrokeThickness(DependencyObject obj, double value)
        {
            obj.SetValue(StrokeThicknessProperty, value);
        }
    }
}
