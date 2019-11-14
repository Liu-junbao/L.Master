using Prism.Regions;
using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prism
{
    [TemplatePart(Name =PART_Header,Type =typeof(ListBox))]
    public class RegionBrowser : ContentControl
    {
        public const string PART_Header = nameof(PART_Header);

        #region commands
        private static RoutedUICommand _closeTab;
        private static RoutedUICommand _navigateTo;

        public static ICommand CloseTab
        {
            get
            {
                if (_closeTab == null)
                {
                    _closeTab = new RoutedUICommand("close tab", nameof(CloseTab), typeof(RegionBrowser));
                    //注册热键
                    _closeTab.InputGestures.Add(new KeyGesture(Key.F4,ModifierKeys.Control));
                }
                return _closeTab;
            }
        }
        public static ICommand NavigateTo
        {
            get
            {
                if (_navigateTo == null)
                {
                    _navigateTo = new RoutedUICommand("Navigate to", nameof(NavigateTo), typeof(RegionBrowser));
                    //注册热键
                    //_navigateTo.InputGestures.Add(new KeyGesture(Key.B,ModifierKeys.Alt));
                }
                return _navigateTo;
            }
        }
        #endregion

        public static RoutedCommand CloseViewCommand = new RoutedCommand();
        private static readonly DependencyPropertyKey RegionPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Region), typeof(IRegion), typeof(RegionBrowser), new PropertyMetadata(null));
        public static readonly DependencyProperty RegionProperty = RegionPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey ViewsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Views), typeof(IEnumerable), typeof(RegionBrowser), new PropertyMetadata(null));
        public static readonly DependencyProperty ViewsProperty = ViewsPropertyKey.DependencyProperty;
        public static readonly DependencyProperty BarLocationProperty =
            DependencyProperty.Register(nameof(BarLocation), typeof(Location), typeof(RegionBrowser), new PropertyMetadata(Location.Top));
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(RegionBrowser), new PropertyMetadata(null));

        static RegionBrowser()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RegionBrowser), new FrameworkPropertyMetadata(typeof(RegionBrowser)));
        }
        private ListBox _headerBox;
        public RegionBrowser()
        {
            this.CommandBindings.Add(new CommandBinding(CloseTab, OnCloseTabHandler));
        }
        public IRegion Region
        {
            get { return (IRegion)GetValue(RegionProperty); }
            protected set { SetValue(RegionPropertyKey, value); }
        }
        public IEnumerable Views
        {
            get { return (IEnumerable)GetValue(ViewsProperty); }
            protected set { SetValue(ViewsPropertyKey, value); }
        }
        public Location BarLocation
        {
            get { return (Location)GetValue(BarLocationProperty); }
            set { SetValue(BarLocationProperty, value); }
        }
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }
        private void ApplyRegion()
        {
            var regionManager = RegionManager.GetRegionManager(this);
            if (regionManager == null && CommonServiceLocator.ServiceLocator.IsLocationProviderSet)
                regionManager = CommonServiceLocator.ServiceLocator.Current.GetInstance<IRegionManager>();
            if (regionManager != null)
            {
                var regionName = RegionManager.GetRegionName(this);
                if (string.IsNullOrEmpty(regionName) == false)
                {
                    if (regionManager.Regions.ContainsRegionWithName(regionName))
                    {
                        InitializeRegion(regionManager.Regions[regionName]);
                    }
                    else
                    {
                        regionManager.Regions.CollectionChanged += (s, e) =>
                        {
                            if (e.NewItems != null)
                            {
                                foreach (IRegion region in e.NewItems)
                                {
                                    if (region.Name == regionName)
                                    {
                                        this.Dispatcher.BeginInvoke(new Action(() => InitializeRegion(region)));
                                        InitializeRegion(region);
                                        break;
                                    }
                                }
                            }
                        };
                    }
                }
            }
        }
        private void InitializeRegion(IRegion region)
        {
            if (Region == null && region != null)
            {
                Region = region;
                Views = region.Views;
            }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _headerBox = this.Template.FindName(PART_Header, this) as ListBox;
            if (_headerBox != null) _headerBox.SelectionChanged += (s, e) => OnHeaderChanged(_headerBox.SelectedItem);
            ApplyRegion();
        }
        protected void OnHeaderChanged(object header)
        {
            if (header != null)
            {
                var activeView = Region?.ActiveViews.FirstOrDefault();
                if (activeView!=header)
                    Region?.Activate(header);
            }
        }
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (_headerBox != null)
            {
                _headerBox.SelectedItem = newContent;
                _headerBox.ScrollIntoView(newContent);
            }
            base.OnContentChanged(oldContent, newContent);
        }
        private void OnCloseTabHandler(object sender, ExecutedRoutedEventArgs e)
        {
            var region = Region;
            if (region?.Views?.Contains(e.Parameter) == true)
            {
                region.Remove(e.Parameter);
                if (region.ActiveViews.Count() == 0)
                {
                    var view = region.Views.FirstOrDefault();
                    if (view != null)
                    {
                        region.Activate(view);
                    }
                }
            }
        }
    }

    public enum Location
    {
        Top,
        Left,
        Right,
        Bottom
    }
}
