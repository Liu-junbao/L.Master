using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace System.Windows
{
    public class CustomDataGrid:DataGrid
    {
        static CustomDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomDataGrid), new FrameworkPropertyMetadata(typeof(CustomDataGrid)));
        }
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CustomDataGridRow;
        }
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CustomDataGridRow(this);
        }
    }
    public class CustomDataGridRow:DataGridRow
    {
        private CustomDataGrid _owner;
        public CustomDataGridRow(CustomDataGrid owner)
        {
            _owner = owner;
        }
    }
    public class CustomDataGridBar: Control
    {
        #region commands
        private static RoutedUICommand _moveToFirstCommand;
        public static ICommand MoveToFirstCommand
        {
            get
            {
                if (_moveToFirstCommand == null)
                {
                    _moveToFirstCommand = new RoutedUICommand("move to first command", nameof(MoveToFirstCommand), typeof(CustomDataGridBar));
                    //注册热键
                    //_moveToFirstCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _moveToFirstCommand;
            }
        }

        private static RoutedUICommand _moveToLastCommand;
        public static ICommand MoveToLastCommand
        {
            get
            {
                if (_moveToLastCommand == null)
                {
                    _moveToLastCommand = new RoutedUICommand("move to last command", nameof(MoveToLastCommand), typeof(CustomDataGridBar));
                    //注册热键
                    //_moveToLastCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _moveToLastCommand;
            }
        }

        private static RoutedUICommand _moveToNextCommand;
        public static ICommand MoveToNextCommand
        {
            get
            {
                if (_moveToNextCommand == null)
                {
                    _moveToNextCommand = new RoutedUICommand("move to next command", nameof(MoveToNextCommand), typeof(CustomDataGridBar));
                    //注册热键
                    //_moveToNextCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _moveToNextCommand;
            }
        }

        private static RoutedUICommand _moveToPreviousCommand;
        public static ICommand MoveToPreviousCommand
        {
            get
            {
                if (_moveToPreviousCommand == null)
                {
                    _moveToPreviousCommand = new RoutedUICommand("move to previous command", nameof(MoveToPreviousCommand), typeof(CustomDataGridBar));
                    //注册热键
                    //_moveToPreviousCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _moveToPreviousCommand;
            }
        }

        private static RoutedUICommand _moveToCurrentCommand;
        public static ICommand MoveToCurrentCommand
        {
            get
            {
                if (_moveToCurrentCommand == null)
                {
                    _moveToCurrentCommand = new RoutedUICommand("move to current command", nameof(MoveToCurrentCommand), typeof(CustomDataGridBar));
                    //注册热键
                    //_moveToCurrentCommand.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _moveToCurrentCommand;
            }
        }

        #endregion

        public static readonly DependencyProperty CountProperty =
          DependencyProperty.Register(nameof(Count), typeof(int), typeof(CustomDataGridBar), new PropertyMetadata(0));
        public static readonly DependencyProperty PageCountProperty =
          DependencyProperty.Register(nameof(PageCount), typeof(int), typeof(CustomDataGridBar), new PropertyMetadata(0));
        public static readonly DependencyProperty PageIndexProperty =
          DependencyProperty.Register(nameof(PageIndex), typeof(int), typeof(CustomDataGridBar), new PropertyMetadata(0));
        public static readonly DependencyProperty DisplayCountProperty =
          DependencyProperty.Register(nameof(DisplayCount), typeof(int), typeof(CustomDataGridBar), new PropertyMetadata(5));
        private static readonly DependencyPropertyKey DisplayIndexStatusPropertyKey =
          DependencyProperty.RegisterReadOnly(nameof(DisplayIndexStatus), typeof(DisplayIndexStatus), typeof(CustomDataGridBar), new PropertyMetadata(DisplayIndexStatus.None));
        public static readonly DependencyProperty DisplayIndexStatusProperty = DisplayIndexStatusPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey DisplayIndexSourcePropertyKey =
          DependencyProperty.RegisterReadOnly(nameof(DisplayIndexSource), typeof(int[]), typeof(CustomDataGridBar), new PropertyMetadata(null));
        public static readonly DependencyProperty DisplayIndexSourceProperty = DisplayIndexSourcePropertyKey.DependencyProperty;
        static CustomDataGridBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomDataGridBar),new FrameworkPropertyMetadata(typeof(CustomDataGridBar)));
        }
        private int _displayIndex;
        public CustomDataGridBar()
        {
            this.CommandBindings.Add(new CommandBinding(MoveToFirstCommand,new ExecutedRoutedEventHandler(OnMoveToFirst)));
            this.CommandBindings.Add(new CommandBinding(MoveToLastCommand, new ExecutedRoutedEventHandler(OnMoveToLast)));
            this.CommandBindings.Add(new CommandBinding(MoveToNextCommand, new ExecutedRoutedEventHandler(OnMoveToNext)));
            this.CommandBindings.Add(new CommandBinding(MoveToPreviousCommand, new ExecutedRoutedEventHandler(OnMoveToPrevious)));
            this.CommandBindings.Add(new CommandBinding(MoveToCurrentCommand, new ExecutedRoutedEventHandler(OnMoveToCurrent)));
        }

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }
        public int PageCount
        {
            get { return (int)GetValue(PageCountProperty); }
            set { SetValue(PageCountProperty, value); }
        }
        public int PageIndex
        {
            get { return (int)GetValue(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }
        public int DisplayCount
        {
            get { return (int)GetValue(DisplayCountProperty); }
            set { SetValue(DisplayCountProperty, value); }
        }
        public DisplayIndexStatus DisplayIndexStatus
        {
            get { return (DisplayIndexStatus)GetValue(DisplayIndexStatusProperty); }
            protected set { SetValue(DisplayIndexStatusPropertyKey, value); }
        }
        public int[] DisplayIndexSource
        {
            get { return (int[])GetValue(DisplayIndexSourceProperty); }
            protected set { SetValue(DisplayIndexSourcePropertyKey, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == PageCountProperty || e.Property == DisplayCountProperty)
            {
                _displayIndex = 0;
                InvalidateDisplayIndex();
            }
            else if (e.Property == PageIndexProperty)
            {
                BringPageIndexIntoView();
            }
        }
        private void OnMoveToFirst(object sender, ExecutedRoutedEventArgs e)
        {
            _displayIndex = 0;
            InvalidateDisplayIndex();
        }
        private void OnMoveToLast(object sender, ExecutedRoutedEventArgs e)
        {
            _displayIndex = PageCount;
            InvalidateDisplayIndex();
        }
        private void OnMoveToPrevious(object sender, ExecutedRoutedEventArgs e)
        {
            _displayIndex--;
            InvalidateDisplayIndex();
        }
        private void OnMoveToNext(object sender, ExecutedRoutedEventArgs e)
        {
            _displayIndex++;
            InvalidateDisplayIndex();
        }
        private void OnMoveToCurrent(object sender, ExecutedRoutedEventArgs e)
        {
            BringPageIndexIntoView();
        }
        private void BringPageIndexIntoView()
        {
            var pageIndex = PageIndex;
            if (pageIndex < 0)
            {
                PageIndex = 0;
                return;
            }
            if (pageIndex > PageCount)
            {
                PageIndex = PageCount;
                return;
            }
            var source = DisplayIndexSource;
            if (source == null || source.Contains(pageIndex) == false)
            {
                var displayCount = DisplayCount;
                if (displayCount <= 0)
                    displayCount = 1;
                int remain;
                var displayIndex = Math.DivRem(pageIndex, displayCount, out remain) - 1;
                if (remain > 0)
                    displayIndex++;
                _displayIndex = displayIndex;
                InvalidateDisplayIndex();
            }
        }
        private void InvalidateDisplayIndex()
        {
            var count = PageCount;
            var targetDisplayIndex = _displayIndex;
            if (count > 0)
            {
                var displayCount = DisplayCount;
                if (displayCount < 1) displayCount = 1;
                if (targetDisplayIndex < 0) targetDisplayIndex = 0;
                int minIndex = targetDisplayIndex * displayCount;
                while (minIndex >= count)
                {
                    targetDisplayIndex--;
                    minIndex = targetDisplayIndex * displayCount;
                }

                _displayIndex = targetDisplayIndex;

                bool isFirstPage = minIndex == 0;
                bool isLastPage = false;
                List<int> indexs = new List<int>();
                for (int i = 0; i < displayCount; i++)
                {
                    var displayIndex = minIndex + i + 1;
                    if (displayIndex <= count)
                    {
                        indexs.Add(displayIndex);
                        if (displayIndex == count) isLastPage = true;
                    }
                    else
                    {
                        isLastPage = true;
                        break;
                    }
                }

                DisplayIndexSource = indexs.ToArray();
                if (isFirstPage && isLastPage)
                    DisplayIndexStatus = DisplayIndexStatus.None;
                else if (isFirstPage)
                    DisplayIndexStatus = DisplayIndexStatus.Start;
                else if (isLastPage)
                    DisplayIndexStatus = DisplayIndexStatus.End;
                else
                    DisplayIndexStatus = DisplayIndexStatus.Center;
            }
            else
            {
                DisplayIndexSource = null;
                DisplayIndexStatus = DisplayIndexStatus.None;
            }
        }
    }

    public enum DisplayIndexStatus
    {
        None,
        Start,
        End,
        Center
    }
}
