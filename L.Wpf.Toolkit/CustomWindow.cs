using System.Windows.Controls;
using System.Windows.Input;

namespace System.Windows
{
    public class CustomWindow : Window
    {
        static CustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));
        }
        public CustomWindow()
        {
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        }
        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode != ResizeMode.NoResize;
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }
        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }
    }

    public class CustomWindowButton:Control
    {
        public static readonly DependencyProperty WindowStyleProperty =
           DependencyProperty.Register(nameof(WindowStyle), typeof(WindowStyle), typeof(CustomWindowButton), new PropertyMetadata(WindowStyle.SingleBorderWindow));
        public static readonly DependencyProperty WindowStateProperty =
           DependencyProperty.Register(nameof(WindowState), typeof(WindowState), typeof(CustomWindowButton), new PropertyMetadata(WindowState.Normal));
        static CustomWindowButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindowButton),new FrameworkPropertyMetadata(typeof(CustomWindowButton)));
        }
        public WindowStyle WindowStyle
        {
            get { return (WindowStyle)GetValue(WindowStyleProperty); }
            set { SetValue(WindowStyleProperty, value); }
        }

        public WindowState WindowState
        {
            get { return (WindowState)GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }
    }

    public class CustomWindowDrager:ContentControl
    {
        public static readonly DependencyProperty IsDoubleClickableProperty =
           DependencyProperty.Register(nameof(IsDoubleClickable), typeof(bool), typeof(CustomWindowDrager), new PropertyMetadata(true));
        static CustomWindowDrager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindowDrager),new FrameworkPropertyMetadata(typeof(CustomWindowDrager)));
        }
        private bool _isDoublePressed;
        public bool IsDoubleClickable
        {
            get { return (bool)GetValue(IsDoubleClickableProperty); }
            set { SetValue(IsDoubleClickableProperty, value); }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && _isDoublePressed == false)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.DragMove();
                }
            }
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            _isDoublePressed = false;
            base.OnPreviewMouseDown(e);
        }
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            _isDoublePressed = true;
            base.OnMouseDoubleClick(e);
            if (IsDoubleClickable)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
            }
        }
    }
}
