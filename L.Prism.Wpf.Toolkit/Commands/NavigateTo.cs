using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;

namespace Prism
{
    public class NavigateTo : ICommand
    {
        public NavigateTo(IRegionManager regionManager)
        {
            RegionManager = regionManager;
        }
        public IRegionManager RegionManager { get; }
        public string ViewName { get; set; }
        public string RegionName { get; set; }
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            var regionName = RegionName;
            IRegion region = null;
            if (string.IsNullOrEmpty(regionName))
                region = this.RegionManager.Regions.FirstOrDefault();
            else if (this.RegionManager.Regions.ContainsRegionWithName(regionName))
                region = this.RegionManager.Regions[regionName];
            if (region != null)
            {
                var viewName = ViewName;
                if (string.IsNullOrEmpty(viewName))
                    viewName = parameter?.ToString();
                if (string.IsNullOrEmpty(viewName) == false)
                    region.RequestNavigate(viewName, i => i.Publich());
            }
        }
        public event EventHandler CanExecuteChanged;
    }
    public class NavigateToExtension : InstanceExtenions<NavigateTo>
    {
        public NavigateToExtension() { }
        public NavigateToExtension(string viewName)
        {
            ViewName = viewName;
        }
        public NavigateToExtension(string regionName,string viewName)
        {
            RegionName = regionName;
            ViewName = viewName;
        }
        [ConstructorArgument("viewName")]
        public string ViewName { get; set; }
        [ConstructorArgument("regionName")]
        public string RegionName { get; set; }
        protected override void OnInitialize(NavigateTo value)
        {
            value.ViewName = ViewName;
            value.RegionName = RegionName;
            base.OnInitialize(value);
        }
    }
}
