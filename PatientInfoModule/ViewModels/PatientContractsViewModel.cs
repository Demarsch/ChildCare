﻿using System;
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

namespace PatientInfoModule.ViewModels
{
    public class PatientContractsViewModel : BindableBase, IConfirmNavigationRequest
    {
        private readonly IPatientService personService;
        private readonly IContractService contractService;
        private readonly IRecordService recordService;
        private readonly ILog log;
        private readonly ICacheService cacheService;
        private readonly IEventAggregator eventAggregator;
        private readonly CommandWrapper reloadContractsDataCommandWrapper;

        public PatientContractsViewModel(IPatientService personService, IContractService contractService, IRecordService recordService, ILog log, ICacheService cacheService, IEventAggregator eventAggregator)
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
            this.log = log;
            this.cacheService = cacheService;
            this.eventAggregator = eventAggregator;
            patientId = SpecialId.NonExisting;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            reloadContractsDataCommandWrapper = new CommandWrapper
                                              {
                                                  Command = new DelegateCommand(() => LoadContractsAsync(patientId)),
                                                  CommandName = "Повторить",
                                              };

            addContractCommand = new DelegateCommand(AddContract);
            saveContractCommand = new DelegateCommand(SaveContract, CanSaveChanges);
            removeContractCommand = new DelegateCommand(RemoveContract);
            printContractCommand = new DelegateCommand(PrintContract);
            printAppendixCommand = new DelegateCommand(PrintAppendix);
            addRecordCommand = new DelegateCommand(AddRecord);
            removeRecordCommand = new DelegateCommand(RemoveRecord);
            addAppendixCommand = new DelegateCommand(AddAppendix);
            removeAppendixCommand = new DelegateCommand(RemoveAppendix);
            IsContractSelected = false;
        }

        public BusyMediator BusyMediator { get; set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }        

        private volatile int patientId;

