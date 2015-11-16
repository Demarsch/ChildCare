using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Core.Reports.Services;
using Microsoft.Practices.Unity;

namespace Core.Reports
{
    public static class CoreReports
    {
        static IUnityContainer container;

        public static void Initialize(IUnityContainer unityContainer)
        {
            if (container != null)
                return;
            container = unityContainer.CreateChildContainer();
                                    
            //services
            container.RegisterInstance(LogManager.GetLogger("REPORTING"));
            container.RegisterType<IReportTemplateService, ReportTemplateService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IReportFileOperations, ReportFileOperations>(new ContainerControlledLifetimeManager());
            container.RegisterType<IReportGeneratorHelper, ReportGeneratorHelper>(new ContainerControlledLifetimeManager());

            //generators
            container.RegisterType<DocXReportGenerator>(new TransientLifetimeManager());
        }
    }
}
