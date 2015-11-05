using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.PopupWindowActionAware;
using Core.Wpf.Services;
using log4net;
using PatientRecordsModule.DTOs;
using PatientRecordsModule.Services;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.Entity;
using Core.Extensions;
using Core.Wpf.Misc;
using Core.Misc;

namespace PatientRecordsModule.ViewModels
{
    public class NewVisitCreatingViewModel : BindableBase, INotification, IPopupWindowActionAware, IDataErrorInfo
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;
        private readonly ILog logService;

        private CancellationTokenSource currentOperationToken;
        private readonly CommandWrapper reloadPatientDataCommandWrapper;
        private readonly CommandWrapper reloadDataSourceCommandWrapper;
        private readonly CommandWrapper saveChangesCommandWrapper;
        private TaskCompletionSource<bool> dataSourcesLoadingTaskSource;

        private Visit visit;
        #endregion

        #region Constructors
        public NewVisitCreatingViewModel(IPatientRecordsService patientRecordsService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            this.patientRecordsService = patientRecordsService;
            this.logService = logService;
            CreateVisitCommand = new DelegateCommand(SaveChangesAsync);
            CancelCommand = new DelegateCommand(Cancel);
            VisitTemplates = new ObservableCollectionEx<CommonIdName>();
            Contracts = new ObservableCollectionEx<CommonIdName>();
            FinancingSources = new ObservableCollectionEx<CommonIdName>();
            Urgentlies = new ObservableCollectionEx<CommonIdName>();
            ExecutionPlaces = new ObservableCollectionEx<CommonIdName>();
            LPUs = new ObservableCollectionEx<CommonIdName>();
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            reloadPatientDataCommandWrapper = new CommandWrapper
            {
                Command = new DelegateCommand(() => SetFieldByVisitTemplateAsync(SelectedVisitTemplateId.ToInt())),
                CommandName = "Повторить",
            };
            reloadDataSourceCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => EnsureDataSourceLoaded()) };
            saveChangesCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => SaveChangesAsync()) };
        }
        #endregion

        #region Properties

        private DateTime date = DateTime.Now;
        public DateTime Date
        {
            get { return date; }
            set { SetProperty(ref date, value); }
        }

        public ObservableCollectionEx<CommonIdName> VisitTemplates { get; set; }

        private int? selectedVisitTemplateId;
        public int? SelectedVisitTemplateId
        {
            get { return selectedVisitTemplateId; }
            set
            {
                if (SetProperty(ref selectedVisitTemplateId, value))
                {
                    SetFieldByVisitTemplateAsync(SelectedVisitTemplateId.ToInt());
                }
                OnPropertyChanged(() => ContractEnabled);
                OnPropertyChanged(() => FinancingSourceEnabled);
                OnPropertyChanged(() => UrgentlyEnabled);
                OnPropertyChanged(() => ExecutionPlaceEnabled);
            }
        }

        public ObservableCollectionEx<CommonIdName> Contracts { get; set; }

        private int? selectedContractId;
        public int? SelectedContractId
        {
            get { return selectedContractId; }
            set
            {
                SetProperty(ref selectedContractId, value);
                OnPropertyChanged(() => ContractEnabled);
            }
        }

        private bool ContractSetByVisitTemplate { get; set; }

        public bool ContractEnabled
        {
            get { return !(SelectedVisitTemplateId.HasValue && SelectedContractId.HasValue && ContractSetByVisitTemplate); }
        }

        public ObservableCollectionEx<CommonIdName> FinancingSources { get; set; }

        private int? selectedFinancingSourceId;
        public int? SelectedFinancingSourceId
        {
            get { return selectedFinancingSourceId; }
            set
            {
                SetProperty(ref selectedFinancingSourceId, value);
                OnPropertyChanged(() => FinancingSourceEnabled);
            }
        }

        private bool FinancingSourceSetByVisitTemplate { get; set; }

        public bool FinancingSourceEnabled
        {
            get { return !(SelectedVisitTemplateId.HasValue && SelectedFinancingSourceId.HasValue && FinancingSourceSetByVisitTemplate); }
        }

        public ObservableCollectionEx<CommonIdName> Urgentlies { get; set; }

        private int? selectedUrgentlyId;
        public int? SelectedUrgentlyId
        {
            get { return selectedUrgentlyId; }
            set
            {
                SetProperty(ref selectedUrgentlyId, value);
                OnPropertyChanged(() => UrgentlyEnabled);
            }
        }

        private bool UrgentlySetByVisitTemplate { get; set; }

        public bool UrgentlyEnabled
        {
            get { return !(SelectedVisitTemplateId.HasValue && SelectedUrgentlyId.HasValue && UrgentlySetByVisitTemplate); }
        }

        public ObservableCollectionEx<CommonIdName> ExecutionPlaces { get; set; }

        private int? selectedExecutionPlaceId;
        public int? SelectedExecutionPlaceId
        {
            get { return selectedExecutionPlaceId; }
            set
            {
                SetProperty(ref selectedExecutionPlaceId, value);
                OnPropertyChanged(() => ExecutionPlaceEnabled);
            }
        }

        private bool ExecutionPlaceSetByVisitTemplate { get; set; }

        public bool ExecutionPlaceEnabled
        {
            get { return !(SelectedVisitTemplateId.HasValue && SelectedExecutionPlaceId.HasValue && ExecutionPlaceSetByVisitTemplate); }
        }

        public ObservableCollectionEx<CommonIdName> LPUs { get; set; }

        private int? selectedLPUId;
        public int? SelectedLPUId
        {
            get { return selectedLPUId; }
            set { SetProperty(ref selectedLPUId, value); }
        }

        private string note;
        public string Note
        {
            get { return note; }
            set { SetProperty(ref note, value); }
        }

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set { SetProperty(ref personId, value); }
        }

        public BusyMediator BusyMediator { get; set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

        #endregion

        #region Commands
        public ICommand CreateVisitCommand { get; private set; }
        private async void SaveChangesAsync()
        {
            CriticalFailureMediator.Deactivate();
            if (!IsValid)
            {
                return;
            }
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            string visitIdString = visit != null ? visit.Id.ToString() : "(new visit)";
            logService.InfoFormat("Saving data for visit with Id = {0} for person with Id = {1}", visitIdString, personId);
            BusyMediator.Activate("Сохранение изменений...");
            var saveSuccesfull = false;
            try
            {
                if (visit == null)
                    visit = new Visit()
                    {
                        MKB = string.Empty,
                        OKATO = string.Empty
                    };
                visit.PersonId = PersonId;
                visit.ContractId = SelectedContractId.Value;
                visit.FinancingSourceId = SelectedFinancingSourceId.Value;
                visit.UrgentlyId = SelectedUrgentlyId.Value;
                visit.VisitTemplateId = SelectedVisitTemplateId.Value;
                visit.ExecutionPlaceId = SelectedExecutionPlaceId.Value;
                visit.SentLPUId = SelectedLPUId.Value;
                visit.BeginDateTime = Date;
                visit.Note = Note;

                var result = await patientRecordsService.SaveVisitAsync(visit, token);
                visit.Id = result;
                saveSuccesfull = true;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to save data for visit with Id = {0} for person with Id = {1}", visitIdString, personId);
                CriticalFailureMediator.Activate("Не удалось сохранить данные случая. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {
                    //changeTracker.UntrackAll();
                    HostWindow.Close();
                }
            }
        }

        public ICommand CancelCommand { get; private set; }
        private void Cancel()
        {
            visit = null;
            HostWindow.Close();
        }
        #endregion

        #region Methods

        public void IntializeCreation(int personId, int? visitTemplateId, Visit visit, DateTime date, string title)
        {
            ContractSetByVisitTemplate = false;
            FinancingSourceSetByVisitTemplate = false;
            UrgentlySetByVisitTemplate = false;
            ExecutionPlaceSetByVisitTemplate = false;
            this.PersonId = personId;
            this.SelectedVisitTemplateId = visitTemplateId;
            this.visit = visit;
            this.Date = date;
            this.Title = title;
            BusyMediator.Deactivate();
            CriticalFailureMediator.Deactivate();
            CriticalFailureMediator = new CriticalFailureMediator();
        }

        private async Task<bool> EnsureDataSourceLoaded()
        {
            if (dataSourcesLoadingTaskSource != null)
            {
                return await dataSourcesLoadingTaskSource.Task;
            }
            dataSourcesLoadingTaskSource = new TaskCompletionSource<bool>();
            VisitTemplates.Clear();
            Contracts.Clear();
            FinancingSources.Clear();
            Urgentlies.Clear();
            BusyMediator.Activate("Загрузка данных при создании случая...");
            logService.Info("Loading data sources for visit creating...");
            IDisposableQueryable<VisitTemplate> visitTemplatesQuery = null;
            IDisposableQueryable<RecordContract> recordContractsQuery = null;
            IDisposableQueryable<FinancingSource> financingSourcesQuery = null;
            IDisposableQueryable<Urgently> urgentliesQuery = null;
            IDisposableQueryable<ExecutionPlace> executionPlacesQuery = null;
            IDisposableQueryable<Org> LPUsQuery = null;
            DateTime curDate = DateTime.Now;
            try
            {
                visitTemplatesQuery = patientRecordsService.GetActualVisitTemplates(curDate);
                recordContractsQuery = patientRecordsService.GetActualRecordContracts(curDate);
                financingSourcesQuery = patientRecordsService.GetActualFinancingSources();
                urgentliesQuery = patientRecordsService.GetActualUrgentlies(curDate);
                executionPlacesQuery = patientRecordsService.GetActualExecutionPlaces();
                LPUsQuery = patientRecordsService.GetLPUs();

                var executionPlaces = await executionPlacesQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();
                ExecutionPlaces.AddRange(executionPlaces);

                var recordContracts = await recordContractsQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = (x.Number.HasValue ? "Договор №" + x.Number.ToString() + " - " : string.Empty) + x.ContractName,
                }).ToListAsync();
                Contracts.AddRange(recordContracts);

                var financingSources = await financingSourcesQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();
                FinancingSources.AddRange(financingSources);

                var urgentlies = await urgentliesQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();
                Urgentlies.AddRange(urgentlies);
                var visitTemplates = await visitTemplatesQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();
                VisitTemplates.AddRange(visitTemplates);

                var lpus = await LPUsQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();
                LPUs.AddRange(lpus);

                logService.InfoFormat("Data sources for visit creating are successfully loaded");
                dataSourcesLoadingTaskSource.SetResult(true);
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources for visit creating");
                CriticalFailureMediator.Activate("Не удалость загрузить данные для создания случая. Попробуйте еще раз или обратитесь в службу поддержки", reloadDataSourceCommandWrapper, ex);
                dataSourcesLoadingTaskSource.SetResult(false);
            }
            finally
            {
                if (visitTemplatesQuery != null)
                {
                    visitTemplatesQuery.Dispose();
                }
                if (recordContractsQuery != null)
                {
                    recordContractsQuery.Dispose();
                }
                if (financingSourcesQuery != null)
                {
                    financingSourcesQuery.Dispose();
                }
                if (urgentliesQuery != null)
                {
                    urgentliesQuery.Dispose();
                }
                if (executionPlacesQuery != null)
                {
                    executionPlacesQuery.Dispose();
                }
                if (LPUsQuery != null)
                {
                    LPUsQuery.Dispose();
                }
                BusyMediator.Deactivate();
            }
            return await dataSourcesLoadingTaskSource.Task;
        }

        public async void SetFieldByVisitTemplateAsync(int visitTemplateId)
        {
            var dataSourcesLoaded = await EnsureDataSourceLoaded();
            ContractSetByVisitTemplate = false;
            FinancingSourceSetByVisitTemplate = false;
            UrgentlySetByVisitTemplate = false;
            ExecutionPlaceSetByVisitTemplate = false;
            if (!dataSourcesLoaded)
            {
                return;
            }
            if (visitTemplateId < 1)
                return;
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Заполнение данных из шаблона...");
            logService.InfoFormat("Loading data from visit template with id {0}...", visitTemplateId);
            IDisposableQueryable<VisitTemplate> visitTemplateQuery = null;
            DateTime curDate = DateTime.Now;
            try
            {
                visitTemplateQuery = patientRecordsService.GetVisitTemplate(visitTemplateId);
                var visitTemplate = await visitTemplateQuery.Select(x => new
                {
                    x.FinancingSourceId,
                    x.ContractId,
                    x.UrgentlyId,
                    x.ExecutionPlaceId
                }).FirstOrDefaultAsync(token);
                ContractSetByVisitTemplate = visitTemplate.ContractId.HasValue;
                SelectedContractId = visitTemplate.ContractId;
                FinancingSourceSetByVisitTemplate = visitTemplate.FinancingSourceId.HasValue;
                SelectedFinancingSourceId = visitTemplate.FinancingSourceId;
                UrgentlySetByVisitTemplate = visitTemplate.UrgentlyId.HasValue;
                SelectedUrgentlyId = visitTemplate.UrgentlyId;
                ExecutionPlaceSetByVisitTemplate = visitTemplate.ExecutionPlaceId.HasValue;
                SelectedExecutionPlaceId = visitTemplate.ExecutionPlaceId;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data from visit template with Id {0}", visitTemplateId);
                CriticalFailureMediator.Activate("Не удалость загрузить из шаблона случая. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientDataCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (visitTemplateQuery != null)
                {
                    visitTemplateQuery.Dispose();
                }
            }
        }

        #endregion

        #region IPopupWindowActionAware implementation
        public System.Windows.Window HostWindow { get; set; }

        public INotification HostNotification { get; set; }
        #endregion

        #region INotification implementation
        public object Content { get; set; }

        public string Title { get; set; }
        #endregion

        #region IDataErrorInfo implementation
        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        private bool IsValid
        {
            get
            {
                saveWasRequested = true;
                OnPropertyChanged(string.Empty);
                return invalidProperties.Count < 1;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                switch (columnName)
                {
                    case "SelectedContractId":
                        result = SelectedContractId == null ? "Не указан договор" : string.Empty;
                        break;
                    case "SelectedFinancingSourceId":
                        result = SelectedFinancingSourceId == null ? "Не указан источник финансирования" : string.Empty;
                        break;
                    case "SelectedExecutionPlaceId":
                        result = SelectedExecutionPlaceId == null ? "Не указано место выполнения" : string.Empty;
                        break;
                    case "SelectedUrgentlyId":
                        result = SelectedUrgentlyId == null ? "Не указана срочность" : string.Empty;
                        break;
                    case "SelectedLPUId":
                        result = SelectedLPUId == null ? "Не указано направившее ЛПУ" : string.Empty;
                        break;
                }
                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
