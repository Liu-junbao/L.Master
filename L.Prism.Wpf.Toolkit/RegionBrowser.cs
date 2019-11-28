using CommonServiceLocator;
using Prism.Ioc;
using Prism.Regions;
using Prism.Regions.Behaviors;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Prism
{
    [ContentProperty(nameof(Child))]
    public class RegionBrowser :ContentControl
    {
        #region commands
        private static RoutedUICommand _closePage;
        private static RoutedUICommand _navigateTo;

        public static ICommand ClosePage
        {
            get
            {
                if (_closePage == null)
                {
                    _closePage = new RoutedUICommand("close page", nameof(ClosePage), typeof(RegionBrowser));
                }
                return _closePage;
            }
        }
        public static ICommand NavigateTo
        {
            get
            {
                if (_navigateTo == null)
                {
                    _navigateTo = new RoutedUICommand("navigate to", nameof(NavigateTo), typeof(RegionBrowser));
                }
                return _navigateTo;
            }
        }
        #endregion

        public static readonly DependencyProperty ChildProperty =
           DependencyProperty.Register(nameof(Child), typeof(object), typeof(RegionBrowser), new PropertyMetadata(null));
        private static readonly DependencyPropertyKey RegionPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Region), typeof(IRegion), typeof(RegionBrowser), new PropertyMetadata(null));
        public static readonly DependencyProperty RegionProperty = RegionPropertyKey.DependencyProperty;
        private static readonly DependencyPropertyKey ViewsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Views), typeof(IEnumerable), typeof(RegionBrowser), new PropertyMetadata(null));
        public static readonly DependencyProperty ActiveViewProperty =
            DependencyProperty.Register(nameof(ActiveView), typeof(object), typeof(RegionBrowser), new PropertyMetadata(null, OnActiveViewChanged));
        public static readonly DependencyProperty ViewsProperty = ViewsPropertyKey.DependencyProperty;
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(RegionBrowser), new PropertyMetadata(null));
        private static void OnActiveViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RegionBrowser)d)?.OnActiveViewChanged(e.NewValue);
        }
        static RegionBrowser()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RegionBrowser), new FrameworkPropertyMetadata(typeof(RegionBrowser)));
        }
        private IRegionManager _regionManager;
        public RegionBrowser()
        {
            this.CommandBindings.Add(new CommandBinding(ClosePage, OnClosePageHandler));
            this.CommandBindings.Add(new CommandBinding(NavigateTo, OnNavigateToHandler));
        }
        public object Child
        {
            get { return (object)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
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
        public object ActiveView
        {
            get { return (object)GetValue(ActiveViewProperty); }
            set { SetValue(ActiveViewProperty, value); }
        }
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ApplyRegion();
        }
        private void ApplyRegion()
        {
            var regionManager = RegionManager.GetRegionManager(this);
            if (regionManager == null)
                regionManager = this.GetInstance<IRegionManager>();
            if (regionManager != null)
            {
                _regionManager = regionManager;
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
                ActiveView = region.ActiveViews.FirstOrDefault();
                region.ActiveViews.CollectionChanged += (s, e) => this.Dispatcher.BeginInvoke(new Action(() => ActiveView = region.ActiveViews.FirstOrDefault()));
            }
        }
        private void OnActiveViewChanged(object activeView)
        {
            if (activeView != null)
            {
                var currActiveView = Region?.ActiveViews.FirstOrDefault();
                if (activeView != currActiveView)
                    this._regionManager?.NavigateToView(RegionManager.GetRegionName(this), activeView);
            }
        }
        private void OnClosePageHandler(object sender, ExecutedRoutedEventArgs e)
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
        private void OnNavigateToHandler(object sender, ExecutedRoutedEventArgs e)
        {
            var param = e.Parameter;
            if (param == null) return;
            if (param is string)
                this._regionManager?.NavigateToView(RegionManager.GetRegionName(this), param.ToString());
            else
                this._regionManager?.NavigateToView(RegionManager.GetRegionName(this), param);
        }
    }
    public class RegionBrowserBar : BrowserBar
    {
        static RegionBrowserBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RegionBrowserBar), new FrameworkPropertyMetadata(typeof(RegionBrowserBar)));
        }
    }
    public class RegionBrowserContentControl : ContentControl
    {
        static RegionBrowserContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RegionBrowserContentControl), new FrameworkPropertyMetadata(typeof(RegionBrowserContentControl)));
        }
    }
}
