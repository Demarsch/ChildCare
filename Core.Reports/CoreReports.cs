using Core.Extensions;
using Core.Reports.Services;
using Microsoft.Practices.Unity;

namespace Core.Reports
{
    public static class CoreReports
    {
        public static void Initialize(IUnityContainer container)
        {
            //services
            container.RegisterTypeIfMissing<IReportTemplateService, ReportTemplateService>(new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<IReportGeneratorHelper, ReportGeneratorHelper>(new ContainerControlledLifetimeManager());

            //generators
            container.RegisterType<DocXReportGenerator>(new TransientLifetimeManager());
        }
    }
}
