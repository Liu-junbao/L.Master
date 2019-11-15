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