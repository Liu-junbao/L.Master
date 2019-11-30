using System;
using System.Threading.Tasks;
using ModuleA.ViewModels;
using ModuleA.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleA
{
    public class ModuleA : IModule
    {
        public  void OnInitialized(IContainerProvider containerProvider)
        {
            this.NavigateToView<ViewA>("ContentRegion");
            containerProvider.RegisterViewNavigationWithRegion<ViewA>("MenuRegion", "ContentRegion", "模块A");
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