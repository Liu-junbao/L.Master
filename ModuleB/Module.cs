using ModuleB.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleB
{
    [Module(ModuleName = nameof(ModuleB))]
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