using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Core;
using DataLib;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;
using TestCore;
using System;
using Environment = Core.Environment;

namespace Registry
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var newCulture = new CultureInfo("ru-RU", true);
            CultureInfo.DefaultThreadCurrentCulture = newCulture;
            CultureInfo.DefaultThreadCurrentUICulture = newCulture;
            var lang = XmlLanguage.GetLanguage(newCulture.IetfLanguageTag);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(lang));
            FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(System.Windows.Documents.TextElement), new FrameworkPropertyMetadata(lang)); ;
            base.OnStartup(e);
            XmlConfigurator.Configure();

            var log = LogManager.GetLogger("Registry");
            var contextProvider = new ModelContextProvider() as IDataContextProvider;
            var personService = new PersonService(contextProvider);
            var securityService = new SecurityService(true) as ISecurityService;

            var cacheService = new DataContextCacheService(contextProvider) as ICacheService;
            Configuration.Initialize(cacheService);
            var environment = new Environment(contextProvider) as IEnvironment;
            var dialogService = new WindowDialogService() as IDialogService;

            var documentService = new DocumentService(contextProvider) as IDocumentService;
            var recordService = new RecordService(contextProvider) as IRecordService;

            var patientAssignmentService = new PatientAssignmentService(contextProvider) as IPatientAssignmentService;
            var patientAssignmentListViewModel = new PatientAssignmentListViewModel(patientAssignmentService, log, cacheService);

            var scheduleService = new ScheduleService(contextProvider, environment);
            var currentPatientAssignmentsViewModel = new CurrentPatientAssignmentsViewModel(patientAssignmentService, cacheService, log);
            var scheduleViewModel = new ScheduleViewModel(currentPatientAssignmentsViewModel, scheduleService, log, cacheService, environment, dialogService, securityService);

            var patientService = new PatientService(contextProvider) as IPatientService;
            var patientSearchViewModel = new PatientSearchViewModel(patientService, personService, log, dialogService, documentService, recordService, patientAssignmentListViewModel);
            var mainViewModel = new MainWindowViewModel(patientSearchViewModel, scheduleViewModel);
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
