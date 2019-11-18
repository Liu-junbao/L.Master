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
        private static RoutedUICommand _closePage;
        public static ICommand ClosePage
        {
            get
            {
                if (_closePage == null)
                {
                    _closePage = new RoutedUICommand("Close Page", nameof(ClosePage), typeof(BrowserBar));
                    //注册热键
                    //_close.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _closePage;
            }
        }
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
            this.CommandBindings.Add(new CommandBinding(BrowserBar.ClosePage, new ExecutedRoutedEventHandler(OnClosePage)));
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
    public class BrowserBarItem : CustomSelectablePresenter
    {
        public static readonly DependencyProperty CanCloseProperty =
           DependencyProperty.Register(nameof(CanClose), typeof(bool), typeof(BrowserBarItem), new PropertyMetadata(true));
        static BrowserBarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BrowserBarItem), new FrameworkPropertyMetadata(typeof(BrowserBarItem)));
        }
        public bool CanClose
        {
            get { return (bool)GetValue(CanCloseProperty); }
            set { SetValue(CanCloseProperty, value); }
        }
    }
    public static class BrowserBarAssist
    {
       
        public static readonly DependencyProperty PupBackgroundProperty =
           DependencyProperty.RegisterAttached("PupBackground", typeof(Brush), typeof(BrowserBarAssist), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.Inherits| FrameworkPropertyMetadataOptions.AffectsRender));
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
