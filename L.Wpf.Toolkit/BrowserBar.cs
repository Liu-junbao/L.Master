using System.Collections;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows
{
    [StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(BrowserBarItem))]
    public class BrowserBar : Selector
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
        public static readonly DependencyProperty IsOpenDownProperty =
            DependencyProperty.Register(nameof(IsOpenDown), typeof(bool), typeof(BrowserBar), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        static BrowserBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BrowserBar), new FrameworkPropertyMetadata(typeof(BrowserBar)));
        }
        public BrowserBar()
        {
            this.CommandBindings.Add(new CommandBinding(BrowserBar.ClosePage, new ExecutedRoutedEventHandler(OnClosePage)));
        }
        public bool IsOpenDown
        {
            get { return (bool)GetValue(IsOpenDownProperty); }
            set { SetValue(IsOpenDownProperty, value); }
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
        private void OnClosePage(object sender, ExecutedRoutedEventArgs e)
        {
            var command = ClosePageCommand;
            if (command != null && command.CanExecute(e.Parameter))
            {
                command.Execute(e.Parameter);
            }
        }
    }

    [StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(BrowserBarItem))]
    [ContentProperty(nameof(Header))]
    public class BrowserPupBox:Selector
    {
        static BrowserPupBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BrowserPupBox), new FrameworkPropertyMetadata(typeof(BrowserPupBox)));
        }
        public static readonly DependencyProperty HeaderProperty =
           DependencyProperty.Register(nameof(Header), typeof(object), typeof(BrowserPupBox), new PropertyMetadata(null));
        public static readonly DependencyProperty IsOpenDownProperty =
           DependencyProperty.Register(nameof(IsOpenDown), typeof(bool), typeof(BrowserPupBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public bool IsOpenDown
        {
            get { return (bool)GetValue(IsOpenDownProperty); }
            set { SetValue(IsOpenDownProperty, value); }
        }
    }
    public class BrowserBarItem : CustomSelectablePresenter
    {
        public static readonly DependencyProperty CanCloseProperty =
           DependencyProperty.Register(nameof(CanClose), typeof(bool), typeof(BrowserBarItem), new PropertyMetadata(false));
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
}
