using System;
using Microsoft.Practices.Unity;
using PatientSearchModule.Views;
using Prism.Modularity;
using Prism.Regions;
using Shell.Shared;

namespace PatientSearchModule
{
    [Module(ModuleName = ModuleName)]
    public class Module : IModule
    {
        public const string ModuleName = "Patient Search Module";

        private readonly IUnityContainer container;

        private readonly IRegionManager regionManager;

        public Module(IUnityContainer container, IRegionManager regionManager)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            this.regionManager = regionManager;
            this.container = container;
        }

        public void Initialize()
        {
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, () => container.Resolve<PatientSearch>());
        }
    }
}
