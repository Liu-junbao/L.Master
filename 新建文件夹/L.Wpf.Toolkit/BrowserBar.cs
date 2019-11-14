using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows
{
    [StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(BrowserBarItem))]
    public class BrowserBar:Control
    {
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(BrowserBar), new PropertyMetadata(Orientation.Horizontal));
        public static readonly DependencyProperty ClosePageCommandProperty =
            DependencyProperty.Register(nameof(ClosePageCommand), typeof(ICommand), typeof(BrowserBar), new PropertyMetadata(null));
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(nameof(ItemContainerStyle), typeof(Style), typeof(BrowserBar), new PropertyMetadata(null));
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(BrowserBar), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(BrowserBar), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(BrowserBar), new PropertyMetadata(-1));
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(BrowserBar), new PropertyMetadata(null));

        static BrowserBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BrowserBar), new FrameworkPropertyMetadata(typeof(BrowserBar)));
        }
        public BrowserBar()
        {
            this.CommandBindings.Add(new CommandBinding(BrowserBarAssist.ClosePage, new ExecutedRoutedEventHandler(OnClosePage)));
        }
        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public ICommand ClosePageCommand
        {
            get { return (ICommand)GetValue(ClosePageCommandProperty); }
            set { SetValue(ClosePageCommandProperty, value); }
        }
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        private void OnClosePage(object sender, ExecutedRoutedEventArgs e)
        {       
            var command = ClosePageCommand;
            if (command != null && command.CanExecute(e.Parameter))
            {
                command.Execute(e.Parameter);
            }
        }
    }
    public class BrowserBarItem : ContentControl
    {
        public static readonly DependencyProperty CanCloseProperty =
           DependencyProperty.Register(nameof(CanClose), typeof(bool), typeof(BrowserBarItem), new PropertyMetadata(true));
        public static readonly DependencyProperty IsSelectedProperty =
           DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(BrowserBarItem), new PropertyMetadata(false,OnIsSelectedChanged));
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BrowserBarItem item = (BrowserBarItem)d;
            if ((bool)e.NewValue == true)
            {
                item.BringIntoView();
            }
        }
        static BrowserBarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BrowserBarItem), new FrameworkPropertyMetadata(typeof(BrowserBarItem)));
        }
        public bool CanClose
        {
            get { return (bool)GetValue(CanCloseProperty); }
            set { SetValue(CanCloseProperty, value); }
        }
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
    }
    public static class BrowserBarAssist
    {
        private static RoutedUICommand _closePage;
        public static ICommand ClosePage
        {
            get
            {
                if (_closePage == null)
                {
                    _closePage = new RoutedUICommand("Close Page", nameof(ClosePage), typeof(BrowserBarAssist));
                    //注册热键
                    //_close.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _closePage;
            }
        }

        public static readonly DependencyProperty CanCloseProperty =
           DependencyProperty.RegisterAttached("CanClose", typeof(bool), typeof(BrowserBarAssist), new PropertyMetadata(true));
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(ImageSource), typeof(BrowserBarAssist), new PropertyMetadata(null));
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.RegisterAttached("Data", typeof(Geometry), typeof(BrowserBarAssist), new PropertyMetadata(null));
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.RegisterAttached("Header", typeof(string), typeof(BrowserBarAssist), new PropertyMetadata(null));
        public static readonly DependencyProperty ToolTipProperty =
            DependencyProperty.RegisterAttached("ToolTip", typeof(object), typeof(BrowserBarAssist), new PropertyMetadata(null));
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.RegisterAttached("Fill", typeof(Brush), typeof(BrowserBarAssist), new PropertyMetadata(null));
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.RegisterAttached("Stroke", typeof(Brush), typeof(BrowserBarAssist), new PropertyMetadata(SystemColors.WindowTextBrush));
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.RegisterAttached("StrokeThickness", typeof(double), typeof(BrowserBarAssist), new PropertyMetadata(0.3));

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
