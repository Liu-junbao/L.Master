using Prism.Ioc;
using MasterDemo.Views;
using System.Windows;
using Prism.Modularity;

namespace MasterDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        { 
            return Container.Resolve<MainWindow>();
        }
        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = "." };
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.PublishInitialized();
        }
    }
}
