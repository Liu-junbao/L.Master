using System;
using ModuleE.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleE
{
    public class ModuleE : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.RegisterViewNavigationWithRegion<ViewA>("MenuRegion", "ContentRegion", "模块E");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterView<ViewA>();
            this.SubscribeApplicationInitialized(OnInitialized);
        }

        private void OnInitialized()
        {
           
        }
    }
}