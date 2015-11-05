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
using PatientInfoModule.Misc;
using PatientInfoModule.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using Prism.Events;
using System.ComponentModel;

namespace PatientInfoModule.ViewModels
{
    public class PatientContractsViewModel : BindableBase, IConfirmNavigationRequest
    {
        private readonly IPatientService personService;
        private readonly IContractService contractService;
        private readonly IRecordService recordService;
        private readonly IAssignmentService assignmentService;
        private readonly ILog log;
        private readonly ICacheService cacheService;
        private readonly IEventAggregator eventAggregator;
        private readonly CommandWrapper reloadContractsDataCommandWrapper;
        private readonly CommandWrapper reloadDataSourcesCommandWrapper;
        private readonly ChangeTracker changeTracker;

        public PatientContractsViewModel(IPatientService personService, IContractService contractService, IRecordService recordService, IAssignmentService assignmentService, ILog log, ICacheService cacheService, IEventAggregator eventAggregator)
        {
            if (personService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.personService = personService;
            this.contractService = contractService;
            this.recordService = recordService;
            this.assignmentService = assignmentService;
            this.log = log;
            this.cacheService = cacheService;
            this.eventAggregator = eventAggregator;
            patientId = SpecialValues.NonExistingId;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            changeTracker = new ChangeTracker();
            changeTracker.PropertyChanged += OnChangesTracked;
            reloadContractsDataCommandWrapper = new CommandWrapper { Command = new DelegateCommand(() => LoadContractsAsync(patientId)) };
            reloadDataSourcesCommandWrapper = new CommandWrapper { Command = new DelegateCommand(LoadDataSources) };
           
            addContractCommand = new DelegateCommand(AddContract);
            saveContractCommand = new DelegateCommand(SaveContract, CanSaveChanges);
            removeContractCommand = new DelegateCommand(RemoveContract, CanRemoveContract);
            printContractCommand = new DelegateCommand(PrintContract);
            printAppendixCommand = new DelegateCommand(PrintAppendix);
            addRecordCommand = new DelegateCommand(AddRecord);
            removeRecordCommand = new DelegateCommand(RemoveRecord);
            addAppendixCommand = new DelegateCommand(AddAppendix);
            removeAppendixCommand = new DelegateCommand(RemoveAppendix);

            saveChangesCommandWrapper = new CommandWrapper { Command = saveContractCommand };
            IsContractSelected = false;            
        }

        private void OnChangesTracked(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) || string.CompareOrdinal(e.PropertyName, "HasChanges") == 0)
            {
                saveContractCommand.RaiseCanExecuteChanged();
                removeContractCommand.RaiseCanExecuteChanged();
            }
        }

        public BusyMediator BusyMediator { get; set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }        

        private int patientId;

