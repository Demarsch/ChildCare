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
using Microsoft.Practices.Unity;
using Core.Wpf.Misc;

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

        private readonly PersonRecordEditorViewModel personRecordEditorViewModel;

        private readonly CommandWrapper reloadPatientVisitCompletedCommandWrapper;

        private readonly CommandWrapper reloadPatientRecordCompletedCommandWrapper;

        private int patientId;

        private CancellationTokenSource currentLoadingToken;
        #endregion

        #region Constructors
        public PersonRecordsHeaderViewModel(PersonRecordListViewModel personRecordListViewModel, PersonRecordEditorViewModel personRecordEditorViewModel, 
            IPatientRecordsService patientRecordsService, ILog logSevice, 
            IEventAggregator eventAggregator, IRegionManager regionManager, IViewNameResolver viewNameResolver, IUnityContainer container)
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
            if (personRecordEditorViewModel == null)
            {
                throw new ArgumentNullException("personRecordEditorViewModel");
            }
            this.personRecordEditorViewModel = personRecordEditorViewModel;
            this.personRecordEditorViewModel.PropertyChanged += personRecordEditorViewModel_PropertyChanged;
            this.personRecordListViewModel = personRecordListViewModel;
            this.patientRecordsService = patientRecordsService;
            this.logService = logSevice;
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.container = container;
            reloadPatientVisitCompletedCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => LoadVisitCompletedAsync(VisitId)) };
            reloadPatientRecordCompletedCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => LoadRecordCompletedAsync(VisitId)) };
            VisitTemplates = new ObservableCollectionEx<VisitTemplateDTO>();
            patientId = SpecialValues.NonExistingId;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            SubscribeToEvents();
            LoadItemsAsync();
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            UnsubscriveFromEvents();
            reloadPatientRecordCompletedCommandWrapper.Dispose();
            reloadPatientVisitCompletedCommandWrapper.Dispose();
        }

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Subscribe(OnPatientSelected);
            eventAggregator.GetEvent<SelectionEvent<Visit>>().Subscribe(OnVisitSelected);
            eventAggregator.GetEvent<SelectionEvent<Assignment>>().Subscribe(OnAssignmentSelected);
            eventAggregator.GetEvent<SelectionEvent<Record>>().Subscribe(OnRecordSelected);
        }

        private void OnRecordSelected(int recordId)
        {
            SetRVAIds(0, 0, recordId);
        }

        private void OnAssignmentSelected(int assignmentId)
        {
            SetRVAIds(0, assignmentId, 0);
        }

        private void OnVisitSelected(int visitId)
        {
            SetRVAIds(visitId, 0, 0);
        }

        private void SetRVAIds(int visitId, int assignmentId, int recordId)
        {
            VisitId = visitId;
            AssignmentId = assignmentId;
            RecordId = recordId;

            OnPropertyChanged(() => IsViewModeActive);
            OnPropertyChanged(() => IsEditModeActive);
        }

        private void OnPatientSelected(int patientId)
        {
            this.patientId = patientId;
            ActivatePersonRecords();
        }

        private void UnsubscriveFromEvents()
        {
            eventAggregator.GetEvent<SelectionEvent<Person>>().Unsubscribe(OnPatientSelected);
            eventAggregator.GetEvent<SelectionEvent<Visit>>().Subscribe(OnVisitSelected);
            eventAggregator.GetEvent<SelectionEvent<Assignment>>().Subscribe(OnAssignmentSelected);
            eventAggregator.GetEvent<SelectionEvent<Record>>().Subscribe(OnRecordSelected);
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
                var result = await loadVisitTemplatesTask;
                VisitTemplates.AddRange(result);
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

        private async void LoadVisitCompletedAsync(int visitId)
        {
            FailureMediator.Deactivate();
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading IsComleted property for visit with Id ={0}", visitId);
            IDisposableQueryable<Visit> visit = null;
            try
            {
                visit = patientRecordsService.GetVisit(visitId);
                var loadVisitTemplatesTask = visit.Select(x => x.IsCompleted).FirstOrDefaultAsync(token);
                var isCompletedResult = await loadVisitTemplatesTask;
                IsVisitCompleted = isCompletedResult;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load IsComleted property for visit with Id ={0}", visitId);
                FailureMediator.Activate("Не удалость состояние случая. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientVisitCompletedCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (visit != null)
                {
                    visit.Dispose();
                }
            }
        }

        private async void LoadRecordCompletedAsync(int recordId)
        {
            FailureMediator.Deactivate();
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate(string.Empty);
            logService.InfoFormat("Loading IsComleted property for record with Id ={0}", recordId);
            IDisposableQueryable<Record> record = null;
            try
            {
                record = patientRecordsService.GetRecord(recordId);
                var loadVisitTemplatesTask = record.Select(x => x.IsCompleted).FirstOrDefaultAsync(token);
                var isCompletedResult = await loadVisitTemplatesTask;
                IsRecordCompleted = isCompletedResult;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load IsComleted property for record with Id ={0}", recordId);
                FailureMediator.Activate("Не удалость состояние записи. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientRecordCompletedCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (record != null)
                {
                    record.Dispose();
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

        void personRecordEditorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsViewModeInCurrentProtocolEditor")
                OnPropertyChanged(() => IsViewModeActive);
            if (e.PropertyName == "IsEditModeInCurrentProtocolEditor")
                OnPropertyChanged(() => IsEditModeActive);
            if (e.PropertyName == "IsRecordCanBeCompleted")
                OnPropertyChanged(() => IsRecordCanBeCompletedEnabled);

            if (e.PropertyName == "AllowDocuments")
                OnPropertyChanged(() => AllowDocuments);
            if (e.PropertyName == "AllowDICOM")
                OnPropertyChanged(() => AllowDICOM);
            if (e.PropertyName == "CanAttachDICOM")
                OnPropertyChanged(() => CanAttachDICOM);
            if (e.PropertyName == "CanDetachDICOM")
                OnPropertyChanged(() => CanDetachDICOM);
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

        private int visitId;
        public int VisitId
        {
            get { return visitId; }
            set
            {
                SetProperty(ref visitId, value);
                OnPropertyChanged(() => IsVisitSelected);
                LoadVisitCompletedAsync(visitId);
            }
        }

        public bool IsVisitSelected
        {
            get { return VisitId > 0; }
        }

        private bool? isVisitCompleted;
        public bool? IsVisitCompleted
        {
            get { return isVisitCompleted; }
            set
            {
                SetProperty(ref isVisitCompleted, value);
                OnPropertyChanged(() => IsVisitCanBeClosed);
                OnPropertyChanged(() => IsVisitCanBeOpened);
            }
        }

        public bool IsVisitCanBeClosed
        {
            get { return IsVisitSelected && (IsVisitCompleted == null || IsVisitCompleted == false); }
        }

        public bool IsVisitCanBeOpened
        {
            get { return IsVisitSelected && IsVisitCompleted == true; }
        }

        private int assignmentId;
        public int AssignmentId
        {
            get { return assignmentId; }
            set
            {
                SetProperty(ref assignmentId, value);
                OnPropertyChanged(() => IsAssignmentSelected);
            }
        }

        public bool IsAssignmentSelected
        {
            get { return AssignmentId > 0; }
        }

        private int recordId;
        public int RecordId
        {
            get { return recordId; }
            set
            {
                SetProperty(ref recordId, value);
                OnPropertyChanged(() => IsRecordSelected);
                LoadRecordCompletedAsync(recordId);
            }
        }

        public bool IsRecordCanBeCompletedEnabled
        {
            get { return personRecordEditorViewModel.IsRecordCanBeCompleted; }
        }

        public bool IsRecordCanBeCompleted
        {
            get { return IsRecordSelected && (IsRecordCompleted == null || IsRecordCompleted == false); }
        }

        public bool IsRecordCanBeInProgress
        {
            get { return IsRecordSelected && IsRecordCompleted == true; }
        }

        private bool? isRecordCompleted;
        public bool? IsRecordCompleted
        {
            get { return isRecordCompleted; }
            set
            {
                SetProperty(ref isRecordCompleted, value);
                OnPropertyChanged(() => IsRecordCanBeCompleted);
                OnPropertyChanged(() => IsRecordCanBeInProgress);
            }
        }

        public bool IsViewModeActive
        {
            get { return (AssignmentId > 0 || RecordId > 0) && personRecordEditorViewModel.IsViewModeInCurrentProtocolEditor; }
        }

        public bool IsEditModeActive
        {
            get { return (AssignmentId > 0 || RecordId > 0) && personRecordEditorViewModel.IsEditModeInCurrentProtocolEditor; }
        }

        public bool IsRecordSelected
        {
            get { return RecordId > 0; }
        }

        private ObservableCollectionEx<VisitTemplateDTO> visitTemplates;
        public ObservableCollectionEx<VisitTemplateDTO> VisitTemplates
        {
            get { return visitTemplates; }
            set { SetProperty(ref visitTemplates, value); }
        }

        private bool allowDocuments;
        public bool AllowDocuments
        {
            get { return personRecordEditorViewModel.AllowDocuments; }
            set { SetProperty(ref allowDocuments, value); }
        }

        private bool allowDICOM;
        public bool AllowDICOM
        {
            get { return personRecordEditorViewModel.AllowDICOM; }
            set { SetProperty(ref allowDICOM, value); }
        }

        private bool canAttachDICOM;
        public bool CanAttachDICOM
        {
            get { return personRecordEditorViewModel.CanAttachDICOM; }
            set { SetProperty(ref canAttachDICOM, value); }
        }

        private bool canDetachDICOM;
        public bool CanDetachDICOM
        {
            get { return personRecordEditorViewModel.CanDetachDICOM; }
            set { SetProperty(ref canDetachDICOM, value); }
        }

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; set; }
        #endregion

        #region Events
        public event EventHandler IsActiveChanged = delegate { };
        #endregion

        #region Commands
        public ICommand CreateNewVisitCommand { get { return personRecordListViewModel.CreateNewVisitCommand; } }

        public ICommand EditVisitCommand { get { return personRecordListViewModel.EditVisitCommand; } }

        public ICommand CompleteVisitCommand { get { return personRecordListViewModel.CompleteVisitCommand; } }

        public ICommand ReturnToActiveVisitCommand { get { return personRecordListViewModel.ReturnToActiveVisitCommand; } }

        public ICommand CompleteRecordCommand { get { return personRecordListViewModel.CompleteRecordCommand; } }

        public ICommand InProgressRecordCommand { get { return personRecordListViewModel.InProgressRecordCommand; } }

        public ICommand PrintProtocolCommand { get { return personRecordEditorViewModel.PrintProtocolCommand; } }

        public ICommand SaveProtocolCommand { get { return personRecordEditorViewModel.SaveProtocolCommand; } }

        public ICommand ShowInEditModeCommand { get { return personRecordEditorViewModel.ShowInEditModeCommand; } }

        public ICommand ShowInViewModeCommand { get { return personRecordEditorViewModel.ShowInViewModeCommand; } }

        public ICommand AttachDocumentCommand { get { return personRecordEditorViewModel.AttachDocumentCommand; } }
        public ICommand DetachDocumentCommand { get { return personRecordEditorViewModel.DetachDocumentCommand; } }
        public ICommand AttachDICOMCommand { get { return personRecordEditorViewModel.AttachDICOMCommand; } }
        public ICommand DetachDICOMCommand { get { return personRecordEditorViewModel.DetachDICOMCommand; } }
               
        #endregion
    }
}
