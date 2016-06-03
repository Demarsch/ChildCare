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
using Core.Wpf.Services;
using OrganizationContractsModule.SuggestionsProviders;
using System.Collections.Specialized;

namespace OrganizationContractsModule.ViewModels
{
    public class OrgContractsViewModel : BindableBase, IConfirmNavigationRequest, IDataErrorInfo, IDisposable
    {
        private readonly IContractService contractService;
        private readonly ILog log;
        private readonly IDialogServiceAsync dialogService;   
        private readonly IDialogService messageService;   
        private readonly Func<AddContractOrganizationViewModel> addContractOrganizationViewModelFactory;
        private readonly Func<AddRecordViewModel> addRecordViewModelFactory;
        private readonly CommandWrapper reloadContractsDataCommandWrapper;
        private readonly CommandWrapper reloadDataSourcesCommandWrapper;
        private readonly ChangeTracker changeTracker;
        private readonly CompositeChangeTracker recordsChangeTracker;
        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; private set; }
        private CancellationTokenSource currentLoadingToken;

        public OrgContractsViewModel(IContractService contractService, ILog log, IDialogServiceAsync dialogService, IDialogService messageService,
                                     Func<AddContractOrganizationViewModel> addContractOrganizationViewModelFactory,
                                     Func<AddRecordViewModel> addRecordViewModelFactory)
        {    
            if (contractService == null)
            {
                throw new ArgumentNullException("contractService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }   
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            } 
            if (addContractOrganizationViewModelFactory == null)
            {
                throw new ArgumentNullException("addContractOrganizationViewModelFactory");
            }
            this.contractService = contractService;            
            this.log = log;
            this.dialogService = dialogService;
            this.messageService = messageService;
            this.addContractOrganizationViewModelFactory = addContractOrganizationViewModelFactory;
            this.addRecordViewModelFactory = addRecordViewModelFactory;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            Records = new ObservableCollectionEx<RecordTypeViewModel>();
            changeTracker = new ChangeTracker();
            changeTracker.PropertyChanged += OnChangesTracked;
            Records.BeforeCollectionChanged += Records_BeforeCollectionChanged;

            recordsChangeTracker = new CompositeChangeTracker(new ObservableCollectionChangeTracker<RecordTypeViewModel>(Records));

            PersonSuggestionsProvider = new PersonSuggestionsProvider(contractService);
            reloadContractsDataCommandWrapper = new CommandWrapper { Command = new DelegateCommand(LoadContractsAsync), CommandName = "Повторить" };
            reloadDataSourcesCommandWrapper = new CommandWrapper { Command = new DelegateCommand(LoadDataSources), CommandName = "Повторить" };
            
            addContractCommand = new DelegateCommand(AddContract);
            saveContractCommand = new DelegateCommand(SaveContract, CanSaveChanges);
            removeContractCommand = new DelegateCommand(RemoveContract, CanRemoveContract);
            addOrganizationCommand = new DelegateCommand(AddOrganization);

            addRecordCommand = new DelegateCommand(AddRecord);
            removeRecordCommand = new DelegateCommand(RemoveRecord);

            IsContractSelected = false;

            saveChangesCommandWrapper = new CommandWrapper { Command = saveContractCommand };
            LoadDataSources();
        }

        void Records_BeforeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<RecordTypeViewModel>())
                    recordsChangeTracker.AddTracker(newItem.ChangeTracker);
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<RecordTypeViewModel>())
                    recordsChangeTracker.RemoveTracker(oldItem.ChangeTracker);
            }
        }
        
        #region Properties

        private DateTime filterBeginDate;
        public DateTime FilterBeginDate
        {
            get { return filterBeginDate; }
            set
            {
                if (SetProperty(ref filterBeginDate, value) && value <= FilterEndDate)
                    LoadContractsAsync();
            }
        }

        private DateTime filterEndDate;
        public DateTime FilterEndDate
        {
            get { return filterEndDate; }
            set
            {
                if (SetProperty(ref filterEndDate, value) && value >= FilterBeginDate)
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
                        recordsChangeTracker.IsEnabled = false;
                        LoadContractData();
                        changeTracker.IsEnabled = true;
                        recordsChangeTracker.IsEnabled = true;
                    }
                    else
                        IsContractSelected = false;
                }
            }
        }

        private string contractName;
        public string ContractName
        {
            get { return contractName; }
            set { SetProperty(ref contractName, value); }
        }

        private string contractFinSource;
        public string ContractFinSource
        {
            get { return contractFinSource; }
            set { SetProperty(ref contractFinSource, value); }
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
                if (SetProperty(ref selectedFinSourceId, value) && value != SpecialValues.NonExistingId)
                {
                    IsDMSContract = contractService.GetFinancingSourceById(value).First().Options.Contains(OptionValues.DMS);
                }
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

        private double contractCost;
        public double ContractCost
        {
            get { return contractCost; }
            set 
            {
                changeTracker.Track(contractCost, value);
                SetProperty(ref contractCost, value); 
            }
        }

        private string orgDetails;
        public string OrgDetails
        {
            get { return orgDetails; }
            set 
            {
                changeTracker.Track(orgDetails, value);
                SetProperty(ref orgDetails, value); 
            }
        }

        private bool isContractSelected;
        public bool IsContractSelected
        {
            get { return isContractSelected; }
            set { SetProperty(ref isContractSelected, value); }
        }

        private bool isDMSContract;
        public bool IsDMSContract
        {
            get { return isDMSContract; }
            set { SetProperty(ref isDMSContract, value); }
        }

        private FieldValue selectedPatient;
        public FieldValue SelectedPatient
        {
            get { return selectedPatient; }
            set
            {
                if (value != null)
                {
                    changeTracker.Track(selectedPatient, value); 
                    SetProperty(ref selectedPatient, value);
                }
            }
        }

        private PersonSuggestionsProvider personSuggestionsProvider;
        public PersonSuggestionsProvider PersonSuggestionsProvider
        {
            get { return personSuggestionsProvider; }
            set { SetProperty(ref personSuggestionsProvider, value); }
        }

        public ObservableCollectionEx<RecordTypeViewModel> Records { get; private set; }

        private RecordTypeViewModel selectedRecord;
        public RecordTypeViewModel SelectedRecord
        {
            get { return selectedRecord; }
            set
            {
                SetProperty(ref selectedRecord, value);
            }
        }

        #endregion

        private async void LoadDataSources()
        {
            FailureMediator.Deactivate();
            var contractRecord = await contractService.GetRecordTypesByOptions(OptionValues.Contract).FirstOrDefaultAsync();
            var reliableStaff = await contractService.GetRecordTypeRolesByOptions(OptionValues.ResponsibleForContract).FirstOrDefaultAsync();
            if (contractRecord == null || reliableStaff == null)
            {
                FailureMediator.Activate("В МИС не найдена информация об услуге 'Договор' и/или об ответственных за выполнение. Отсутствует запись в таблицах RecordTypes, RecordTypeRoles", reloadDataSourcesCommandWrapper);
                return;
            }
            var personStaffs = await contractService.GetAllowedPersonStaffs(contractRecord.Id, reliableStaff.Id, DateTime.Now).ToArrayAsync();
            if (!personStaffs.Any())
            {
                FailureMediator.Activate("В МИС не найдена информация о правах на выполнение услуги. Отсутствует запись в таблице RecordTypeRolePermissions", reloadDataSourcesCommandWrapper);
                return;
            }
            if (!contractService.GetPaymentTypes().Any())
            {
                FailureMediator.Activate("В МИС не найдена информация о методах оплаты. Отсутствуют записи в таблице PaymentTypes", reloadDataSourcesCommandWrapper);
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
            orgs.AddRange(orgsSource.Where(x => x.UseInContract).OrderBy(x => x.Name).Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
            Organizations = new ObservableCollectionEx<FieldValue>(orgs);
            SelectedOrganizationId = SpecialValues.NonExistingId;

            FilterBeginDate = DateTime.Now;
            FilterEndDate = DateTime.Now;
            SelectedFilterFinSourceId = SpecialValues.NonExistingId;
        }

        private async void LoadContractsAsync()
        {
            if (selectedFilterFinSourceId == 0) return;
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
            IOrderedQueryable<RecordContract> contractsQuery = null;
            try
            {
                contractsQuery = contractService.GetContractsWithOrgs(filterBeginDate, filterEndDate, selectedFilterFinSourceId).OrderBy(x => x.BeginDateTime);
                var result = await Task.Factory.StartNew(() =>
                {
                    return contractsQuery.Select(x => new
                    {
                        Id = x.Id,
                        ContractName = (x.Number.HasValue ? "№" + x.Number.ToString() : string.Empty) + " - " + x.ContractName,
                        Patient = x.Person1,
                        OrgId = x.OrgId,
                        OrgDetails = x.OrgDetails,
                        ContractBeginDate = x.BeginDateTime,
                        ContractEndDate = x.EndDateTime,
                        FinancingSourceName = x.FinancingSource.ShortName,
                        FinancingSourceId = x.FinancingSourceId,
                        PaymentTypeId = x.PaymentTypeId,
                        RegistratorId = x.InUserId,
                        IsCashless = x.PaymentType.Options.Contains(OptionValues.Cashless),
                        ContractCost = x.ContractCost,
                        Records = x.RecordContractLimits
                    }).ToArray();
                }, token);          

                Contracts = new ObservableCollectionEx<ContractViewModel>(
                    result.Select(x => new ContractViewModel()
                    {
                        Id = x.Id,
                        ContractName = x.ContractName,
                        ContractFinSource = x.FinancingSourceName,
                        Patient = x.Patient != null ? new FieldValue { Field = x.Patient.FullName + ", " + x.Patient.BirthYear, Value = x.Patient.Id } : null,
                        OrgId = x.OrgId.Value,
                        OrgDetails = x.OrgDetails,
                        ContractCost = x.ContractCost,
                        ContractBeginDate = x.ContractBeginDate,
                        ContractEndDate = x.ContractEndDate,
                        FinancingSourceId = x.FinancingSourceId,
                        PaymentTypeId = x.PaymentTypeId,
                        RegistratorId = x.RegistratorId,
                        IsCashless = x.IsCashless,
                        Records = x.Records.Select(a => new RecordTypeViewModel() { Id = a.RecordTypeId, Name = a.RecordType.Name }).ToArray()
                    }));
                if (contracts.Any())
                    SelectedContract = contracts.OrderByDescending(x => x.ContractBeginDate).First();
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
            ContractName = SelectedContract.ContractName;
            ContractFinSource = SelectedContract.ContractFinSource;
            SelectedFinSourceId = SelectedContract.FinancingSourceId;
            SelectedOrganizationId = SelectedContract.OrgId;
            SelectedPatient = SelectedContract.Patient;
            ContractBeginDate = SelectedContract.ContractBeginDate;
            ContractEndDate = SelectedContract.ContractEndDate;
            ContractCost = SelectedContract.ContractCost;
            OrgDetails = SelectedContract.OrgDetails;
            SelectedRegistratorId = SelectedContract.RegistratorId;
            Records.Clear();
            if (SelectedContract.Records != null)
                Records.AddRange(new ObservableCollectionEx<RecordTypeViewModel>(SelectedContract.Records));            
        }

        private bool CanSaveChanges()
        {
            if (selectedContract == null)
            {
                return false;
            }
            return changeTracker.HasChanges || (Records.Any() && recordsChangeTracker.HasChanges);
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
                ContractName = "НОВЫЙ ДОГОВОР",
                ContractFinSource = string.Empty,
                OrgDetails = string.Empty,
                ContractCost = 0,
                ContractBeginDate = DateTime.Now,
                ContractEndDate = DateTime.Now,
                FinancingSourceId = -1,
                PaymentTypeId = -1,
                RegistratorId = -1,
                OrgId = -1
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
            if (IsDMSContract)
            {
                contract.ConsumerId = selectedPatient.Value;
                contract.ContractName = contractService.GetPersonById(selectedPatient.Value).First().ShortName + " (" + contractService.GetOrganizationById(selectedOrganizationId).First().Name + ")";
            }
            else
            {
                contract.ConsumerId = (int?)null;
                contract.ContractName = contractService.GetOrganizationById(selectedOrganizationId).First().Name;
            }
            contract.PaymentTypeId = contractService.GetPaymentTypes().First(x => x.Options.Contains(OptionValues.Cashless)).Id;
            contract.TransactionNumber = string.Empty;
            contract.TransactionDate = string.Empty;            
            contract.Priority = 1;
            contract.InUserId = selectedRegistratorId;
            contract.InDateTime = DateTime.Now;
            contract.OrgId = selectedOrganizationId;
            contract.OrgDetails = orgDetails;
            contract.ContractCost = contractCost;
            contract.Options = string.Empty;            

            try
            {
                contract.Id = contractService.SaveContractData(contract, Records.Select(x => x.Id).ToArray());
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
                    contract = contractService.GetContractById(contract.Id).First(); 
                    SelectedContract.Id = contract.Id;
                    SelectedContract.ContractFinSource = contract.FinancingSource.ShortName;
                    SelectedContract.FinancingSourceId = contract.FinancingSourceId;
                    SelectedContract.ContractName = contract.DisplayName;
                    SelectedContract.ContractBeginDate = contract.BeginDateTime;
                    SelectedContract.ContractEndDate = contract.EndDateTime;
                    SelectedContract.OrgId = contract.OrgId.Value;
                    SelectedContract.OrgDetails = contract.OrgDetails;
                    SelectedContract.RegistratorId = contract.InUserId;
                    SelectedContract.ContractCost = contract.ContractCost;
                    SelectedContract.Records = contract.RecordContractLimits.Select(x => new RecordTypeViewModel() { Id = x.RecordTypeId, Name = x.RecordType.Name }).ToArray();
                    if (IsDMSContract)
                    {
                        var patient = contractService.GetPersonById(selectedPatient.Value).First();
                        SelectedContract.Patient = new FieldValue { Field = patient.FullName + ", " + patient.BirthYear, Value = patient.Id };
                    }                       
                    ContractName = contract.DisplayName;
                    changeTracker.UntrackAll();
                    recordsChangeTracker.AcceptChanges();
                    saveContractCommand.RaiseCanExecuteChanged();
                    removeContractCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void RemoveContract()
        {
            if (selectedContract == null)
            {
                messageService.ShowWarning("Не выбран договор.");
                return;
            }
            if (messageService.AskUser("Вы уверены, что хотите удалить договор?") == true)
            {
                var visit = contractService.GetVisitsByContractId(selectedContract.Id).FirstOrDefault();
                if (visit != null)
                {
                    messageService.ShowInformation("Данный договор уже закреплен за случаем обращения пациента \"" + visit.VisitTemplate.ShortName + "\". Удаление договора невозможно.");
                    return;
                }
                if (selectedContract.Id != SpecialValues.NewId)
                    contractService.DeleteContract(selectedContract.Id);
                Contracts.Remove(selectedContract);
                saveContractCommand.RaiseCanExecuteChanged();
                removeContractCommand.RaiseCanExecuteChanged();
            }
        }     

        private async void AddOrganization()
        {
            var addContractOrganizationViewModel = addContractOrganizationViewModelFactory();
            var result = await dialogService.ShowDialogAsync(addContractOrganizationViewModel);
            if (addContractOrganizationViewModel.SaveSuccesfull)
            {
                Organizations.Add(new FieldValue() { Value = addContractOrganizationViewModel.orgId, Field = contractService.GetOrganizationById(addContractOrganizationViewModel.orgId).First().Name });
                if (SelectedContract.Id == SpecialValues.NewId)
                    SelectedOrganizationId = addContractOrganizationViewModel.orgId;
            }
        }

        private void RemoveRecord()
        {
            if (SelectedRecord != null)
            {
                if (messageService.AskUser("Удалить услугу '" + SelectedRecord.Name + "' из договора ?") == true)
                {
                    if (!SpecialValues.IsNewOrNonExisting(SelectedContract.Id))
                        contractService.RemoveRecord(SelectedContract.Id, SelectedRecord.Id);
                    Records.Remove(SelectedRecord);
                }
            }
        }

        private async void AddRecord()
        {
            var addRecordViewModel = addRecordViewModelFactory();
            addRecordViewModel.Initialize();
            var result = await dialogService.ShowDialogAsync(addRecordViewModel);
            if (result == true)
            {
                foreach (var item in addRecordViewModel.RecordTypes.Where(x => x.IsChecked))
                    Records.Add(new RecordTypeViewModel() { Id = item.Id, Name = item.Name });
                saveContractCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly DelegateCommand addContractCommand;
        private readonly DelegateCommand saveContractCommand;
        private readonly DelegateCommand removeContractCommand;
        private readonly DelegateCommand addOrganizationCommand;
                
        public ICommand AddContractCommand { get { return addContractCommand; } }
        public ICommand SaveContractCommand { get { return saveContractCommand; } }
        public ICommand RemoveContractCommand { get { return removeContractCommand; } }
        public ICommand AddOrganizationCommand { get { return addOrganizationCommand; } }

        private readonly DelegateCommand addRecordCommand;
        private readonly DelegateCommand removeRecordCommand;
        public ICommand AddRecordCommand { get { return addRecordCommand; } }
        public ICommand RemoveRecordCommand { get { return removeRecordCommand; } }
                
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

        public void Dispose()
        {
            reloadContractsDataCommandWrapper.Dispose();
            reloadDataSourcesCommandWrapper.Dispose();
            saveChangesCommandWrapper.Dispose();
        }
    }
}
