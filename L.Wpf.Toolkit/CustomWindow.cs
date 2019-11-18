using System.Windows.Input;

namespace System.Windows
{
    public class CustomWindow : Window
    {
        public static readonly DependencyProperty HeaderBarProperty =
            DependencyProperty.Register(nameof(HeaderBar), typeof(object), typeof(CustomWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(CustomWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty ShouldNotifyIconProperty =
           DependencyProperty.Register(nameof(ShouldNotifyIcon), typeof(bool), typeof(CustomWindow), new PropertyMetadata(false));

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
        public object HeaderBar
        {
            get { return (object)GetValue(HeaderBarProperty); }
            set { SetValue(HeaderBarProperty, value); }
        }
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public bool ShouldNotifyIcon
        {
            get { return (bool)GetValue(ShouldNotifyIconProperty); }
            set { SetValue(ShouldNotifyIconProperty, value); }
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
}
