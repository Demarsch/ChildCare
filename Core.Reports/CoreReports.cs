using Core.Extensions;
using Core.Reports.Services;
using Microsoft.Practices.Unity;
using System;
using System.Windows;

namespace Core.Reports
{
    public static class CoreReports
    {
        public static void Initialize(IUnityContainer container)
        {
            //services
            container.RegisterTypeIfMissing<IReportTemplateService, ReportTemplateService>(new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<IReportGeneratorHelper, ReportGeneratorHelper>(new ContainerControlledLifetimeManager());
            container.RegisterTypeIfMissing<IDocumentService, DocumentService>(new ContainerControlledLifetimeManager());

            //generators
            container.RegisterType<DocXReportGenerator>(new TransientLifetimeManager());
            container.RegisterType<PrintedDocumentsCollectionViewModel>(new TransientLifetimeManager());

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"pack://application:,,,/Core.Reports;Component/Themes/Generic.xaml", UriKind.Absolute) });
        }
    }
}
