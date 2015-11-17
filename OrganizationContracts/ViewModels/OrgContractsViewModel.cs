using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using log4net;
using OrganizationContractsModule.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using Prism.Events;
using System.ComponentModel;
using Prism.Interactivity.InteractionRequest;

namespace OrganizationContractsModule.ViewModels
{
    public class OrgContractsViewModel : BindableBase, IConfirmNavigationRequest, INavigationAware, IDataErrorInfo
    {
        private readonly IContractService contractService;
        private readonly ILog log;
        private readonly ICacheService cacheService;
        private readonly IDocumentService documentService;
        private readonly IEventAggregator eventAggregator;
        private readonly Func<AddContractOrganizationViewModel> addContractOrganizationViewModelFactory;
        private readonly CommandWrapper reloadContractsDataCommandWrapper;
        private readonly CommandWrapper reloadDataSourcesCommandWrapper;
        private readonly ChangeTracker changeTracker;
        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; private set; }
        private CancellationTokenSource currentLoadingToken;
        public InteractionRequest<Confirmation> ConfirmationInteractionRequest { get; private set; }
        public InteractionRequest<Notification> NotificationInteractionRequest { get; private set; }
        public InteractionRequest<AddContractOrganizationViewModel> AddContractOrgInteractionRequest { get; private set; }

        public RecordDocumentsCollectionViewModel DocsViewer { get; set; }

