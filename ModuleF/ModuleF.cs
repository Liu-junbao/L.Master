using System;
using ModuleF.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleF
{
    public class ModuleF : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.RegisterViewNavigationWithRegion<ViewA>("MenuRegion", "ContentRegion", "模块F");
            this.SubscribeApplicationInitialized(OnApplicationInitialized);
        }

        private void OnApplicationInitialized()
        {
           
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterView<ViewA>();
        }
    }
}