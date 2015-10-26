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

namespace PatientInfoModule.ViewModels
{
    public class PatientContractsViewModel : BindableBase, IConfirmNavigationRequest
    {
        private readonly IPatientService personService;
        private readonly IContractService contractService;
        private readonly IRecordService recordService;
        private readonly ILog log;
        private readonly ICacheService cacheService;
        private readonly CommandWrapper reloadContractsDataCommandWrapper;

        public PatientContractsViewModel(IPatientService personService, IContractService contractService, IRecordService recordService, ILog log, ICacheService cacheService)
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
            this.personService = personService;
            this.contractService = contractService;
            this.recordService = recordService;
            this.log = log;
            this.cacheService = cacheService;
            BusyMediator = new BusyMediator();
            CriticalFailureMediator = new CriticalFailureMediator();
            reloadContractsDataCommandWrapper = new CommandWrapper
                                              {
                                                  Command = new DelegateCommand(() => LoadContractsAsync()),
                                                  CommandName = "Повторить",
                                              };
        }

        public BusyMediator BusyMediator { get; set; }

        public CriticalFailureMediator CriticalFailureMediator { get; private set; }        

        private int patientId;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.patientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialId.NonExisting;
            LoadDataSources();
        }

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

        private async void LoadContractsAsync()
        {
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
                contractsQuery = contractService.GetContracts(patientId, null, null, SelectedFilterRegistratorId);
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

        private async void LoadContractDataAsync()
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
            ContractItems.Add(new ContractItemViewModel()
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
            var item = new ContractItemViewModel()
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
                    LoadContractDataAsync();
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
                    IsCashless = (value != -1) && recordService.GetPaymentTypeById(value).Any(x => x.Options.Contains("|cashless|"));
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

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            throw new NotImplementedException();
        }
    }
}
