using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Modularity;
using Shell.Shared;
using Microsoft.Practices.Unity;
using log4net;
using ReportingModule.Services;
using Prism.Regions;

namespace ReportingModule
{
    [Module(ModuleName = WellKnownModuleNames.ReportingModule)]
    public class Module : IModule
    {
        IUnityContainer container;
        IRegionManager regionManager;

        public Module(IUnityContainer container, IRegionManager regionManager)
        {
            this.container = container.CreateChildContainer();
            this.regionManager = regionManager;
        }

        public void Initialize()
        {
            //services
            container.RegisterInstance(LogManager.GetLogger("REPORTING"));
            container.RegisterType<IReportTemplateService, ReportTemplateService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IReportModuleFileOperations, ReportModuleFileOperations>(new ContainerControlledLifetimeManager());
            container.RegisterType<IReportGeneratorHelper, ReportGeneratorHelper>(new ContainerControlledLifetimeManager());

            //generators
            container.RegisterType<IReportGenerator, DocXReportGenerator>(new TransientLifetimeManager());
        }
    }
}
