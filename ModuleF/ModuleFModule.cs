using ModuleF.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ModuleF
{
    public class ModuleFModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterView<ViewA>();
        }
    }
}