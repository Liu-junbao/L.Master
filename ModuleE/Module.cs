using ModuleE.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleE
{
    [Module(ModuleName = nameof(ModuleE))]
    public class Module : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
          
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterView<ViewA>("ContentRegion");
        }
    }
}