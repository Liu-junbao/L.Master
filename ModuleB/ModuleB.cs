using System;
using ModuleB.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleB
{
    public class ModuleB : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.RegisterViewNavigationWithRegion<ViewA>("MenuRegion", "ContentRegion", "模块B");
            this.NavigateToView<ViewA>("ContentRegion");
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