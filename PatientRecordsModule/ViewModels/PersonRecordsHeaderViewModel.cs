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
using Microsoft.Practices.Unity;

namespace PatientRecordsModule.ViewModels
{
    public class PersonRecordsHeaderViewModel : BindableBase, IActiveAware, IDisposable
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;

        private readonly ILog logService;

        private readonly IEventAggregator eventAggregator;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private readonly IUnityContainer container;

        private readonly PersonRecordListViewModel personRecordListViewModel;

        private int patientId;

        private CancellationTokenSource currentLoadingToken;
        #endregion

        #region Constructors
        public PersonRecordsHeaderViewModel(PersonRecordListViewModel personRecordListViewModel, IPatientRecordsService patientRecordsService, ILog logSevice, IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver, IUnityContainer container)
        {
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (container == null)
            {
                throw new ArgumentNullException("container");
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
            if (personRecordListViewModel == null)
            {
                throw new ArgumentNullException("personRecordListViewModel");
            }
            this.personRecordListViewModel = personRecordListViewModel;
            this.patientRecordsService = patientRecordsService;
            this.logService = logSevice;
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.container = container;
            VisitTemplates = new ObservableCollectionEx<VisitTemplateDTO>();
            patientId = SpecialValues.NonExistingId;
            BusyMediator = new BusyMediator();
            SubscribeToEvents();            
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
            ActivatePersonRecords();
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Unsubscribe(OnPatientSelected);
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
                var loadVisitTemplatesTask = visitTemplates.Select(x => new VisitTemplateDTO()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Contract = x.ContractId.HasValue ? ((x.RecordContract.Number.HasValue ? "Договор №" + x.RecordContract.Number.ToString() + " - " : string.Empty) +
                        x.RecordContract.ContractName) : string.Empty,
                    FinancingSource = x.FinancingSource.Name,
                    Urgently = x.Urgently.Name
                }).ToListAsync(token);
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
                //FailureMediator.Activate("Не удалость загрузить шаблоны. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitsCommandWrapper, ex);
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

        private void ActivatePersonRecords()
        {
            if (patientId == SpecialValues.NonExistingId)
            {
                //regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<EmptyPatientInfoViewModel>());
            }
            else
            {
                var navigationParameters = new NavigationParameters { { "PatientId", patientId } };
                regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<PersonRecordsViewModel>(), navigationParameters);
            }
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
                        ActivatePersonRecords();
                    }
                }
            }
        }

        private ObservableCollectionEx<VisitTemplateDTO> visitTemplates;
        public ObservableCollectionEx<VisitTemplateDTO> VisitTemplates
        {
            get { return visitTemplates; }
            set { SetProperty(ref visitTemplates, value); }
        }

        public BusyMediator BusyMediator { get; set; }
        #endregion

        #region Events
        public event EventHandler IsActiveChanged = delegate { };
        #endregion

        #region Commands
        public ICommand CreateNewVisitCommand { get { return personRecordListViewModel.CreateNewVisitCommand; } }
        #endregion
    }
}
