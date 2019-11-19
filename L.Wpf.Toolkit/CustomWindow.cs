﻿using System.Windows.Controls;
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
        public static readonly DependencyProperty DragWindowProperty =
          DependencyProperty.Register(nameof(DragWindow), typeof(Window), typeof(CustomWindowDrager), new PropertyMetadata(null));
        static CustomWindowDrager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindowDrager),new FrameworkPropertyMetadata(typeof(CustomWindowDrager)));
        }
        public Window DragWindow
        {
            get { return (Window)GetValue(DragWindowProperty); }
            set { SetValue(DragWindowProperty, value); }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragWindow?.DragMove();
            }
        }
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (DragWindow != null)
                DragWindow.WindowState = DragWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }
}
