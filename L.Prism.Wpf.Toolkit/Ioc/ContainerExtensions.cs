using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="view"></param>
        /// <param name="regionName"></param>
        public static void NavigateToView<TView>(this object obj, string regionName)
        {
            obj.GetInstance<IRegionManager>()?.RequestNavigate(regionName, typeof(TView).FullName.Replace(".", "/"), OnNavigated);
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
        public static void SubscribeApplicationInitialized(this object obj,Action onApplicationInitialized)
        {
            obj.GetInstance<IEventAggregator>()?.GetEvent<PrismApplicationInitializedEvent>().Subscribe(onApplicationInitialized,ThreadOption.UIThread);
        }
        #endregion
    }
}