        public async void LoadDataSources()
        {
            CriticalFailureMediator.Deactivate();
            var contractRecord = await recordService.GetRecordTypesByOptions("|contract|").FirstOrDefaultAsync();
            var reliableStaff = await recordService.GetRecordTypeRolesByOptions("|responsible|contract|pay|").FirstOrDefaultAsync();
            if (contractRecord == null || reliableStaff == null)
            {
                CriticalFailureMediator.Activate("В МИС не найдена информация об услуге 'Договор' и/или об ответственных за выполнение. Отсутствует запись в таблицах RecordTypes, RecordTypeRoles.", reloadDataSourcesCommandWrapper, null);
                return;
            }
            var personStaffs = await personService.GetAllowedPersonStaffs(contractRecord.Id, reliableStaff.Id).ToArrayAsync();
            if (!personStaffs.Any())
            {
                CriticalFailureMediator.Activate("В МИС не найдена информация о правах на выполнение услуги. Отсутствует запись в таблице RecordTypeRolePermissions.", reloadDataSourcesCommandWrapper, null);
                return;
            }
            List<FieldValue> elements = new List<FieldValue>();
            elements.Add(new FieldValue() { Value = -1, Field = "- все -" });
            elements.AddRange(personStaffs.Select(x => new FieldValue() { Value = x.Id, Field = x.Person.ShortName }));           
            FilterRegistrators = new ObservableCollectionEx<FieldValue>(elements);
            Registrators = new ObservableCollectionEx<FieldValue>(elements);

            List<FieldValue> finSources = new List<FieldValue>();
            var fSources = await recordService.GetActiveFinancingSources().ToArrayAsync();
            finSources.Add(new FieldValue() { Value = -1, Field = "- выберите ист. финансирования -" });
            finSources.AddRange(fSources.Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
            FinancingSources = new ObservableCollectionEx<FieldValue>(finSources);

            List<FieldValue> paymentTypesSource = new List<FieldValue>();
            var paymentSources = await recordService.GetPaymentTypes().ToArrayAsync();
            paymentTypesSource.Add(new FieldValue() { Value = -1, Field = "- выберите метод оплаты -" });
            paymentTypesSource.AddRange(paymentSources.Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
            PaymentTypes = new ObservableCollectionEx<FieldValue>(paymentTypesSource);

            IsCashless = false;
            PersonSuggestionProvider = new PersonSuggestionProvider(personService);
            RecordTypesSuggestionProvider = new RecordTypesSuggestionProvider(recordService);
            Contracts = new ObservableCollectionEx<ContractViewModel>();
            Assignments = new ObservableCollectionEx<ContractAssignmentsViewModel>();

            SelectedRegistratorId = -1;
            SelectedPaymentTypeId = -1;
            SelectedFinancingSourceId = -1;
            ContractBeginDateTime = DateTime.Now;
            ContractEndDateTime = DateTime.Now;
            ContractName = "НОВЫЙ ДОГОВОР";
            ContractsCount = 0;
            ContractsSum = "0 руб.";

            IsAssignRecordsChecked = false;
            IsNewRecordChecked = false;

            SelectedFilterRegistratorId = -1;
        }

        private CancellationTokenSource currentLoadingToken;

        private async void LoadContractsAsync(int patientId)
        {
            this.patientId = patientId;
            saveContractCommand.RaiseCanExecuteChanged();
            removeContractCommand.RaiseCanExecuteChanged();
            if (patientId == SpecialValues.NewId || patientId == SpecialValues.NonExistingId)
            {
                return;
            }
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            CriticalFailureMediator.Deactivate();
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate("Загрузка договоров пациента...");
            log.InfoFormat("Loading contracts for patient with Id {0}...", patientId);
            try
            {
                var result = await contractService
                                .GetContracts(patientId, null, null, selectedFilterRegistratorId)
                                .OrderBy(x => x.BeginDateTime)
                                .Select(x => new
                                {
                                    Id = x.Id,
                                    ContractNumber = x.Number,
                                    Client = x.Person.ShortName,
                                    ContractCost = x.RecordContractItems.Where(a => a.IsPaid).Sum(a => a.Cost),
                                    ContractBeginDate = x.BeginDateTime
                                }).ToArrayAsync(token);
               
                ContractsCount = result.Count();
                ContractsSum = result.Sum(x => x.ContractCost) + " руб.";
                Contracts = new ObservableCollectionEx<ContractViewModel>();
                ContractItems = new ObservableCollectionEx<ContractItemViewModel>();
                Contracts.AddRange(result.Select(x => new ContractViewModel()
                    {
                        Id = x.Id,
                        ContractNumber = x.ContractNumber.ToSafeString(),
                        Client = x.Client,
                        ContractCost = x.ContractCost,
                        ContractBeginDate = x.ContractBeginDate.ToShortDateString()
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
                log.ErrorFormatEx(ex, "Failed to load contracts for patient with Id {0}", patientId);
                CriticalFailureMediator.Activate("Не удалость загрузить договоры пациента. Попробуйте еще раз или обратитесь в службу поддержки", reloadContractsDataCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
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
                ContractBeginDateTime = contract.BeginDateTime;
                ContractEndDateTime = contract.EndDateTime;
                ContractName = contract.DisplayName;
                SelectedFinancingSourceId = contract.FinancingSourceId;
                SelectedRegistratorId = contract.InUserId;
                SelectedPaymentTypeId = contract.PaymentTypeId;
                SelectedClient = contract.Person;
                Consumer = contract.Person1.FullName;
                IsCashless = false;
                
                Assignments = new ObservableCollectionEx<ContractAssignmentsViewModel>(assignmentService.GetPersonAssignments(this.patientId).ToList()
                                            .Select(x => new ContractAssignmentsViewModel()
                                            {
                                                Id = x.Id,
                                                RecordTypeId = x.RecordTypeId,
                                                AssignDateTime = x.AssignDateTime,
                                                RecordTypeName = x.RecordType.Name,
                                                RecordTypeCost = recordService.GetRecordTypeCost(x.RecordTypeId)
                                            }));
                LoadContractItems();
            }
        }

        private void ClearData()
        {
            ContractName = "НОВЫЙ ДОГОВОР";
            SelectedRegistratorId = -1;
            SelectedFinancingSourceId = -1;
            SelectedPaymentTypeId = -1;
            IsCashless = false;
            SelectedClient = null;
            Consumer = string.Empty;
            ContractItems.Clear();            
        }

        private void LoadContractItems()
        {
            ContractItems = new ObservableCollectionEx<ContractItemViewModel>();
            var items = contractService.GetContractItems(selectedContract.Id);
            foreach (var groupedItem in items.GroupBy(x => x.Appendix).OrderBy(x => x.Key))
            {
                if (groupedItem.Key.HasValue)
                    AddSectionRow(groupedItem.Key.Value, Color.LightSalmon, HorizontalAlignment.Center);
                foreach (var item in groupedItem)
                    AddContractItemRow(item);
            }
            if (items.Any())
                AddSectionRow(-1, Color.LightGreen, HorizontalAlignment.Right);
        }

        private void AddContractItemRow(RecordContractItem item)
        {
            ContractItems.Add(new ContractItemViewModel(recordService)
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
            });
        }

        private void AddSectionRow(int appendix, Color backColor, HorizontalAlignment alignment, int insertPosition = -1)
        {
            var item = new ContractItemViewModel(recordService)
            {
                IsSection = true,
                SectionName = appendix != -1 ? "Доп. соглашение № " + appendix.ToSafeString() : ("ИТОГО: " + contractItems.Sum(x => x.RecordCost * x.RecordCount) + " руб."),
                Appendix = appendix,
                SectionAlignment = alignment,
                SectionBackColor = backColor
            };
            if (insertPosition == -1)
                ContractItems.Add(item);
            else
                ContractItems.Insert(insertPosition, item);
        }

        private void UpdateTotalSumRow()
        {
            if (contractItems.Any(x => x.IsSection && x.Appendix == -1))
                contractItems.First(x => x.IsSection && x.Appendix == -1).SectionName = "ИТОГО: " + contractItems.Sum(x => x.RecordCost * x.RecordCount) + " руб.";
            else
                AddSectionRow(-1, Color.LightGreen, HorizontalAlignment.Right);
            if (selectedContract != null)
                SelectedContract.ContractCost = contractService.GetContractCost(selectedContract.Id);
            ContractsCount = Contracts.Count();
            ContractsSum = Contracts.Sum(x => x.ContractCost) + " руб.";            
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

        private ObservableCollectionEx<FieldValue> filterRegistrators;
        public ObservableCollectionEx<FieldValue> FilterRegistrators
        {
            get { return filterRegistrators; }
            set { SetProperty(ref filterRegistrators, value); }
        }

        private int selectedFilterRegistratorId;
        public int SelectedFilterRegistratorId
        {
            get { return selectedFilterRegistratorId; }
            set
            {
                SetProperty(ref selectedFilterRegistratorId, value);
                LoadContractsAsync(this.patientId);
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
            set
            {
                changeTracker.Track(selectedFinancingSourceId, value); 
                SetProperty(ref selectedFinancingSourceId, value);
            }
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
                changeTracker.Track(selectedFinancingSourceId, value); 
                if (SetProperty(ref selectedPaymentTypeId, value))
                    IsCashless = (value == -1) || recordService.GetPaymentTypeById(value).Any(x => x.Options.Contains("|cashless|"));
            }
        }

        private DateTime contractBeginDateTime;
        public DateTime ContractBeginDateTime
        {
            get { return contractBeginDateTime; }
            set
            {
                changeTracker.Track(contractBeginDateTime, value); 
                SetProperty(ref contractBeginDateTime, value);
            }
        }

        private DateTime contractEndDateTime;
        public DateTime ContractEndDateTime
        {
            get { return contractEndDateTime; }
            set
            {
                changeTracker.Track(contractEndDateTime, value); 
                SetProperty(ref contractEndDateTime, value); 
            }
        }

        private bool isCashless;
        public bool IsCashless
        {
            get { return isCashless; }
            set { SetProperty(ref isCashless, value); }
        }

        private PersonSuggestionProvider personSuggestionProvider;
        public PersonSuggestionProvider PersonSuggestionProvider
        {
            get { return personSuggestionProvider; }
            set { SetProperty(ref personSuggestionProvider, value); }
        }

        private Person selectedClient;
        public Person SelectedClient
        {
            get { return selectedClient; }
            set 
            {
                changeTracker.Track(selectedClient, value); 
                SetProperty(ref selectedClient, value);
            }
        }

        private string consumer;
        public string Consumer
        {
            get { return consumer; }
            set 
            {
                SetProperty(ref consumer, value);
            }
        }

        private ObservableCollectionEx<ContractItemViewModel> contractItems;
        public ObservableCollectionEx<ContractItemViewModel> ContractItems
        {
            get { return contractItems; }
            set 
            {
                changeTracker.Track(contractItems, value); 
                SetProperty(ref contractItems, value);
            }
        }

        private ContractItemViewModel selectedContractItem;
        public ContractItemViewModel SelectedContractItem
        {
            get { return selectedContractItem; }
            set { SetProperty(ref selectedContractItem, value); }
        }

        private bool isNewRecordChecked;
        public bool IsNewRecordChecked
        {
            get { return isNewRecordChecked; }
            set { SetProperty(ref isNewRecordChecked, value); }
        }

        private bool isAssignRecordsChecked;
        public bool IsAssignRecordsChecked
        {
            get { return isAssignRecordsChecked; }
            set { SetProperty(ref isAssignRecordsChecked, value); }
        }

        private double assignRecordTypeCost;
        public double AssignRecordTypeCost
        {
            get { return assignRecordTypeCost; }
            set { SetProperty(ref assignRecordTypeCost, value); }
        }

        private int recordsCount;
        public int RecordsCount
        {
            get { return recordsCount; }
            set
            {
                if (SetProperty(ref recordsCount, value))
                   AssignRecordTypeCost = (recordService.GetRecordTypeCost(selectedRecord.Id) * recordsCount);
            }
        }

        private RecordTypesSuggestionProvider recordTypesSuggestionProvider;
        public RecordTypesSuggestionProvider RecordTypesSuggestionProvider
        {
            get { return recordTypesSuggestionProvider; }
            set { SetProperty(ref recordTypesSuggestionProvider, value); }
        }

        private RecordType selectedRecord;
        public RecordType SelectedRecord
        {
            get { return selectedRecord; }
            set
            {
                if (SetProperty(ref selectedRecord, value))
                    RecordsCount = 1;
            }
        }

        private bool isContractSelected;
        public bool IsContractSelected
        {
            get { return isContractSelected; }
            set 
            {
                SetProperty(ref isContractSelected, value);
            }
        }

        private string transationNumber;
        public string TransationNumber
        {
            get { return transationNumber; }
            set
            {
                changeTracker.Track(transationNumber, value); 
                SetProperty(ref transationNumber, value); 
            }
        }

        private string transationDate;
        public string TransationDate
        {
            get { return transationDate; }
            set
            {
                changeTracker.Track(transationDate, value); 
                SetProperty(ref transationDate, value);
            }
        }

        private ObservableCollectionEx<ContractAssignmentsViewModel> assignments;
        public ObservableCollectionEx<ContractAssignmentsViewModel> Assignments
        {
            get { return assignments; }
            set { SetProperty(ref assignments, value); }
        }

        private ContractAssignmentsViewModel selectedAssignment;
        public ContractAssignmentsViewModel SelectedAssignment
        {
            get { return selectedAssignment; }
            set { SetProperty(ref selectedAssignment, value); }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialValues.NonExistingId;
            if (targetPatientId != patientId)
            {
                this.patientId = targetPatientId;
                LoadDataSources();
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

        public ICommand AddContractCommand { get { return addContractCommand; } }
        public ICommand SaveContractCommand { get { return saveContractCommand; } }
        public ICommand RemoveContractCommand { get { return removeContractCommand; } }
        public ICommand PrintContractCommand { get { return printContractCommand; } }
        public ICommand PrintAppendixCommand { get { return printAppendixCommand; } }
        public ICommand AddRecordCommand { get { return addRecordCommand; } }
        public ICommand RemoveRecordCommand { get { return removeRecordCommand; } }
        public ICommand AddAppendixCommand { get { return addAppendixCommand; } }
        public ICommand RemoveAppendixCommand { get { return removeAppendixCommand; } }

       
        private void RemoveAppendix()
        {
            if (selectedContractItem != null && selectedContractItem.IsSection /*&& this.dialogService.AskUser("Удалить " + SelectedContractItem.SectionName + " вместе со вложенными услугами ?", true) == true*/)
            {
                int? appendix = selectedContractItem.Appendix;
                foreach (var item in ContractItems.ToList())
                {
                    if (item.Appendix == appendix)
                    {
                        if (!item.IsSection)
                            contractService.DeleteContractItemById(item.Id);
                        ContractItems.Remove(item);
                    }
                }
                UpdateTotalSumRow();
            }
        }

        private void AddAppendix()
        {
            int? appendixCount = contractItems.Where(x => x.IsSection && x.Appendix > 0).Select(x => x.Appendix.Value).OrderByDescending(x => x).FirstOrDefault();
            if (!contractItems.Any(x => x.IsSection && x.Appendix == (appendixCount.ToInt() + 1)))
                AddSectionRow(appendixCount.ToInt() + 1, Color.LightSalmon, HorizontalAlignment.Center, contractItems.Count - 1);
        }

        private void RemoveRecord()
        {
            if (selectedRecord == null) return;
            //if (this.dialogService.AskUser("Удалить услугу " + SelectedContractItem.RecordTypeName + " из договора ?", true) == true)
            //{
                contractService.DeleteContractItemById(selectedContractItem.Id);
                ContractItems.Remove(selectedContractItem);
                UpdateTotalSumRow();
            //}
        }

        private void AddRecord()
        {
            int? appendixCount = contractItems.Where(x => x.Appendix > 0).Select(x => x.Appendix.Value).OrderByDescending(x => x).FirstOrDefault();
            if (isAssignRecordsChecked)
            {
                foreach (var assignment in assignments.Where(x => x.IsSelected))
                {
                    int insertPosition = contractItems.Any() ? contractItems.Count - 1 : 0;
                    ContractItems.Insert(insertPosition, new ContractItemViewModel(recordService)
                    {
                        Id = 0,
                        RecordContractId = (int?)null,
                        AssignmentId = assignment.Id,
                        RecordTypeId = assignment.RecordTypeId,
                        IsPaid = true,
                        RecordTypeName = assignment.RecordTypeName,
                        RecordCount = 1,
                        RecordCost = assignment.RecordTypeCost,
                        Appendix = (appendixCount == 0 ? (int?)null : appendixCount)
                    });
                }
            }
            else
            {
                if (selectedRecord == null) return;
                int insertPosition = contractItems.Any() ? contractItems.Count - 1 : 0;
                ContractItems.Insert(insertPosition, new ContractItemViewModel(recordService)
                {
                    Id = 0,
                    RecordContractId = (int?)null,
                    AssignmentId = (int?)null,
                    RecordTypeId = selectedRecord.Id,
                    IsPaid = true,
                    RecordTypeName = selectedRecord.Name,
                    RecordCount = recordsCount,
                    RecordCost = assignRecordTypeCost,
                    Appendix = (appendixCount == 0 ? (int?)null : appendixCount)
                });
            }
            UpdateTotalSumRow();
            changeTracker.Track(null, contractItems);
            saveContractCommand.RaiseCanExecuteChanged();
        }

        private void PrintAppendix()
        {
            throw new NotImplementedException();
        }

        private void PrintContract()
        {
            throw new NotImplementedException();
        }

        private void RemoveContract()
        {
            //if (this.dialogService.AskUser("Удалить договор " + SelectedContract.ContractName + "?", true) == true)
            //{
                var visit = recordService.GetVisitsByContractId(SelectedContract.Id).FirstOrDefault();
                if (visit != null)
                {
                    //this.dialogService.ShowMessage("Данный договор уже закреплен за случаем обращения пациента " + personService.GetPersonById(visit.PersonId).ShortName + ". Удаление договора невозможно.");
                    return;
                }
                if (selectedContract.Id != SpecialValues.NewId)
                    contractService.DeleteContract(selectedContract.Id);

                Contracts.Remove(selectedContract);
                UpdateTotalSumRow();
                saveContractCommand.RaiseCanExecuteChanged();
                removeContractCommand.RaiseCanExecuteChanged();
            //}
        }

        private bool CanSaveChanges()
        {
            if (patientId == SpecialValues.NonExistingId || selectedContract == null || patientId == SpecialValues.NewId)
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
        private CancellationTokenSource currentOperationToken;

        private void SaveContract()
        {
            CriticalFailureMediator.Deactivate();
            if (currentOperationToken != null)
            {
                currentOperationToken.Cancel();
                currentOperationToken.Dispose();
            }
            currentOperationToken = new CancellationTokenSource();
            var token = currentOperationToken.Token;

            RecordContract contract = new RecordContract();
            if (selectedContract.Id != SpecialValues.NewId)
                contract = contractService.GetContractById(selectedContract.Id).First();

            log.InfoFormat("Saving contract data with Id {0} for patient with Id = {1}", ((contract == null || contract.Id == SpecialValues.NewId) ? "(New contract)" : contract.Id.ToString()), patientId);
            BusyMediator.Activate("Сохранение изменений...");
            var saveSuccesfull = false;
                        
            if (!contract.Number.HasValue)
            {
                DateTime beginYear = new DateTime(contractBeginDateTime.Year, 1, 1);
                DateTime endYear = new DateTime(contractBeginDateTime.Year, 12, 31);
                contract.Number = FirstUnused(contractService.GetContracts(null, beginYear, endYear).Select(x => x.Number.Value).ToArray());
            }
            contract.BeginDateTime = contractBeginDateTime;
            contract.EndDateTime = contractEndDateTime;
            contract.FinancingSourceId = selectedFinancingSourceId;
            contract.ClientId = selectedClient.Id;
            contract.ConsumerId = patientId;
            contract.ContractName = selectedClient.ShortName;
            contract.PaymentTypeId = selectedPaymentTypeId;
            if (recordService.GetPaymentTypeById(selectedPaymentTypeId).Select(x => x.Options).Contains("|cashless|"))
            {
                contract.TransactionNumber = transationNumber;
                contract.TransactionDate = transationDate;
            }
            contract.Priority = 1;
            contract.InUserId = selectedRegistratorId;
            contract.InDateTime = DateTime.Now;
            contract.OrgId = (int?)null;
            contract.ContractCost = 0;
            contract.Options = string.Empty;
            
            var recordContractsItems = contractItems.Where(x => !x.IsSection)
                .Select(x => new RecordContractItem()
                {
                    Id = x.Id,
                    AssignmentId = x.AssignmentId,
                    RecordTypeId = x.RecordTypeId,
                    IsPaid = x.IsPaid,
                    Count = x.RecordCount,
                    Cost = x.RecordCost,
                    Appendix = x.Appendix,
                    InUserId = contract.InUserId,
                    InDateTime = contract.InDateTime
                }).ToArray();

            string message = string.Empty;
            try
            {
                contract.Id = contractService.SaveContractData(contract, recordContractsItems, out message);
                saveSuccesfull = true;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to Save RecordContract. " + message));
                log.ErrorFormatEx(ex, "Failed to save RecordContract with Id {0} for patient with Id {1}", ((contract == null || contract.Id == SpecialValues.NewId) ? "(New contract)" : contract.Id.ToString()), patientId);
                CriticalFailureMediator.Activate("Не удалось сохранить договор. Попробуйте еще раз или обратитесь в службу поддержки", saveChangesCommandWrapper, ex);
                return;
            }
            finally
            {
                BusyMediator.Deactivate();
                if (saveSuccesfull)
                {
                    selectedContract.Id = contract.Id;
                    LoadContractItems();
                    SelectedContract.ContractNumber = contract.Number.ToSafeString();
                    SelectedContract.Client = contract.ContractName;
                    SelectedContract.ContractBeginDate = contract.BeginDateTime.ToShortDateString();
                    SelectedContract.ContractEndDate = contract.EndDateTime.ToShortDateString();
                    UpdateTotalSumRow();
                    changeTracker.UntrackAll();
                    saveContractCommand.RaiseCanExecuteChanged();
                    removeContractCommand.RaiseCanExecuteChanged();
                }
            }                        
        }

        private void AddContract()
        {
            if (patientId == SpecialValues.NonExistingId || Contracts == null || Contracts.Any(x => x.Id == 0)) return;
            Contracts.Add(new ContractViewModel()
            {
                Id = 0,
                ContractNumber = string.Empty,
                ContractName = "НОВЫЙ ДОГОВОР",
                Client = "НОВЫЙ ДОГОВОР",
                ContractCost = 0,
                ContractBeginDate = DateTime.Now.ToShortDateString(),
                ContractEndDate = DateTime.Now.ToShortDateString()
            });            
            SelectedContract = contracts.First(x => x.Id == 0);
            SelectedPaymentTypeId = recordService.GetPaymentTypes().First(x => x.Options.Contains("|cash|")).Id;
            Consumer = personService.GetPatientQuery(this.patientId).First().FullName;
            ContractBeginDateTime = DateTime.Now.Date;
            ContractEndDateTime = DateTime.Now.Date;
            saveContractCommand.RaiseCanExecuteChanged();
            removeContractCommand.RaiseCanExecuteChanged();
        }
    }
}
