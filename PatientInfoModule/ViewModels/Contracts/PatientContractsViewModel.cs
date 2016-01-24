using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Core.Data;
using Core.Data.Misc;
using Core.Extensions;
using Core.Misc;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Shell.Shared;

namespace PatientInfoModule.ViewModels
{
    public class PatientContractsViewModel : TrackableBindableBase, IConfirmNavigationRequest, IDataErrorInfo, IChangeTrackerMediator, IDisposable
    {
        private readonly IPatientService personService;

        private readonly IContractService contractService;

        private readonly IRecordService recordService;

        private readonly IEventAggregator eventAggregator;

        private readonly ILog log;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private readonly IDialogServiceAsync dialogService;

        private readonly IDialogService messageService;

        private readonly CommandWrapper reloadContractsDataCommandWrapper;

        private readonly CommandWrapper reloadDataSourcesCommandWrapper;

        public BusyMediator BusyMediator { get; set; }

        public FailureMediator FailureMediator { get; private set; }

        private CancellationTokenSource currentLoadingToken;

        private readonly Func<AddContractRecordsViewModel> addContractRecordsViewModelFactory;

        private int patientId;

        public PatientContractsViewModel(IPatientService personService,
                                         IContractService contractService,
                                         IRecordService recordService,
                                         IEventAggregator eventAggregator,
                                         ILog log,
                                         IRegionManager regionManager,
                                         IViewNameResolver viewNameResolver,
                                         IDialogService messageService,
                                         IDialogServiceAsync dialogService,
                                         Func<AddContractRecordsViewModel> addContractRecordsViewModelFactory)
        {
            if (personService == null)
            {
                throw new ArgumentNullException("personService");
            }
            if (contractService == null)
            {
                throw new ArgumentNullException("contractService");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            if (recordService == null)
            {
                throw new ArgumentNullException("recordService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
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
            this.personService = personService;
            this.contractService = contractService;
            this.recordService = recordService;
            this.eventAggregator = eventAggregator;
            this.log = log;
            this.regionManager = regionManager;
            this.viewNameResolver = viewNameResolver;
            this.dialogService = dialogService;
            this.messageService = messageService;
            this.addContractRecordsViewModelFactory = addContractRecordsViewModelFactory;
            patientId = SpecialValues.NonExistingId;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();

            ChangeTracker = new ChangeTrackerEx<PatientContractsViewModel>(this);
            ChangeTracker.PropertyChanged += OnChangesTracked;

            reloadContractsDataCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => LoadContractsAsync(patientId)), CommandName = "Повторить" };
            reloadDataSourcesCommandWrapper = new CommandWrapper { Command = new DelegateCommand(LoadDataSources), CommandName = "Повторить" };

            PersonSuggestionsProvider = new PersonSuggestionsProvider(personService);

            ContractItems = new ObservableCollectionEx<ContractItemViewModel>();

            contractItemsTracker = new CompositeChangeTracker(ChangeTracker);

            addContractCommand = new DelegateCommand(AddContract, CanAddContract);
            saveContractCommand = new DelegateCommand(() => SaveContract(), CanSaveChanges);
            removeContractCommand = new DelegateCommand(RemoveContract, CanRemoveContract);
            printContractCommand = new DelegateCommand(PrintContract);
            printAppendixCommand = new DelegateCommand(PrintAppendix);
            addRecordCommand = new DelegateCommand(AddRecord, CanRemoveContract);
            removeRecordCommand = new DelegateCommand(RemoveRecord, CanRemoveContract);
            addAppendixCommand = new DelegateCommand(AddAppendix, CanRemoveContract);
            removeAppendixCommand = new DelegateCommand(RemoveAppendix, CanRemoveContract);

            saveChangesCommandWrapper = new CommandWrapper { Command = saveContractCommand };
            IsActive = false;
            LoadDataSources();
            eventAggregator.GetEvent<BeforeSelectionChangedEvent<Person>>().Subscribe(OnBeforePatientSelected);
        }

        private void OnBeforePatientSelected(BeforeSelectionChangedEventData data)
        {
            if (patientId == SpecialValues.NewId || ChangeTracker.HasChanges)
            {
                data.AddActionToPerform(async () => await Task<bool>.Factory.StartNew(SaveContract), () => regionManager.RequestNavigate(RegionNames.ModuleList, viewNameResolver.Resolve<ContractsHeaderViewModel>()));
            }
        }

        private void OnBeforeContractItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<ContractItemViewModel>())
                {
                    if (!newItem.ChangeTracker.IsEnabled)
                    {
                        newItem.ChangeTracker.IsEnabled = true;
                    }
                    newItem.PropertyChanged += contractItem_PropertyChanged;
                    contractItemsTracker.AddTracker(newItem.ChangeTracker);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<ContractItemViewModel>())
                {
                    oldItem.PropertyChanged -= contractItem_PropertyChanged;
                    contractItemsTracker.RemoveTracker(oldItem.ChangeTracker);
                }
            }
        }

        private void contractItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateTotalSumRow();
            UpdateChangeCommandsState();
        }

        public IChangeTracker ChangeTracker { get; private set; }

        private readonly CompositeChangeTracker contractItemsTracker;

        public void Dispose()
        {
            contractItemsTracker.Dispose();
            reloadContractsDataCommandWrapper.Dispose();
            reloadDataSourcesCommandWrapper.Dispose();
            saveChangesCommandWrapper.Dispose();
        }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                UpdateChangeCommandsState();
            }
        }

        private void UpdateChangeCommandsState()
        {
            saveContractCommand.RaiseCanExecuteChanged();
            removeContractCommand.RaiseCanExecuteChanged();
            addContractCommand.RaiseCanExecuteChanged();
            addRecordCommand.RaiseCanExecuteChanged();
            removeRecordCommand.RaiseCanExecuteChanged();
            addAppendixCommand.RaiseCanExecuteChanged();
            removeAppendixCommand.RaiseCanExecuteChanged();
        }

        public async void LoadDataSources()
        {
            FailureMediator.Deactivate();
            var contractRecord = await recordService.GetRecordTypesByOptions(OptionValues.Contract).FirstOrDefaultAsync();
            var reliableStaff = await recordService.GetRecordTypeRolesByOptions(OptionValues.ResponsibleForContract).FirstOrDefaultAsync();
            if (contractRecord == null || reliableStaff == null)
            {
                FailureMediator.Activate("В МИС не найдена информация об услуге 'Договор' и/или об ответственных за выполнение. Отсутствует запись в таблицах RecordTypes, RecordTypeRoles", reloadDataSourcesCommandWrapper);
                return;
            }
            var personStaffs = await personService.GetAllowedPersonStaffs(contractRecord.Id, reliableStaff.Id, DateTime.Now).ToArrayAsync();
            if (!personStaffs.Any())
            {
                FailureMediator.Activate("В МИС не найдена информация о правах на выполнение услуги. Отсутствует запись в таблице RecordTypeRolePermissions", reloadDataSourcesCommandWrapper);
                return;
            }
            if (!recordService.GetPaymentTypes().Any())
            {
                FailureMediator.Activate("В МИС не найдена информация о методах оплаты. Отсутствуют записи в таблице PaymentTypes", reloadDataSourcesCommandWrapper);
                return;
            }
            var elements = new List<FieldValue>();
            elements.Add(new FieldValue { Value = -1, Field = "- все -" });
            elements.AddRange(personStaffs.Select(x => new FieldValue { Value = x.Id, Field = x.Person.ShortName }));
            Registrators = new ObservableCollectionEx<FieldValue>(elements);

            var finSources = new List<FieldValue>();
            var fSources = await recordService.GetActiveFinancingSources().ToArrayAsync();
            finSources.Add(new FieldValue { Value = -1, Field = "- выберите ист. финансирования -" });
            finSources.AddRange(fSources.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
            FinancingSources = new ObservableCollectionEx<FieldValue>(finSources);

            var paymentTypesSource = new List<FieldValue>();
            var paymentSources = await recordService.GetPaymentTypes().ToArrayAsync();
            paymentTypesSource.Add(new FieldValue { Value = -1, Field = "- выберите метод оплаты -" });
            paymentTypesSource.AddRange(paymentSources.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
            PaymentTypes = new ObservableCollectionEx<FieldValue>(paymentTypesSource);

            IsCashless = false;
            SelectedRegistratorId = SpecialValues.NonExistingId;
            SelectedPaymentTypeId = SpecialValues.NonExistingId;
            SelectedFinancingSourceId = SpecialValues.NonExistingId;
        }

        private async void LoadContractsAsync(int patientId)
        {
            this.patientId = patientId;
            if (patientId == SpecialValues.NewId || patientId == SpecialValues.NonExistingId)
            {
                return;
            }

            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            FailureMediator.Deactivate();
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate("Загрузка договоров пациента...");
            log.InfoFormat("Loading contracts for patient with Id {0}...", patientId);
            IDisposableQueryable<RecordContract> contractsQuery = null;
            try
            {
                contractsQuery = contractService.GetContracts(patientId);
                var result = await Task.Factory.StartNew(() =>
                                                         {
                                                             return contractsQuery.Select(x => new
                                                                                               {
                                                                                                   x.Id,
                                                                                                   ContractNumber = x.Number,
                                                                                                   ContractName = (x.Number.HasValue ? "№" + x.Number.ToString() + " - " : string.Empty) + x.ContractName,
                                                                                                   Client = x.Person,
                                                                                                   Consumer = x.Person1,
                                                                                                   ContractBeginDate = x.BeginDateTime,
                                                                                                   ContractEndDate = x.EndDateTime, x.FinancingSourceId, x.PaymentTypeId,
                                                                                                   ContractCost = x.RecordContractItems.Any(a => a.IsPaid) ? x.RecordContractItems.Where(a => a.IsPaid).Sum(a => a.Cost) : 0.0,
                                                                                                   RegistratorId = x.InUserId,
                                                                                                   IsCashless = x.PaymentType.Options.Contains(OptionValues.Cashless)
                                                                                               })
                                                                                  .OrderByDescending(x => x.ContractBeginDate)
                                                                                  .ToArray();
                                                         }, token);

                Contracts = new ObservableCollectionEx<ContractViewModel>(
                    result.Select(x => new ContractViewModel
                                       {
                                           Id = x.Id,
                                           ContractNumber = x.ContractNumber.ToSafeString(),
                                           ContractName = x.ContractName,
                                           Client = new FieldValue { Field = x.Client.FullName + ", " + x.Client.BirthYear, Value = x.Client.Id },
                                           Consumer = x.Consumer.ToString(),
                                           ContractCost = x.ContractCost,
                                           ContractBeginDate = x.ContractBeginDate,
                                           ContractEndDate = x.ContractEndDate,
                                           FinancingSourceId = x.FinancingSourceId,
                                           PaymentTypeId = x.PaymentTypeId,
                                           RegistratorId = x.RegistratorId,
                                           IsCashless = x.IsCashless
                                       }));
                ContractsCount = Contracts.Count();
                ContractsSum = Contracts.Sum(x => x.ContractCost) + " руб.";

                if (contracts.Any())
                {
                    SelectedContract = contracts.OrderByDescending(x => x.ContractBeginDate).First();
                }
                else
                {
                    IsActive = false;
                }

                loadingIsCompleted = true;
            }
            catch (OperationCanceledException)
            {
                //Do nothing. Cancelled operation means that user selected different patient before previous one was loaded
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to load contracts for patient with Id {0}", patientId);
                FailureMediator.Activate("Не удалость загрузить договоры пациента. Попробуйте еще раз или обратитесь в службу поддержки", reloadContractsDataCommandWrapper, ex);
                loadingIsCompleted = false;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                    UpdateChangeCommandsState();
                }
            }
        }

        private void LoadContractData()
        {
            contractItemsTracker.IsEnabled = false;
            Consumer = SelectedContract.Consumer;
            ContractBeginDateTime = SelectedContract.ContractBeginDate;
            ContractEndDateTime = SelectedContract.ContractEndDate;
            ContractName = SelectedContract.ContractName;

            SelectedFinancingSourceId = SelectedContract.FinancingSourceId;
            SelectedRegistratorId = SelectedContract.RegistratorId;
            SelectedPaymentTypeId = SelectedContract.PaymentTypeId;
            SelectedClient = SelectedContract.Client;
            IsCashless = SelectedContract.IsCashless;

            LoadContractItems();
            ContractItems.BeforeCollectionChanged += OnBeforeContractItemsChanged;
            contractItemsTracker.IsEnabled = true;
        }

        private void LoadContractItems()
        {
            ContractItems.Clear();
            var items = contractService.GetContractItems(selectedContract.Id);
            foreach (var groupedItem in items.GroupBy(x => x.Appendix).OrderBy(x => x.Key))
            {
                if (groupedItem.Key.HasValue)
                {
                    AddSectionRow(groupedItem.Key.Value, Color.LightSalmon, HorizontalAlignment.Center);
                }
                foreach (var item in groupedItem)
                {
                    AddContractItemRow(item);
                }
            }
            if (items.Any())
            {
                AddSectionRow(-1, Color.LightGreen, HorizontalAlignment.Right);
            }
        }

        private void AddContractItemRow(RecordContractItem item)
        {
            var contractItem = new ContractItemViewModel(recordService, personService, patientId, selectedFinancingSourceId, contractBeginDateTime)
                               {
                                   Id = item.Id,
                                   RecordContractId = item.RecordContractId,
                                   AssignmentId = item.AssignmentId,
                                   RecordTypeId = item.RecordTypeId,
                                   IsPaid = item.IsPaid,
                                   RecordTypeName = item.RecordType.Name,
                                   RecordCount = item.Count,
                                   RecordCost = item.Cost,
                                   Appendix = item.Appendix
                               };
            ContractItems.Add(contractItem);
        }

        private void AddSectionRow(int appendix, Color backColor, HorizontalAlignment alignment, int insertPosition = -1)
        {
            var item = new ContractItemViewModel(recordService, personService, patientId, selectedFinancingSourceId, contractBeginDateTime)
                       {
                           IsSection = true,
                           SectionName = appendix != -1 ? "Доп. соглашение № " + appendix.ToSafeString() :
                                             ("ИТОГО: " + (contractItems.Any(x => x.IsPaid) ? contractItems.Where(x => x.IsPaid).Sum(x => x.RecordCost) : 0) + " руб."),
                           Appendix = appendix,
                           SectionAlignment = alignment,
                           SectionBackColor = backColor
                       };
            if (insertPosition == -1)
            {
                ContractItems.Add(item);
            }
            else
            {
                ContractItems.Insert(insertPosition, item);
            }
        }

        private void UpdateTotalSumRow()
        {
            if (contractItems.Any(x => x.IsSection && x.Appendix == -1))
            {
                contractItems.First(x => x.IsSection && x.Appendix == -1).SectionName =
                    "ИТОГО: " + (contractItems.Any(x => x.IsPaid) ? contractItems.Where(x => x.IsPaid).Sum(x => x.RecordCost) : 0) + " руб.";
            }
            else
            {
                AddSectionRow(-1, Color.LightGreen, HorizontalAlignment.Right);
            }
            if (selectedContract != null && selectedContract.Id != SpecialValues.NewId)
            {
                SelectedContract.ContractCost = contractService.GetContractCost(selectedContract.Id);
            }
            ContractsCount = Contracts.Count();
            ContractsSum = Contracts.Sum(x => x.ContractCost) + " руб.";
        }

        #region Properties

        private PersonSuggestionsProvider personSuggestionsProvider;

        public PersonSuggestionsProvider PersonSuggestionsProvider
        {
            get { return personSuggestionsProvider; }
            set { SetProperty(ref personSuggestionsProvider, value); }
        }

        private string contractName;

        public string ContractName
        {
            get { return contractName; }
            set { SetProperty(ref contractName, value); }
        }

        private int contractsCount;

        public int ContractsCount
        {
            get { return contractsCount; }
            set { SetProperty(ref contractsCount, value); }
        }

        private string contractsSum;

        public string ContractsSum
        {
            get { return contractsSum; }
            set { SetProperty(ref contractsSum, value); }
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
                        IsActive = true;
                        LoadContractData();
                    }
                    else
                    {
                        IsActive = false;
                    }
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
            set { SetTrackedProperty(ref selectedRegistratorId, value); }
        }

        private ObservableCollectionEx<FieldValue> financingSources;

        public ObservableCollectionEx<FieldValue> FinancingSources
        {
            get { return financingSources; }
            set { SetProperty(ref financingSources, value); }
        }

        private int selectedFinancingSourceId;

        public int SelectedFinancingSourceId
        {
            get { return selectedFinancingSourceId; }
            set { SetTrackedProperty(ref selectedFinancingSourceId, value); }
        }

        private ObservableCollectionEx<FieldValue> paymentTypes;

        public ObservableCollectionEx<FieldValue> PaymentTypes
        {
            get { return paymentTypes; }
            set { SetProperty(ref paymentTypes, value); }
        }

        private int selectedPaymentTypeId;

        public int SelectedPaymentTypeId
        {
            get { return selectedPaymentTypeId; }
            set
            {
                if (SetTrackedProperty(ref selectedPaymentTypeId, value))
                {
                    IsCashless = (value != SpecialValues.NonExistingId) && recordService.GetPaymentTypeById(value).First().Options.Contains(OptionValues.Cashless);
                }
            }
        }

        private DateTime contractBeginDateTime;

        public DateTime ContractBeginDateTime
        {
            get { return contractBeginDateTime; }
            set { SetTrackedProperty(ref contractBeginDateTime, value); }
        }

        private DateTime contractEndDateTime;

        public DateTime ContractEndDateTime
        {
            get { return contractEndDateTime; }
            set { SetTrackedProperty(ref contractEndDateTime, value); }
        }

        private bool isCashless;

        public bool IsCashless
        {
            get { return isCashless; }
            set { SetTrackedProperty(ref isCashless, value); }
        }

        private FieldValue selectedClient;

        public FieldValue SelectedClient
        {
            get { return selectedClient; }
            set
            {
                if (value != null)
                {
                    SetTrackedProperty(ref selectedClient, value);
                }
            }
        }

        private string consumer;

        public string Consumer
        {
            get { return consumer; }
            set { SetProperty(ref consumer, value); }
        }

        private ObservableCollectionEx<ContractItemViewModel> contractItems;

        public ObservableCollectionEx<ContractItemViewModel> ContractItems
        {
            get { return contractItems; }
            set { SetTrackedProperty(ref contractItems, value); }
        }

        private ContractItemViewModel selectedContractItem;

        public ContractItemViewModel SelectedContractItem
        {
            get { return selectedContractItem; }
            set { SetProperty(ref selectedContractItem, value); }
        }

        private bool isActive;

        public bool IsActive
        {
            get { return isActive; }
            set { SetProperty(ref isActive, value); }
        }

        private string transationNumber;

        public string TransationNumber
        {
            get { return transationNumber; }
            set { SetTrackedProperty(ref transationNumber, value); }
        }

        private string transationDate;

        public string TransationDate
        {
            get { return transationDate; }
            set { SetTrackedProperty(ref transationDate, value); }
        }

        #endregion

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialValues.NonExistingId;
            if (targetPatientId != patientId)
            {
                patientId = targetPatientId;
                LoadContractsAsync(patientId);
            }
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

        private readonly DelegateCommand addContractCommand;

        private readonly DelegateCommand saveContractCommand;

        private readonly DelegateCommand removeContractCommand;

        private readonly DelegateCommand printContractCommand;

        private readonly DelegateCommand printAppendixCommand;

        private readonly DelegateCommand addRecordCommand;

        private readonly DelegateCommand removeRecordCommand;

        private readonly DelegateCommand addAppendixCommand;

        private readonly DelegateCommand removeAppendixCommand;

        public ICommand AddContractCommand
        {
            get { return addContractCommand; }
        }

        public ICommand SaveContractCommand
        {
            get { return saveContractCommand; }
        }

        public ICommand RemoveContractCommand
        {
            get { return removeContractCommand; }
        }

        public ICommand PrintContractCommand
        {
            get { return printContractCommand; }
        }

        public ICommand PrintAppendixCommand
        {
            get { return printAppendixCommand; }
        }

        public ICommand AddRecordCommand
        {
            get { return addRecordCommand; }
        }

        public ICommand RemoveRecordCommand
        {
            get { return removeRecordCommand; }
        }

        public ICommand AddAppendixCommand
        {
            get { return addAppendixCommand; }
        }

        public ICommand RemoveAppendixCommand
        {
            get { return removeAppendixCommand; }
        }


        private void RemoveAppendix()
        {
            if (selectedContractItem == null || !selectedContractItem.IsSection)
            {
                messageService.ShowWarning("Не выбрано соглашение для удаления.");
                return;
            }

            if (messageService.AskUser("Удалить " + selectedContractItem.SectionName + " вместе со вложенными услугами ?") == true)
            {
                var appendix = selectedContractItem.Appendix;
                foreach (var item in ContractItems.ToList())
                {
                    if (item.Appendix == appendix)
                    {
                        if (!item.IsSection && item.Id != SpecialValues.NewId)
                        {
                            contractService.DeleteContractItemById(item.Id);
                        }
                        ContractItems.Remove(item);
                    }
                }
                UpdateTotalSumRow();
            }
        }

        private void AddAppendix()
        {
            if (selectedContract == null)
            {
                messageService.ShowWarning("Не выбран договор. Добавление доп. соглашения невозможно.");
                return;
            }
            int? appendixCount = contractItems.Where(x => x.IsSection && x.Appendix > 0).Select(x => x.Appendix.Value).OrderByDescending(x => x).FirstOrDefault();
            if (!contractItems.Any(x => x.IsSection && x.Appendix == (appendixCount.ToInt() + 1)))
            {
                AddSectionRow(appendixCount.ToInt() + 1, Color.LightSalmon, HorizontalAlignment.Center, contractItems.Count - 1);
            }
        }

        private void RemoveRecord()
        {
            if (selectedContractItem == null || selectedContractItem.IsSection)
            {
                messageService.ShowWarning("Не выбрана услуга для удаления.");
                return;
            }
            if (messageService.AskUser("Удалить услугу " + SelectedContractItem.RecordTypeName + " из договора ?") == true)
            {
                if (selectedContractItem.Id != SpecialValues.NewId)
                {
                    contractService.DeleteContractItemById(selectedContractItem.Id);
                }
                ContractItems.Remove(selectedContractItem);
                UpdateTotalSumRow();
            }
        }

        private async void AddRecord()
        {
            if (selectedContract == null)
            {
                messageService.ShowWarning("Не выбран договор. Добавление услуг невозможно.");
                return;
            }

            var addContractRecordsViewModel = addContractRecordsViewModelFactory();
            addContractRecordsViewModel.Intialize(patientId, selectedFinancingSourceId, contractBeginDateTime, false, false);
            var result = await dialogService.ShowDialogAsync(addContractRecordsViewModel);
            if (addContractRecordsViewModel.RecordsWasSelected)
            {
                int? appendixCount = contractItems.Where(x => x.Appendix > 0).Select(x => x.Appendix.Value).OrderByDescending(x => x).FirstOrDefault();
                if (addContractRecordsViewModel.IsAssignRecordsChecked)
                {
                    foreach (var assignment in addContractRecordsViewModel.Assignments.Where(x => x.IsSelected))
                    {
                        var insertPosition = contractItems.Any() ? contractItems.Count - 1 : 0;
                        var contractItem = new ContractItemViewModel(recordService, personService, patientId, selectedFinancingSourceId, contractBeginDateTime);
                        contractItem.ChangeTracker.IsEnabled = true;
                        contractItem.Id = 0;
                        contractItem.RecordContractId = null;
                        contractItem.AssignmentId = assignment.Id;
                        contractItem.RecordTypeId = assignment.RecordTypeId;
                        contractItem.IsPaid = true;
                        contractItem.RecordTypeName = assignment.RecordTypeName;
                        contractItem.RecordCount = 1;
                        contractItem.RecordCost = assignment.RecordTypeCost;
                        contractItem.Appendix = (appendixCount == 0 ? null : appendixCount);
                        ContractItems.Insert(insertPosition, contractItem);
                    }
                }
                else
                {
                    if (addContractRecordsViewModel.SelectedRecord == null)
                    {
                        return;
                    }
                    var insertPosition = contractItems.Any() ? contractItems.Count - 1 : 0;
                    var contractItem = new ContractItemViewModel(recordService, personService, patientId, selectedFinancingSourceId, contractBeginDateTime);
                    contractItem.ChangeTracker.IsEnabled = true;
                    contractItem.Id = 0;
                    contractItem.RecordContractId = null;
                    contractItem.AssignmentId = null;
                    contractItem.RecordTypeId = addContractRecordsViewModel.SelectedRecord.Id;
                    contractItem.IsPaid = true;
                    contractItem.RecordTypeName = addContractRecordsViewModel.SelectedRecord.Name;
                    contractItem.RecordCount = addContractRecordsViewModel.RecordsCount;
                    contractItem.RecordCost = addContractRecordsViewModel.AssignRecordTypeCost;
                    contractItem.Appendix = (appendixCount == 0 ? null : appendixCount);
                    ContractItems.Insert(insertPosition, contractItem);
                }

                UpdateTotalSumRow();
                UpdateChangeCommandsState();
            }
        }

        private void PrintAppendix()
        {
            if (selectedContract == null)
            {
                messageService.ShowWarning("Не выбран договор. Печать невозможна.");
                return;
            }
            if (!contractService.GetContractItems(selectedContract.Id).Any(x => x.Appendix.HasValue))
            {
                messageService.ShowWarning("В договоре отсутствует доп. соглашение. Печать невозможна.");
            }
        }

        private void PrintContract()
        {
            if (selectedContract == null)
            {
                messageService.ShowWarning("Не выбран договор. Печать невозможна.");
            }
        }

        private void RemoveContract()
        {
            if (selectedContract == null)
            {
                messageService.ShowWarning("Не выбран договор.");
                return;
            }
            if (messageService.AskUser("Вы уверены, что хотите удалить договор " + ContractName + "?") == true)
            {
                var visit = recordService.GetVisitsByContractId(selectedContract.Id).FirstOrDefault();
                if (visit != null)
                {
                    messageService.ShowWarning("Данный договор уже закреплен за случаем обращения пациента \"" + visit.VisitTemplate.ShortName + "\". Удаление договора невозможно.");
                    return;
                }
                if (selectedContract.Id != SpecialValues.NewId)
                {
                    contractService.DeleteContract(selectedContract.Id);
                }
                Contracts.Remove(selectedContract);
                UpdateTotalSumRow();
                UpdateChangeCommandsState();
            }
        }

        private bool CanSaveChanges()
        {
            if (patientId == SpecialValues.NonExistingId || selectedContract == null || patientId == SpecialValues.NewId)
            {
                return false;
            }
            return contractItemsTracker.HasChanges;
        }

        private bool CanRemoveContract()
        {
            if (selectedContract == null)
            {
                return false;
            }
            return true;
        }

        private bool CanAddContract()
        {
            if (patientId == SpecialValues.NonExistingId || patientId == SpecialValues.NewId)
            {
                return false;
            }
            return true;
        }

        private int FirstUnused(int[] numbers)
        {
            var i = 1;
            foreach (var t in numbers.Distinct().OrderBy(x => x))
            {
                if (t != i)
                {
                    break;
                }
                i++;
            }
            return i;
        }

        private readonly CommandWrapper saveChangesCommandWrapper;

        private bool SaveContract()
        {
            FailureMediator.Deactivate();
            if (!IsValid)
            {
                FailureMediator.Activate("Проверьте правильность заполнения полей.", null, null, true);
                return false;
            }
            var contract = new RecordContract();
            if (selectedContract.Id != SpecialValues.NewId)
            {
                contract = contractService.GetContractById(selectedContract.Id).First();
            }

            log.InfoFormat("Saving contract data with Id {0} for patient with Id = {1}", ((contract == null || contract.Id == SpecialValues.NewId) ? "(New contract)" : contract.Id.ToString()), patientId);
            BusyMediator.Activate("Сохранение изменений...");
            var saveSuccesfull = false;

            if (!contract.Number.HasValue)
            {
                var beginYear = new DateTime(contractBeginDateTime.Year, 1, 1);
                var endYear = new DateTime(contractBeginDateTime.Year, 12, 31);
                contract.Number = FirstUnused(contractService.GetContracts(null, beginYear, endYear).Select(x => x.Number.Value).ToArray());
            }
            contract.BeginDateTime = contractBeginDateTime;
            contract.EndDateTime = contractEndDateTime;
            contract.FinancingSourceId = selectedFinancingSourceId;
            contract.ClientId = selectedClient.Value;
            contract.ConsumerId = patientId;
            contract.ContractName = personService.GetPatientQuery(selectedClient.Value).First().ShortName;
            contract.PaymentTypeId = selectedPaymentTypeId;
            contract.TransactionNumber = string.Empty;
            contract.TransactionDate = string.Empty;
            if (IsCashless)
            {
                contract.TransactionNumber = transationNumber;
                contract.TransactionDate = transationDate;
            }
            contract.Priority = 1;
            contract.InUserId = selectedRegistratorId;
            contract.InDateTime = DateTime.Now;
            contract.OrgId = null;
            contract.OrgDetails = string.Empty;
            contract.ContractCost = 0;
            contract.Options = string.Empty;

            contract.RecordContractItems.Clear();

            foreach (var item in contractItems.Where(x => !x.IsSection))
            {
                var contractItem = contractService.GetContractItemById(item.Id).FirstOrDefault();
                if (contractItem == null)
                {
                    contractItem = new RecordContractItem();
                }
                contractItem.Id = item.Id;
                contractItem.AssignmentId = item.AssignmentId;
                contractItem.RecordTypeId = item.RecordTypeId;
                contractItem.IsPaid = item.IsPaid;
                contractItem.Count = item.RecordCount;
                contractItem.Cost = item.RecordCost;
                contractItem.Appendix = item.Appendix;
                contractItem.InUserId = contract.InUserId;
                contractItem.InDateTime = contract.InDateTime;

                contract.RecordContractItems.Add(contractItem);
            }

            try
            {
                contract.Id = contractService.SaveContractData(contract);
                saveSuccesfull = true;
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Failed to save RecordContract with Id {0} for patient with Id {1}", ((contract == null || contract.Id == SpecialValues.NewId) ? "(New contract)" : contract.Id.ToString()), patientId);
                FailureMediator.Activate("Не удалось сохранить договор. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {
                    selectedContract.Id = contract.Id;
                    saveWasRequested = false;
                    OnPropertyChanged(string.Empty);
                    LoadContractItems();
                    SelectedContract.ContractNumber = contract.Number.ToSafeString();
                    SelectedContract.ContractName = contract.DisplayName;
                    SelectedContract.Client = new FieldValue { Field = contract.Person.FullName + ", " + contract.Person.BirthYear, Value = contract.Person.Id };
                    SelectedContract.ContractBeginDate = contract.BeginDateTime;
                    SelectedContract.ContractEndDate = contract.EndDateTime;
                    UpdateTotalSumRow();
                    contractItemsTracker.AcceptChanges();
                    contractItemsTracker.IsEnabled = true;
                    UpdateChangeCommandsState();
                }
            }
            return saveSuccesfull;
        }

        private void AddContract()
        {
            if (patientId == SpecialValues.NonExistingId || Contracts == null || Contracts.Any(x => x.Id == 0))
            {
                return;
            }
            var patient = personService.GetPatientQuery(patientId).First();
            Contracts.Add(new ContractViewModel
                          {
                              Id = 0,
                              ContractNumber = string.Empty,
                              ContractName = "НОВЫЙ ДОГОВОР",
                              Consumer = patient.ToString(),
                              ContractCost = 0,
                              ContractBeginDate = DateTime.Now,
                              ContractEndDate = DateTime.Now,
                              FinancingSourceId = -1,
                              PaymentTypeId = recordService.GetPaymentTypes().First(x => x.Options.Contains(OptionValues.Cash)).Id,
                              RegistratorId = -1,
                              Client = new FieldValue { Value = patient.Id, Field = patient.FullName + ", " + patient.BirthYear }
                          });
            SelectedContract = contracts.First(x => x.Id == 0);
            UpdateChangeCommandsState();
        }

        #region Implementation IDataErrorInfo

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
                    case "SelectedFinancingSourceId":
                        result = SelectedFinancingSourceId == SpecialValues.NonExistingId ? "Укажите источник финансирования" : string.Empty;
                        break;
                    case "SelectedRegistratorId":
                        result = SelectedRegistratorId == SpecialValues.NonExistingId ? "Укажите ответственного за договор" : string.Empty;
                        break;
                    case "SelectedPaymentTypeId":
                        result = SelectedPaymentTypeId == SpecialValues.NonExistingId ? "Укажите метод оплаты" : string.Empty;
                        break;
                    case "SelectedClient":
                        result = SelectedClient == null ? "Укажите заказчика" : string.Empty;
                        break;
                    case "TransationNumber":
                        result = string.IsNullOrEmpty(TransationNumber) && IsCashless ? "Укажите № транзакции" : string.Empty;
                        break;
                    case "TransationDate":
                        result = string.IsNullOrEmpty(TransationDate) && IsCashless ? "Укажите дату транзакции" : string.Empty;
                        break;
                    case "ContractItems":
                        result = !ContractItems.Any() ? "Договор должен содержать хотя бы одну услугу" : string.Empty;
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