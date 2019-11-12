using ModuleD.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleD
{
    [Module(ModuleName =nameof(ModuleD))]
    public class Module : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.RegisterView<ViewA>("MenuRegion");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterView<ViewA>("ContentRegion");
        }
    }
}