        public OrgContractsViewModel(IContractService contractService, IDocumentService documentService, ILog log, ICacheService cacheService, 
            IEventAggregator eventAggregator, Func<AddContractOrganizationViewModel> addContractOrganizationViewModelFactory)
        {            
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (documentService == null)
            {
                throw new ArgumentNullException("documentService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }            
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (addContractOrganizationViewModelFactory == null)
            {
                throw new ArgumentNullException("addContractOrganizationViewModelFactory");
            }
            this.contractService = contractService;            
            this.log = log;
            this.cacheService = cacheService;
            this.documentService = documentService;            
            this.eventAggregator = eventAggregator;
            this.addContractOrganizationViewModelFactory = addContractOrganizationViewModelFactory;
            BusyMediator = new BusyMediator();
            DocsViewer = new RecordDocumentsCollectionViewModel(documentService, log);
            FailureMediator = new FailureMediator();
            changeTracker = new ChangeTracker();
            changeTracker.PropertyChanged += OnChangesTracked;
            reloadContractsDataCommandWrapper = new CommandWrapper { Command = new DelegateCommand(LoadContractsAsync) };
            reloadDataSourcesCommandWrapper = new CommandWrapper { Command = new DelegateCommand(LoadDataSources) };

            ConfirmationInteractionRequest = new InteractionRequest<Confirmation>();
            NotificationInteractionRequest = new InteractionRequest<Notification>();
            AddContractOrgInteractionRequest = new InteractionRequest<AddContractOrganizationViewModel>();

            addContractCommand = new DelegateCommand(AddContract);
            saveContractCommand = new DelegateCommand(SaveContract, CanSaveChanges);
            removeContractCommand = new DelegateCommand(RemoveContract, CanRemoveContract);
            addOrganizationCommand = new DelegateCommand(AddOrganization);
            IsContractSelected = false;

            saveChangesCommandWrapper = new CommandWrapper { Command = saveContractCommand };
            LoadDataSources();
        }

        #region Properties

        private ObservableCollectionEx<FieldValue> years;
        public ObservableCollectionEx<FieldValue> Years
        {
            get { return years; }
            set { SetProperty(ref years, value); }
        }

        private int selectedYear;
        public int SelectedYear
        {
            get { return selectedYear; }
            set
            {
                if (SetProperty(ref selectedYear, value))
                    LoadContractsAsync();
            }
        }

        private ObservableCollectionEx<FieldValue> filterFinSources;
        public ObservableCollectionEx<FieldValue> FilterFinSources
        {
            get { return filterFinSources; }
            set { SetProperty(ref filterFinSources, value); }
        }

        private int selectedFilterFinSourceId;
        public int SelectedFilterFinSourceId
        {
            get { return selectedFilterFinSourceId; }
            set
            {
                if (SetProperty(ref selectedFilterFinSourceId, value))
                    LoadContractsAsync();
            }
        }

        private ObservableCollectionEx<ContractViewModel> contracts;
        public ObservableCollectionEx<ContractViewModel> Contracts
        {
            get { return contracts; }
            set { SetProperty(ref contracts, value); }
        }

        private ContractViewModel selectedContract;
        public ContractViewModel SelectedContract
        {
            get { return selectedContract; }
            set
            {
                if (SetProperty(ref selectedContract, value))
                {
                    if (value != null)
                    {
                        IsContractSelected = true;
                        changeTracker.IsEnabled = false;
                        LoadContractData();
                        changeTracker.IsEnabled = true;
                    }
                    else
                        IsContractSelected = false;
                }
            }
        }

        private string number;
        public string Number
        {
            get { return number; }
            set { SetProperty(ref number, value); }
        }

        private ObservableCollectionEx<FieldValue> organizations;
        public ObservableCollectionEx<FieldValue> Organizations
        {
            get { return organizations; }
            set { SetProperty(ref organizations, value); }
        }

        private int selectedOrganizationId;
        public int SelectedOrganizationId
        {
            get { return selectedOrganizationId; }
            set 
            {
                changeTracker.Track(selectedOrganizationId, value); 
                SetProperty(ref selectedOrganizationId, value);
            }
        }

        private ObservableCollectionEx<FieldValue> finSources;
        public ObservableCollectionEx<FieldValue> FinSources
        {
            get { return finSources; }
            set { SetProperty(ref finSources, value); }
        }

        private int selectedFinSourceId;
        public int SelectedFinSourceId
        {
            get { return selectedFinSourceId; }
            set 
            {
                changeTracker.Track(selectedFinSourceId, value); 
                SetProperty(ref selectedFinSourceId, value);
            }
        }

        private ObservableCollectionEx<FieldValue> registrators;
        public ObservableCollectionEx<FieldValue> Registrators
        {
            get { return registrators; }
            set { SetProperty(ref registrators, value); }
        }

        private int selectedRegistratorId;
        public int SelectedRegistratorId
        {
            get { return selectedRegistratorId; }
            set 
            {
                changeTracker.Track(selectedRegistratorId, value); 
                SetProperty(ref selectedRegistratorId, value); 
            }
        }

        private DateTime contractBeginDate;
        public DateTime ContractBeginDate
        {
            get { return contractBeginDate; }
            set 
            {
                changeTracker.Track(contractBeginDate, value); 
                SetProperty(ref contractBeginDate, value); 
            }
        }

        private DateTime contractEndDate;
        public DateTime ContractEndDate
        {
            get { return contractEndDate; }
            set 
            {
                changeTracker.Track(contractEndDate, value); 
                SetProperty(ref contractEndDate, value); 
            }
        }

        private double cost;
        public double Cost
        {
            get { return cost; }
            set 
            {
                changeTracker.Track(cost, value); 
                SetProperty(ref cost, value); 
            }
        }

        private string info;
        public string Info
        {
            get { return info; }
            set 
            {
                changeTracker.Track(info, value); 
                SetProperty(ref info, value); 
            }
        }

        private bool isContractSelected;
        public bool IsContractSelected
        {
            get { return isContractSelected; }
            set { SetProperty(ref isContractSelected, value); }
        }
        #endregion

        private async void LoadDataSources()
        {
            FailureMediator.Deactivate();
            var contractRecord = await contractService.GetRecordTypesByOptions(OptionValues.Contract).FirstOrDefaultAsync();
            var reliableStaff = await contractService.GetRecordTypeRolesByOptions(OptionValues.ResponsibleForContract).FirstOrDefaultAsync();
            if (contractRecord == null || reliableStaff == null)
            {
                FailureMediator.Activate("В МИС не найдена информация об услуге 'Договор' и/или об ответственных за выполнение.", reloadDataSourcesCommandWrapper, new Exception("Отсутствует запись в таблицах RecordTypes, RecordTypeRoles"));
                return;
            }
            var personStaffs = await contractService.GetAllowedPersonStaffs(contractRecord.Id, reliableStaff.Id).ToArrayAsync();
            if (!personStaffs.Any())
            {
                FailureMediator.Activate("В МИС не найдена информация о правах на выполнение услуги.", reloadDataSourcesCommandWrapper, new Exception("Отсутствует запись в таблице RecordTypeRolePermissions"));
                return;
            }
            List<FieldValue> users = new List<FieldValue>();
            users.Add(new FieldValue() { Value = -1, Field = "- выберите ответственного -" });
            users.AddRange(personStaffs.Select(x => new FieldValue() { Value = x.Id, Field = x.Person.ShortName }));
            Registrators = new ObservableCollectionEx<FieldValue>(users);

            List<FieldValue> finSources = new List<FieldValue>();
            var fSources = await contractService.GetActiveFinancingSources().ToArrayAsync();
            finSources.Add(new FieldValue() { Value = -1, Field = "- выберите ист. финансирования -" });
            finSources.AddRange(fSources.Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
            FinSources = new ObservableCollectionEx<FieldValue>(finSources);
            FilterFinSources = new ObservableCollectionEx<FieldValue>(finSources);

            List<FieldValue> orgs = new List<FieldValue>();
            var orgsSource = await contractService.GetOrganizations().ToArrayAsync();
            orgs.Add(new FieldValue() { Value = -1, Field = "- выберите организацию -" });            
            orgs.AddRange(orgsSource.Where(x => x.UseInContract).Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
            Organizations = new ObservableCollectionEx<FieldValue>(orgs);
            SelectedOrganizationId = SpecialValues.NonExistingId;

            Contracts = new ObservableCollectionEx<ContractViewModel>();

            List<FieldValue> elements = new List<FieldValue>();
            elements.Add(new FieldValue() { Value = -1, Field = "- все -" });
            int begin = DateTime.Now.Year - 10;
            int end = DateTime.Now.Year + 10;
            for(int i = begin; i < end; i++)
                elements.Add(new FieldValue() { Value = i, Field = i + " год" });
            Years = new ObservableCollectionEx<FieldValue>(elements);
            SelectedYear = DateTime.Now.Year;
            SelectedFilterFinSourceId = SpecialValues.NonExistingId;
        }

        private async void LoadContractsAsync()
        {            
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            FailureMediator.Deactivate();
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate("Загрузка договоров...");
            log.InfoFormat("Loading org contracts...", "");
            try
            {
                var result = await contractService
                                .GetContractsWithOrgs(new DateTime(selectedYear, 1, 1), new DateTime(selectedYear, 12, 31), selectedFilterFinSourceId)
                                .OrderBy(x => x.BeginDateTime)
                                .Select(x => new
                                {
                                    Id = x.Id,
                                    ContractNumber = x.Number,
                                    Organization = x.Org.Name,
                                    BeginDate = x.BeginDateTime,
                                    EndDate = x.EndDateTime
                                }).ToArrayAsync(token);

                Contracts.Clear();
                Contracts.AddRange(result.Select(x => 
                    new ContractViewModel()
                    {
                        Id = x.Id,
                        ContractNumber = x.ContractNumber.ToSafeString(),
                        OrganizationName = x.Organization,
                        BeginDate = x.BeginDate,
                        EndDate = x.EndDate
                    }));
                if (contracts.Any())
                    SelectedContract = contracts.OrderByDescending(x => x.BeginDate).First();
                else
                    IsContractSelected = false;
                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load org contracts", "");
                FailureMediator.Activate("Не удалость загрузить договора с юр. лицами. Попробуйте еще раз или обратитесь в службу поддержки", reloadContractsDataCommandWrapper, ex);
                loadingIsCompleted = false;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                    saveContractCommand.RaiseCanExecuteChanged();
                    removeContractCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void LoadContractData()
        {
            if (SelectedContract.Id == SpecialValues.NewId)
                ClearData();
            else
            {
                var contract = contractService.GetContractById(selectedContract.Id).First();
                Number = contract.DisplayName;
                SelectedFinSourceId = contract.FinancingSourceId;
                SelectedOrganizationId = contract.OrgId.Value;
                ContractBeginDate = contract.BeginDateTime;
                ContractEndDate = contract.EndDateTime;
                Cost = contract.ContractCost;
                Info = contract.OrgDetails;
                SelectedRegistratorId = contract.InUserId;                
            }
            DocsViewer.LoadDocs(2);
        }

        private void ClearData()
        {
            Number = "НОВЫЙ ДОГОВОР";
            SelectedOrganizationId = -1;
            SelectedFinSourceId = -1;
            SelectedRegistratorId = -1;
            ContractBeginDate = DateTime.Now;
            ContractEndDate = DateTime.Now;
            Cost = 0;
            Info = string.Empty;
        }

        private bool CanSaveChanges()
        {
            if (selectedContract == null)
            {
                return false;
            }
            return changeTracker.HasChanges;
        }

        private bool CanRemoveContract()
        {
            if (selectedContract == null)
            {
                return false;
            }
            return true;
        }

        private void AddContract()
        {
            if (Contracts == null || Contracts.Any(x => x.Id == 0)) return;
            Contracts.Add(new ContractViewModel()
            {
                Id = 0,
                ContractNumber = string.Empty,
                OrganizationName = "НОВЫЙ ДОГОВОР"
            });
            SelectedContract = contracts.First(x => x.Id == 0);
            saveContractCommand.RaiseCanExecuteChanged();
            removeContractCommand.RaiseCanExecuteChanged();
        }

        private int FirstUnused(int[] numbers)
        {
            int i = 1;
            foreach (int t in numbers.Distinct().OrderBy(x => x))
            {
                if (t != i) break;
                i++;
            }
            return i;
        }

        private readonly CommandWrapper saveChangesCommandWrapper;
        private void SaveContract()
        {
            FailureMediator.Deactivate();
            if (!IsValid)
            {
                FailureMediator.Activate("Проверьте правильность заполнения полей.", null, null, true);
                return;
            }
            RecordContract contract = new RecordContract();
            if (selectedContract.Id != SpecialValues.NewId)
                contract = contractService.GetContractById(selectedContract.Id).First();
            log.InfoFormat("Saving contract data with Id {0} for Org", ((contract == null || contract.Id == SpecialValues.NewId) ? "(New contract)" : contract.Id.ToString()));
            BusyMediator.Activate("Сохранение изменений...");
            var saveSuccesfull = false;

            if (!contract.Number.HasValue)
            {
                DateTime beginYear = new DateTime(contractBeginDate.Year, 1, 1);
                DateTime endYear = new DateTime(contractEndDate.Year, 12, 31);
                contract.Number = FirstUnused(contractService.GetContractsWithOrgs(beginYear, endYear).Select(x => x.Number.Value).ToArray());
            }
            contract.BeginDateTime = contractBeginDate;
            contract.EndDateTime = contractEndDate;
            contract.FinancingSourceId = selectedFinSourceId;
            contract.ClientId = (int?)null;
            contract.ConsumerId = (int?)null;
            contract.ContractName = contractService.GetOrganizationById(selectedOrganizationId).First().Name;
            contract.PaymentTypeId = contractService.GetPaymentTypes().First(x => x.Options.Contains(OptionValues.Cashless)).Id;
            contract.TransactionNumber = string.Empty;
            contract.TransactionDate = string.Empty;            
            contract.Priority = 1;
            contract.InUserId = selectedRegistratorId;
            contract.InDateTime = DateTime.Now;
            contract.OrgId = selectedOrganizationId;
            contract.OrgDetails = info;
            contract.ContractCost = cost;
            contract.Options = string.Empty;

            try
            {
                contract.Id = contractService.SaveContractData(contract);
                saveSuccesfull = true;
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to save RecordContract with Id {0} for Org", ((contract == null || contract.Id == SpecialValues.NewId) ? "(New contract)" : contract.Id.ToString()));
                FailureMediator.Activate("Не удалось сохранить договор. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex);
                return;
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {
                    selectedContract.Id = contract.Id;
                    selectedContract.ContractNumber = contract.Number.ToSafeString();
                    selectedContract.OrganizationName = contract.ContractName;
                    selectedContract.BeginDate = contract.BeginDateTime;
                    selectedContract.EndDate = contract.EndDateTime;
                    Number = contract.DisplayName;
                    changeTracker.UntrackAll();
                    saveContractCommand.RaiseCanExecuteChanged();
                    removeContractCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void RemoveContract()
        {
            if (selectedContract == null)
            {
                NotificationInteractionRequest.Raise(new Notification()
                         {
                             Title = "Внимание",
                             Content = "Не выбран договор."
                         });
                return;
            }
            ConfirmationInteractionRequest.Raise(new Confirmation()
            {
                Title = "Внимание",
                Content = "Вы уверены, что хотите удалить договор?"
            }, OnDialogClosed);
        }

        private void OnDialogClosed(Confirmation confirmation)
        {
            if (confirmation.Confirmed)
            {
                var visit = contractService.GetVisitsByContractId(selectedContract.Id).FirstOrDefault();
                if (visit != null)
                {
                    NotificationInteractionRequest.Raise(new Notification()
                    {
                        Title = "Внимание",
                        Content = "Данный договор уже закреплен за случаем обращения пациента \"" + visit.VisitTemplate.ShortName + "\". Удаление договора невозможно."
                    }, (notification) => { return; });
                }
                if (selectedContract.Id != SpecialValues.NewId)
                    contractService.DeleteContract(selectedContract.Id);
                Contracts.Remove(selectedContract);
                saveContractCommand.RaiseCanExecuteChanged();
                removeContractCommand.RaiseCanExecuteChanged();
            }
        }

        private void AddOrganization()
        {
            var addContractOrganizationViewModel = addContractOrganizationViewModelFactory();
            addContractOrganizationViewModel.IntializeCreation("Добавить организацию");
            AddContractOrgInteractionRequest.Raise(addContractOrganizationViewModel, OnAddOrgDialogClosed);
        }

        private void OnAddOrgDialogClosed(AddContractOrganizationViewModel viewModel)
        {
            if (!viewModel.saveSuccesfull) return;
            Organizations.Add(new FieldValue() { Value = viewModel.orgId, Field = contractService.GetOrganizationById(viewModel.orgId).First().Name });
            if (SelectedContract.Id == SpecialValues.NewId)
                SelectedOrganizationId = viewModel.orgId;
        }

        private readonly DelegateCommand addContractCommand;
        private readonly DelegateCommand saveContractCommand;
        private readonly DelegateCommand removeContractCommand;
        private readonly DelegateCommand addOrganizationCommand;

        public ICommand AddContractCommand { get { return addContractCommand; } }
        public ICommand SaveContractCommand { get { return saveContractCommand; } }
        public ICommand RemoveContractCommand { get { return removeContractCommand; } }
        public ICommand AddOrganizationCommand { get { return addOrganizationCommand; } }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                saveContractCommand.RaiseCanExecuteChanged();
                removeContractCommand.RaiseCanExecuteChanged();
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }
               
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            //We use only one view-model for patient info, thus we says that current view-model can accept navigation requests
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //TODO: place here logic for current view being deactivated
        }

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            //TODO: probably implement proper logic
            continuationCallback(true);
        }

        #region Inplementation IDataErrorInfo

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
                    case "SelectedFinSourceId":
                        result = SelectedFinSourceId == SpecialValues.NonExistingId ? "Укажите источник финансирования" : string.Empty;
                        break;
                    case "SelectedRegistratorId":
                        result = SelectedRegistratorId == SpecialValues.NonExistingId ? "Укажите ответственного за договор" : string.Empty;
                        break;
                    case "SelectedOrganizationId":
                        result = SelectedOrganizationId == SpecialValues.NonExistingId ? "Укажите организацию, с которой заключен договор" : string.Empty;
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
