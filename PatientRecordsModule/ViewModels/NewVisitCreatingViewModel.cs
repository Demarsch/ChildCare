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
    public class NewVisitCreatingViewModel : BindableBase, INotification, IPopupWindowActionAware
    {
        #region Fields
        private readonly IPatientRecordsService patientRecordsService;
        private readonly ILog logService;

        private CancellationTokenSource currentOperationToken;
        private readonly CommandWrapper reloadPatientDataCommandWrapper;
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
            CreateVisitCommand = new DelegateCommand(CreateVisit);
            VisitTemplates = new ObservableCollectionEx<CommonIdName>();
            Contracts = new ObservableCollectionEx<CommonIdName>();
            FinancingSources = new ObservableCollectionEx<CommonIdName>();
            Urgentlies = new ObservableCollectionEx<CommonIdName>();
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            reloadPatientDataCommandWrapper = new CommandWrapper
            {
                Command = new DelegateCommand(() => LoadCommonDataAsync()),
                CommandName = "Повторить",
            };
            LoadCommonDataAsync();
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
            set
            {
                visitId = value;
            }
        }

        public BusyMediator BusyMediator { get; set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }

        #endregion

        #region Commands
        public ICommand CreateVisitCommand { get; set; }
        private void CreateVisit()
        {
            VisitId = 1;
            HostWindow.Close();
        }
        #endregion

        #region Methods

        public async void LoadCommonDataAsync()
        {
            VisitTemplates.Clear();
            Contracts.Clear();
            FinancingSources.Clear();
            Urgentlies.Clear();
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;
            BusyMediator.Activate("Загрузка данных при создании случая...");
            logService.Info("Loading data for visit creating...");
            IDisposableQueryable<VisitTemplate> visitTemplatesQuery = null;
            IDisposableQueryable<RecordContract> recordContractsQuery = null;
            IDisposableQueryable<FinancingSource> financingSourcesQuery = null;
            IDisposableQueryable<Urgently> urgentliesQuery = null;
            DateTime curDate = DateTime.Now;
            try
            {
                visitTemplatesQuery = patientRecordsService.GetActualVisitTemplates(curDate);
                recordContractsQuery = patientRecordsService.GetActualRecordContracts(curDate);
                financingSourcesQuery = patientRecordsService.GetActualFinancingSources();
                urgentliesQuery = patientRecordsService.GetActualUrgentlies(curDate);

                var recordContracts = await recordContractsQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = (x.Number.HasValue ? "Договор №" + x.Number.ToString() + " - " : string.Empty) + x.ContractName,
                }).ToListAsync(token);
                loadingIsCompleted = true;
                Contracts.AddRange(recordContracts);

                var financingSources = await financingSourcesQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync(token);
                FinancingSources.AddRange(financingSources);

                var urgentlies = await urgentliesQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync(token);
                Urgentlies.AddRange(urgentlies);

                var visitTemplates = await visitTemplatesQuery.Select(x => new CommonIdName()
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync(token);
                VisitTemplates.AddRange(visitTemplates);
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data for visit creating");
                CriticalFailureMediator.Activate("Не удалость загрузить данные для создания случая. Попробуйте еще раз или обратитесь в службу поддержки", reloadPatientDataCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
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
            }
        }

        public async void SetFieldByVisitTemplateAsync(int visitTemplateId)
        {
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
                    x.UrgentlyId
                }).FirstOrDefaultAsync(token);
                SelectedContractId = visitTemplate.ContractId;
                SelectedFinancingSourceId = visitTemplate.FinancingSourceId;
                SelectedUrgentlyId = visitTemplate.UrgentlyId;
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
    }
}
