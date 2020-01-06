using System;
using ModuleC.ViewModels;
using ModuleC.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleC
{
    public class ModuleC : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.RegisterViewNavigationWithRegion<ViewA>("MenuRegion", "ContentRegion", "Netty.Socket");
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