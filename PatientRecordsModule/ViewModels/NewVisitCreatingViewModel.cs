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
                    OnPropertyChanged(() => IsNotSelectedVisitTemplate);
                    SetFieldByVisitTemplateAsync(SelectedVisitTemplateId.ToInt());
                }
            }
        }

        public bool IsNotSelectedVisitTemplate
        {
            get { return !SelectedVisitTemplateId.HasValue; }
        }

        public ObservableCollectionEx<CommonIdName> Contracts { get; set; }

        private int? selectedContractId;
        public int? SelectedContractId
        {
            get { return selectedContractId; }
            set { SetProperty(ref selectedContractId, value); }
        }

        public ObservableCollectionEx<CommonIdName> FinancingSources { get; set; }

        private int? selectedFinancingSourceId;
        public int? SelectedFinancingSourceId
        {
            get { return selectedFinancingSourceId; }
            set { SetProperty(ref selectedFinancingSourceId, value); }
        }

        public ObservableCollectionEx<CommonIdName> Urgentlies { get; set; }

        private int? selectedUrgentlyId;
        public int? SelectedUrgentlyId
        {
            get { return selectedUrgentlyId; }
            set { SetProperty(ref selectedUrgentlyId, value); }
        }

        public ObservableCollectionEx<CommonIdName> ExecutionPlaces { get; set; }

        private int? selectedExecutionPlaceId;
        public int? SelectedExecutionPlaceId
        {
            get { return selectedExecutionPlaceId; }
            set { SetProperty(ref selectedExecutionPlaceId, value); }
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

        private int visitId;
        public int VisitId
        {
            get { return visitId; }
            set { SetProperty(ref visitId, value); }
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
            logService.InfoFormat("Saving data for visit with Id = {0} for person with Id = {1}", VisitId > 0 ? VisitId.ToString() : "(new visit)", personId);
            BusyMediator.Activate("Сохранение изменений...");
            var saveSuccesfull = false;
            try
            {
                var visit = new Visit()
                {
                    Id = VisitId,
                    PersonId = PersonId,
                    ContractId = SelectedContractId.Value,
                    FinancingSourceId = SelectedFinancingSourceId.Value,
                    UrgentlyId = SelectedUrgentlyId.Value,
                    VisitTemplateId = SelectedVisitTemplateId.Value,
                    ExecutionPlaceId = SelectedExecutionPlaceId.Value,
                    SentLPUId = SelectedLPUId.Value,
                    BeginDateTime = Date,
                    Note = Note
                };

                var result = await patientRecordsService.SaveVisitAsync(visit, token);
                VisitId = result;
                saveSuccesfull = true;
            }
            catch (OperationCanceledException)
            {
                //Nothing to do as it means that we somehow cancelled save operation
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to save data for visit with Id = {0} for person with Id = {1}", VisitId > 0 ? VisitId.ToString() : "(new visit)", personId);
                CriticalFailureMediator.Activate("Не удалось сохранить данные случая. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {
                    //changeTracker.UntrackAll();
                }
            }
        }

        public ICommand CancelCommand { get; private set; }
        private void Cancel()
        {
            VisitId = -1;
            HostWindow.Close();
        }
        #endregion

        #region Methods

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
                SelectedContractId = visitTemplate.ContractId;
                SelectedFinancingSourceId = visitTemplate.FinancingSourceId;
                SelectedUrgentlyId = visitTemplate.UrgentlyId;
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
                    case "LastName":
                        //result = string.IsNullOrWhiteSpace(LastName) ? "Фамилия не указана" : string.Empty;
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
