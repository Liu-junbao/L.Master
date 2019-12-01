using System;
using ModuleD.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleD
{
    public class ModuleD : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.RegisterViewNavigationWithRegion<ViewA>("MenuRegion", "ContentRegion", "模块D");
            containerProvider.NavigateToView<ViewA>("MenuRegion");
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