        public void LoadDataSources()
        {
            var contractRecord = recordService.GetRecordTypesByOptions("|contract|").FirstOrDefault();
            var reliableStaff = recordService.GetRecordTypeRolesByOptions("|responsible|contract|pay|").FirstOrDefault();
            if (contractRecord == null || reliableStaff == null) return;

            var personStaffs = personService.GetAllowedPersonStaffs(contractRecord.Id, reliableStaff.Id);
            if (!personStaffs.Any()) return;
            List<FieldValue> elements = new List<FieldValue>();
            elements.Add(new FieldValue() { Value = -1, Field = "- все -" });
            elements.AddRange(personStaffs.Select(x => new FieldValue() { Value = x.Id, Field = x.Person.ShortName }));           
            FilterRegistrators = new ObservableCollectionEx<FieldValue>(elements);
            Registrators = new ObservableCollectionEx<FieldValue>(elements);

            List<FieldValue> finSources = new List<FieldValue>();
            finSources.Add(new FieldValue() { Value = -1, Field = "- выберите ист. финансирования -" });
            finSources.AddRange(recordService.GetActiveFinancingSources().Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
            FinancingSources = new ObservableCollectionEx<FieldValue>(finSources);

            List<FieldValue> paymentTypesSource = new List<FieldValue>();
            paymentTypesSource.Add(new FieldValue() { Value = -1, Field = "- выберите метод оплаты -" });
            paymentTypesSource.AddRange(recordService.GetPaymentTypes().Select(x => new FieldValue() { Value = x.Id, Field = x.Name }));
            PaymentTypes = new ObservableCollectionEx<FieldValue>(paymentTypesSource);

            IsCashless = false;
            PersonSuggestionProvider = new PersonSuggestionProvider(personService);
            Contracts = new ObservableCollectionEx<ContractViewModel>();

            SelectedRegistratorId = -1;
            SelectedPaymentTypeId = -1;
            SelectedFinancingSourceId = -1;
            ContractBeginDateTime = DateTime.Now;
            ContractEndDateTime = DateTime.Now;
            ContractName = "НОВЫЙ ДОГОВОР";

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
            if (patientId == SpecialId.New || patientId == SpecialId.NonExisting)
            {
                return;
            }
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            var loadingIsCompleted = false;
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;
            BusyMediator.Activate("Загрузка договоров пациента...");
            log.InfoFormat("Loading contracts for patient with Id {0}...", patientId);
            IDisposableQueryable<RecordContract> contractsQuery = null;
            try
            {
                contractsQuery = contractService.GetContracts(patientId, null, null, selectedFilterRegistratorId);
                var loadContractsTask = contractsQuery.OrderBy(x => x.BeginDateTime)
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            ContractNumber = x.Number,
                                            Client = x.Person.ShortName,
                                            Cost = x.RecordContractItems.Where(a => a.IsPaid).Sum(a => a.Cost),
                                            ContractDate = x.BeginDateTime
                                        }).ToArrayAsync(token);
                await Task.WhenAll(loadContractsTask, Task.Delay(AppConfiguration.PendingOperationDelay, token));
                var result = loadContractsTask.Result;

                ContractsCount = result.Count();
                ContractsSum = result.Sum(x => x.Cost) + " руб.";
                Contracts = new ObservableCollectionEx<ContractViewModel>();
                foreach (var contract in contractsQuery)
                    Contracts.Add(new ContractViewModel()
                    {
                        Id = contract.Id,
                        ContractNumber = contract.Number.ToSafeString(),
                        Client = contract.Person.ShortName,
                        ContractCost = contractService.GetContractCost(contract.Id).ToString() + " руб.",
                        ContractBeginDate = contract.BeginDateTime.ToShortDateString()
                    });
                if (Contracts.Any())
                    SelectedContract = Contracts.OrderByDescending(x => x.ContractBeginDate).First();
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
                CriticalFailureMediator.Activate("Не удалость загрузить договора пациента. Попробуйте еще раз или обратитесь в службу поддержки", reloadContractsDataCommandWrapper, ex);
                loadingIsCompleted = true;
            }
            finally
            {
                if (loadingIsCompleted)
                {
                    BusyMediator.Deactivate();
                }
                if (contractsQuery != null)
                {
                    contractsQuery.Dispose();
                }
            }   
        }

        private void LoadContractDataAsync()
        {
            if (SelectedContract.Id == SpecialId.New)
                ClearData();
            else
            {
                ContractItems = new ObservableCollectionEx<ContractItemViewModel>();
                var contract = contractService.GetContractById(SelectedContract.Id).First();
                ContractBeginDateTime = contract.BeginDateTime;
                ContractEndDateTime = contract.EndDateTime;
                ContractName = contract.DisplayName;
                SelectedFinancingSourceId = contract.FinancingSourceId;
                SelectedRegistratorId = contract.InUserId;
                SelectedPaymentTypeId = contract.PaymentTypeId;
                LoadContractItems();
            }
        }

        private void ClearData()
        {
            ContractName = "НОВЫЙ ДОГОВОР";
            SelectedRegistratorId = -1;
            SelectedFinancingSourceId = -1;
            SelectedPaymentTypeId = -1;
            IsCashless = true;
            SelectedClient = null;
            SelectedConsumer = null;
            ContractItems.Clear();
        }

        private void LoadContractItems()
        {
            var contractItems = contractService.GetContractItems(SelectedContract.Id);
            foreach (var groupedItem in contractItems.GroupBy(x => x.Appendix).OrderBy(x => x.Key))
            {
                if (groupedItem.Key.HasValue)
                    AddSectionRow(groupedItem.Key.Value, Color.LightSalmon, HorizontalAlignment.Center);
                foreach (var contractItem in groupedItem)
                    AddContractItemRow(contractItem);
            }
            if (contractItems.Any())
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
                SectionName = appendix != -1 ? "Доп. соглашение № " + appendix.ToSafeString() : ("ИТОГО: " + ContractItems.Sum(x => x.RecordCost * x.RecordCount) + " руб."),
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
            ContractItems.First(x => x.IsSection && x.Appendix == -1).SectionName = "ИТОГО: " + ContractItems.Sum(x => x.RecordCost * x.RecordCount) + " руб.";
            ContractsSum = ContractItems.Sum(x => x.RecordCost * x.RecordCount) + " руб.";
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
                if (SetProperty(ref selectedFilterRegistratorId, value))
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
                    IsContractSelected = true;
                    LoadContractDataAsync();
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
            set { SetProperty(ref selectedRegistratorId, value); }
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
            set { SetProperty(ref selectedFinancingSourceId, value); }
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
                if (SetProperty(ref selectedPaymentTypeId, value))
                    IsCashless = (value == -1) || recordService.GetPaymentTypeById(value).Any(x => x.Options.Contains("|cashless|"));
            }
        }

        private DateTime contractBeginDateTime;
        public DateTime ContractBeginDateTime
        {
            get { return contractBeginDateTime; }
            set { SetProperty(ref contractBeginDateTime, value); }
        }

        private DateTime contractEndDateTime;
        public DateTime ContractEndDateTime
        {
            get { return contractEndDateTime; }
            set { SetProperty(ref contractEndDateTime, value); }
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
            set { SetProperty(ref selectedClient, value); }
        }

        private Person selectedConsumer;
        public Person SelectedConsumer
        {
            get { return selectedConsumer; }
            set { SetProperty(ref selectedConsumer, value); }
        }

        private ObservableCollectionEx<ContractItemViewModel> contractItems;
        public ObservableCollectionEx<ContractItemViewModel> ContractItems
        {
            get { return contractItems; }
            set { SetProperty(ref contractItems, value); }
        }

        private ContractItemViewModel selectedContractItem;
        public ContractItemViewModel SelectedContractItem
        {
            get { return selectedContractItem; }
            set { SetProperty(ref selectedContractItem, value); }
        }

        private bool isAssignRecordsChecked;
        public bool IsAssignRecordsChecked
        {
            get { return isAssignRecordsChecked; }
            set { SetProperty(ref isAssignRecordsChecked, value); }
        }

        private bool isNewRecordChecked;
        public bool IsNewRecordChecked
        {
            get { return isNewRecordChecked; }
            set { SetProperty(ref isNewRecordChecked, value); }
        }

        private bool isContractSelected;
        public bool IsContractSelected
        {
            get { return isContractSelected; }
            set 
            {
                if (SetProperty(ref isContractSelected, value))
                {
                    ContractsCount = 0;
                    ContractsSum = string.Empty;
                }
            }
        }

        private string transationNumber;
        public string TransationNumber
        {
            get { return transationNumber; }
            set { SetProperty(ref transationNumber, value); }
        }

        private string transationDate;
        public string TransationDate
        {
            get { return transationDate; }
            set { SetProperty(ref transationDate, value); }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialId.NonExisting;
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
            if (SelectedContractItem.IsSection /*&& this.dialogService.AskUser("Удалить " + SelectedContractItem.SectionName + " вместе со вложенными услугами ?", true) == true*/)
            {
                int? appendix = SelectedContractItem.Appendix;
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
            int? appendixCount = ContractItems.Where(x => x.IsSection && x.Appendix > 0).Select(x => x.Appendix.Value).OrderByDescending(x => x).FirstOrDefault();
            if (!ContractItems.Any(x => x.IsSection && (x.Appendix == appendixCount.ToInt())))
                AddSectionRow(appendixCount.ToInt() + 1, Color.LightSalmon, HorizontalAlignment.Center, ContractItems.Count - 1);
        }

        private void RemoveRecord()
        {
            //if (this.dialogService.AskUser("Удалить услугу " + SelectedContractItem.RecordTypeName + " из договора ?", true) == true)
            //{
                contractService.DeleteContractItemById(SelectedContractItem.Id);
                ContractItems.Remove(SelectedContractItem);
                UpdateTotalSumRow();
            //}
        }

        private void AddRecord()
        {
            /*int? appendixCount = ContractItems.Where(x => x.Appendix > 0).Select(x => x.Appendix.Value).OrderByDescending(x => x).FirstOrDefault();
            if (isAssignRecordsChecked)
            {
                foreach (var assignment in assignments.Where(x => x.IsSelected))
                {
                    int insertPosition = ContractItems.Any() ? ContractItems.Count - 1 : 0;
                    ContractItems.Insert(insertPosition, new ContractItemDetailsViewModel()
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
                int insertPosition = ContractItems.Any() ? ContractItems.Count - 1 : 0;
                ContractItems.Insert(insertPosition, new ContractItemViewModel()
                {
                    Id = 0,
                    RecordContractId = (int?)null,
                    AssignmentId = (int?)null,
                    RecordTypeId = SelectedRecord.Id,
                    IsPaid = true,
                    RecordTypeName = SelectedRecord.Name,
                    RecordCount = RecordsCount,
                    RecordCost = AssignRecordTypeCost,
                    Appendix = (appendixCount == 0 ? (int?)null : appendixCount)
                });
            }
            UpdateTotalSumRow();*/
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
                contractService.DeleteContractItems(SelectedContract.Id);
                contractService.DeleteContract(SelectedContract.Id);
                Contracts.Remove(SelectedContract);
                ContractsCount = Contracts.Count;
            //}
        }

        private bool CanSaveChanges()
        {
            if (patientId == SpecialId.NonExisting)
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

        private void SaveContract()
        {            
            RecordContract contract = new RecordContract();
            if (SelectedContract.Id != SpecialId.New)
                contract = contractService.GetContractById(SelectedContract.Id).First();
            if (!contract.Number.HasValue)
            {
                DateTime beginYear = new DateTime(ContractBeginDateTime.Year, 1, 1);
                DateTime endYear = new DateTime(ContractBeginDateTime.Year, 12, 31);
                contract.Number = FirstUnused(contractService.GetContracts(null, beginYear, endYear).Select(x => x.Number.Value).ToArray());
            }
            contract.BeginDateTime = ContractBeginDateTime;
            contract.EndDateTime = ContractEndDateTime;
            contract.FinancingSourceId = SelectedFinancingSourceId;
            contract.ClientId = SelectedClient.Id;
            contract.ConsumerId = SelectedConsumer.Id;
            contract.ContractName = SelectedClient.ShortName;
            contract.PaymentTypeId = SelectedPaymentTypeId;
            if (recordService.GetPaymentTypeById(SelectedPaymentTypeId).Select(x => x.Options).Contains("|cashless|"))
            {
                contract.TransactionNumber = TransationNumber;
                contract.TransactionDate = TransationDate;
            }
            contract.Priority = 1;
            contract.InUserId = SelectedRegistratorId;
            contract.InDateTime = DateTime.Now;
            contract.OrgId = (int?)null;
            contract.ContractCost = 0;
            contract.Options = string.Empty;

            string message = string.Empty;
            contract.Id = contractService.SaveContractData(contract, out message);
            if (contract.Id == 0)
            {
                //dialogService.ShowError("При сохранении договора возникла ошибка: " + message);
                log.Error(string.Format("Failed to Save RecordContract. " + message));
                return;
            }
            SelectedContract.Id = contract.Id;

            foreach (ContractItemViewModel itemVM in ContractItems.Where(x => !x.IsSection))
            {
                RecordContractItem item = contractService.GetContractItemById(itemVM.Id).FirstOrDefault();
                if (item == null)
                    item = new RecordContractItem();
                item.RecordContractId = contract.Id;
                item.AssignmentId = itemVM.AssignmentId;
                item.RecordTypeId = itemVM.RecordTypeId;
                item.IsPaid = itemVM.IsPaid;
                item.Count = itemVM.RecordCount;
                item.Cost = itemVM.RecordCost;
                item.Appendix = itemVM.Appendix;
                item.InUserId = contract.InUserId;
                item.InDateTime = contract.InDateTime;
                item.Id = contractService.SaveContractItemData(item, out message);
                if (item.Id == 0)
                {
                    //dialogService.ShowError("При сохранении услуг по договору возникла ошибка: " + message);
                    log.Error(string.Format("Failed to Save RecordContractItem. " + message));
                    return;
                }
                itemVM.Id = item.Id;
            }

            SelectedContract.ContractNumber = contract.Number.ToSafeString();
            SelectedContract.Client = contract.ContractName;
            SelectedContract.ContractCost = contractService.GetContractCost(contract.Id).ToSafeString() + " руб.";
            SelectedContract.ContractBeginDate = contract.BeginDateTime.ToShortDateString();
            SelectedContract.ContractEndDate = contract.EndDateTime.ToShortDateString();
            UpdateTotalSumRow();

            //dialogService.ShowMessage("Данные сохранены");
            
        }

        private void AddContract()
        {
            if (patientId == SpecialId.NonExisting || Contracts.Any(x => x.Id == 0)) return;
            Contracts.Add(new ContractViewModel()
            {
                Id = 0,
                ContractNumber = string.Empty,
                ContractName = "НОВЫЙ ДОГОВОР",
                Client = "НОВЫЙ ДОГОВОР",
                ContractCost = string.Empty,
                ContractBeginDate = DateTime.Now.ToShortDateString(),
                ContractEndDate = DateTime.Now.ToShortDateString()
            });            
            SelectedContract = Contracts.First(x => x.Id == 0);
        }
    }
}
