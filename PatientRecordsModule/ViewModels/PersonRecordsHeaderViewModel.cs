using System;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Wpf.Events;
using Core.Wpf.Services;
using log4net;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;
using Core.Wpf.Mvvm;
using System.Threading;
using System.Threading.Tasks;
using Core.Misc;
using System.Linq;
using PatientRecordsModule.Services;
using System.Data.Entity;
using Core.Extensions;
using PatientRecordsModule.DTO;
using PatientRecordsModule.DTOs;

namespace PatientRecordsModule.ViewModels
{
    public class PersonRecordsHeaderViewModel : BindableBase, IDisposable, IActiveAware
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;

        private readonly ILog logService;

        private readonly IEventAggregator eventAggregator;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private int patientId;

        private CancellationTokenSource currentLoadingToken;
        #endregion

        #region Constructors
        public PersonRecordsHeaderViewModel(IPatientRecordsService patientRecordsService, ILog logSevice, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver)
        {
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (logSevice == null)
            {
                throw new ArgumentNullException("log");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            this.patientRecordsService = patientRecordsService;
            this.logService = logSevice;
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            VisitTemplates = new ObservableCollectionEx<CommonIdName>();
            patientId = SpecialId.NonExisting;
            SubscribeToEvents();
            BusyMediator = new BusyMediator();
            createNewVisitCommand = new DelegateCommand<CommonIdName>(CreateNewVisit);
            LoadItemsAsync();
        }

        #endregion

        #region Methods
        public void Dispose()
        {
            UnsubscriveFromEvents();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Subscribe(OnPatientSelected);
        }

        private void OnPatientSelected(int patientId)
        {
            this.patientId = patientId;
            LoadSelectedPatientData();
            ActivatePatientInfo();
        }

        private void LoadSelectedPatientData()
        {
            //TODO:
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Unsubscribe(OnPatientSelected);
        }

        private void ActivatePatientInfo()
        {
            if (patientId == SpecialId.NonExisting)
            {
                //regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<EmptyPatientInfoViewModel>());
            }
            else
            {
                var navigationParameters = new NavigationParameters { { "PatientId", patientId } };
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<PersonRecordsViewModel>(), navigationParameters);
            }
        }

        private async void LoadItemsAsync()
        {
            var loadingIsCompleted = false;
            VisitTemplates.Clear();
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.Info("Loading visit templates...");
            IDisposableQueryable<VisitTemplate> visitTemplates = null;
            try
            {
                visitTemplates = patientRecordsService.GetActualVisitTemplates(DateTime.Now);
                var loadVisitTemplatesTask = visitTemplates.Select(x => new CommonIdName() { Id = x.Id, Name = x.Name }).ToListAsync(token);
                await Task.WhenAll(loadVisitTemplatesTask, Task.Delay(AppConfiguration.PendingOperationDelay, token));
                VisitTemplates.AddRange(loadVisitTemplatesTask.Result);
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load visit templates");
                //CriticalFailureMediator.Activate("Не удалость загрузить шаблоны. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (visitTemplates != null)
                {
                    visitTemplates.Dispose();
                }
            }
        }

        private void CreateNewVisit(CommonIdName selectedTemplate)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Properties
        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    IsActiveChanged(this, EventArgs.Empty);
                    OnPropertyChanged(() => IsActive);
                    if (value)
                    {
                        ActivatePatientInfo();
                    }
                }
            }
        }

        private ObservableCollectionEx<CommonIdName> visitTemplates;
        public ObservableCollectionEx<CommonIdName> VisitTemplates
        {
            get { return visitTemplates; }
            set { SetProperty(ref visitTemplates, value); }
        }

        private CommonIdName selectedVisitTemplate;
        public CommonIdName SelectedVisitTemplate
        {
            get { return selectedVisitTemplate; }
            set { SetProperty(ref selectedVisitTemplate, value); }
        }

        public BusyMediator BusyMediator { get; set; }
        #endregion

        #region Events
        public event EventHandler IsActiveChanged = delegate { };
        #endregion

        #region Commands
        private readonly DelegateCommand<CommonIdName> createNewVisitCommand;
        public ICommand CreateNewVisitCommand { get { return createNewVisitCommand; } }
        #endregion
    }
}
