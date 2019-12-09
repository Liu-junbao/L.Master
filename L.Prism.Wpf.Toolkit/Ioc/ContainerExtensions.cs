using Prism.Events;
using Prism.Modularity;
using Prism.Regions;
using System;
using Unity;

namespace Prism.Ioc
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// 获取依赖注入的对象
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TInstance GetInstance<TInstance>(this object obj)
        {
            if (CommonServiceLocator.ServiceLocator.IsLocationProviderSet)
            {
                return CommonServiceLocator.ServiceLocator.Current.GetInstance<TInstance>();
            }
            return default(TInstance);
        }

        #region Navigation
        /// <summary>
        /// 指定区域导航到指定页面
        /// </summary>
        /// <param name="view"></param>
        /// <param name="regionName"></param>
        public static void NavigateToView(this object view, string regionName)
        {
            view.GetInstance<IRegionManager>().RequestNavigate(regionName, view.GetType().FullName.Replace(".", "/"));
        }
        /// <summary>
        /// 指定区域导航到指定页面
        /// </summary>
        /// <param name="regionManager"></param>
        /// <param name="view"></param>
        /// <param name="regionName"></param>
        public static void NavigateToView(this IRegionManager regionManager, string regionName, object view)
        {
            regionManager.RequestNavigate(regionName, view.GetType().FullName.Replace(".", "/"));
        }
        /// <summary>
        /// 指定区域导航到指定页面
        /// </summary>
        /// <param name="view"></param>
        /// <param name="regionName"></param>
        public static void NavigateToView<TView>(this object obj, string regionName)
        {
            obj.GetInstance<IRegionManager>()?.RequestNavigate(regionName, typeof(TView).FullName.Replace(".", "/"), OnNavigated);
        }
        /// <summary>
        /// 指定区域导航到指定页面
        /// </summary>
        /// <typeparam name="TView"></typeparam>
        /// <param name="regionManager"></param>
        /// <param name="regionName"></param>
        public static void NavigateToView<TView>(this IRegionManager regionManager, string regionName)
        {
            regionManager.RequestNavigate(regionName, typeof(TView).FullName.Replace(".", "/"), OnNavigated);
        }
        /// <summary>
        /// 指定区域导航到指定页面
        /// </summary>
        /// <param name="regionManager"></param>
        /// <param name="regionName"></param>
        /// <param name="viewName"></param>
        public static void NavigateToView(this IRegionManager regionManager, string regionName, string viewName)
        {
            regionManager.RequestNavigate(regionName, viewName, OnNavigated);
        }

        /// <summary>
        /// 在区域内注册页面
        /// </summary>
        /// <typeparam name="TView"></typeparam>
        /// <param name="provider"></param>
        public static void RegisterViewWithRegion<TView>(this IContainerProvider provider, string regionName)
        {
            provider.Resolve<IRegionManager>().RegisterViewWithRegion(regionName, typeof(TView));
        }

        /// <summary>
        /// 在区域内注册Navigation导航菜单标签
        /// </summary>
        /// <typeparam name="TView"></typeparam>
        /// <param name="provider"></param>
        /// <param name="menuRegionName"></param>
        /// <param name="contentRegionName"></param>
        /// <param name="title"></param>
        /// <param name="toolTip"></param>
        /// <param name="moreSettings"></param>
        public static void RegisterViewNavigationWithRegion<TView>(this IContainerProvider provider, string menuRegionName,string contentRegionName, string title, string toolTip = null, Action<RegionNavigationItem<TView>> moreSettings = null)
        {
            provider.Resolve<IRegionManager>().RegisterViewWithRegion(menuRegionName, () =>
            {
                var item = new RegionNavigationItem<TView>() { TargetRegionName = contentRegionName, Title = title, ToolTip = string.IsNullOrEmpty(toolTip) ? title : toolTip };
                moreSettings?.Invoke(item);
                return item;
            });
        }


        /// <summary>
        /// 在区域内注册Navigation导航菜单标签
        /// </summary>
        /// <typeparam name="TView"></typeparam>
        /// <param name="module"></param>
        /// <param name="menuRegionName"></param>
        /// <param name="contentRegionName"></param>
        /// <param name="title"></param>
        /// <param name="toolTip"></param>
        /// <param name="moreSettings"></param>
        public static void RegisterViewNavigationWithRegion<TView>(this IModule module, string menuRegionName,string contentRegionName, string title, string toolTip = null, Action<RegionNavigationItem<TView>> moreSettings = null)
        {
            module.GetInstance<IRegionManager>().RegisterViewWithRegion(menuRegionName, () =>
            {
                var item = new RegionNavigationItem<TView>() { TargetRegionName = contentRegionName, Title = title, ToolTip = string.IsNullOrEmpty(toolTip) ? title : toolTip };
                moreSettings?.Invoke(item);
                return item;
            });
        }


        /// <summary>
        ///  在区域内注册页面
        /// </summary>
        /// <typeparam name="TView"></typeparam>
        /// <param name="module"></param>
        /// <param name="regionName"></param>
        public static void RegisterViewWithRegion<TView>(this IModule module, string regionName)
        {
            module.GetInstance<IRegionManager>().RegisterViewWithRegion(regionName, typeof(TView));
        }

        /// <summary>
        /// 在区域内注册页面
        /// </summary>
        /// <typeparam name="TView"></typeparam>
        /// <param name="regionManager"></param>
        /// <param name="regionName"></param>
        public static void RegisterViewWithRegion<TView>(this IRegionManager regionManager, string regionName)
        {
            regionManager.RegisterViewWithRegion(regionName, typeof(TView));
        }
        /// <summary>
        /// 注册页面
        /// </summary>
        /// <typeparam name="TView"></typeparam>
        /// <param name="registry"></param>
        /// <param name="isSingleView">页面是否单例</param>
        public static void RegisterView<TView>(this IContainerRegistry registry, bool isSingleView = true)
        {
            if (isSingleView) registry.RegisterSingleton<TView>();
            registry.RegisterForNavigation<TView>(typeof(TView).FullName.Replace(".", "/"));
        }
        private static void OnNavigated(NavigationResult obj)
        {
            obj.Publish();
        }
        #endregion

        #region Events
        /// <summary>
        /// 通知导航结果
        /// </summary>
        /// <param name="result"></param>
        public static void Publish(this NavigationResult result)
        {
            result.GetInstance<IEventAggregator>()?.GetEvent<PubSubEvent<NavigationResult>>().Publish(result);
        }
        /// <summary>
        /// 订阅导航事件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="onResult"></param>
        /// <param name="threadOption"></param>
        public static void SubscribeNavigated(this object obj, Action<NavigationResult> onResult, ThreadOption threadOption = ThreadOption.PublisherThread)
        {
            obj.GetInstance<IEventAggregator>()?.GetEvent<PubSubEvent<NavigationResult>>().Subscribe(onResult, threadOption);
        }
        /// <summary>
        /// 通知程序初始化完成
        /// </summary>
        /// <param name="app"></param>
        public static void PublishInitialized(this PrismApplicationBase app)
        {
            app.GetInstance<IEventAggregator>()?.GetEvent<PrismApplicationInitializedEvent>().Publish();
        }
        /// <summary>
        ///  订阅程序初始化完成事件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="onApplicationInitialized"></param>
        public static void SubscribeApplicationInitialized(this object obj, Action onApplicationInitialized)
        {
            var prismApplicationInitializedEvent = obj.GetInstance<IEventAggregator>()?.GetEvent<PrismApplicationInitializedEvent>();
            if (prismApplicationInitializedEvent != null)
            {
                Action action = null;
                action = () =>
                {
                    onApplicationInitialized?.Invoke();
                    prismApplicationInitializedEvent.Unsubscribe(action);
                };
                prismApplicationInitializedEvent.Subscribe(action, true);
            }
        }
        #endregion
    }

}
