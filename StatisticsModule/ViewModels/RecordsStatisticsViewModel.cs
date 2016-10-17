using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Classes;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using StatisticsModule.Services;
using Prism.Regions;
using System.Windows.Input;
using StatisticsModule.DTO;
using System.Collections.ObjectModel;
using Core.Misc;

namespace StatisticsModule.ViewModels
{
    public class RecordsStatisticsViewModel : BindableBase, IDisposable, INavigationAware
    {
        private readonly IStatisticsService statisticsService;

        private readonly IRecordTypesTree recordTypeTree;

        private readonly ILog logService;

        private readonly IDialogServiceAsync dialogService;

        private readonly IDialogService messageService;

        public BusyMediator BusyMediator { get; set; }

        private CancellationTokenSource currentLoadingToken;

        public RecordsStatisticsViewModel(IStatisticsService statisticsService,
                                        IRecordTypesTree recordTypeTree,
                                      IDialogServiceAsync dialogService,
                                      IDialogService messageService,
                                      ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (statisticsService == null)
            {
                throw new ArgumentNullException("statisticsService");
            }
            if (recordTypeTree == null)
            {
                throw new ArgumentNullException("recordTypeTree");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }

            this.statisticsService = statisticsService;
            this.recordTypeTree = recordTypeTree;
            this.logService = logService;
            this.dialogService = dialogService;
            this.messageService = messageService;
            loadResultCommand = new DelegateCommand(LoadResult);

            BusyMediator = new BusyMediator();
            FinSources = new ObservableCollectionEx<FieldValue>();
            Employees = new ObservableCollectionEx<FieldValue>();
            Source = new ObservableCollectionEx<DataGridRowDefinition>();
            Details = new ObservableCollectionEx<DataGridRowDefinition>();
        }
        
        void RowDef_RowExpanding(DataGridRowDefinition row)
        {
            RecursiveExpanding(row);
            Source = new ObservableCollectionEx<DataGridRowDefinition>(Source);
        }

        void RowDef_RowCollapsing(DataGridRowDefinition row)
        {
            RecursiveCollapsing(row);
            Source = new ObservableCollectionEx<DataGridRowDefinition>(Source);
        }

        private void RecursiveExpanding(DataGridRowDefinition row)
        {
            foreach (var child in Source.Where(x => x.ParentId == row.Id))
            {
                child.IsVisible = true;
                RecursiveExpanding(child);
            }
        }

        private void RecursiveCollapsing(DataGridRowDefinition row)
        {
            foreach (var child in Source.Where(x => x.ParentId == row.Id))
            {
                child.IsVisible = false;
                RecursiveCollapsing(child);
            }
        }

        internal async Task InitialLoadingAsync()
        {
            FinSources.Clear();
            Employees.Clear();
            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading data sources...");
            IDisposableQueryable<FinancingSource> finSourcesQuery = null;
            IDisposableQueryable<PersonStaff> employeesQuery = null;
            try
            {               
                finSourcesQuery = statisticsService.GetActualFinancingSources();
                var finSourcesSelectQuery = await finSourcesQuery.Where(x => x.Options != string.Empty).Select(x => new { x.Id, x.Name }).ToArrayAsync();
                FinSources.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- все ист. финансирования -" });
                FinSources.AddRange(finSourcesSelectQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                SelectedFinSourceId = SpecialValues.NonExistingId;

                employeesQuery = statisticsService.GetPersonStaffs();
                var employeesSelectQuery = await employeesQuery.Select(x => new { x.PersonId, PersonName = x.Person.ShortName }).ToArrayAsync();
                Employees.Add(new FieldValue { Value = SpecialValues.NonExistingId, Field = "- все сотрудники -" });
                Employees.AddRange(employeesSelectQuery.Select(x => new FieldValue { Value = x.PersonId, Field = x.PersonName }));
                SelectedEmployeeId = SpecialValues.NonExistingId;

                BeginDate = DateTime.Now.Date;
                EndDate = new DateTime(BeginDate.Year + 1, 1, 1);
                IsCompleted = true;
                IsAmbulatory = true;
                IsStationary = true;
                IsDayStationary = true;
                logService.InfoFormat("Data sources are successfully loaded");
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources");
                messageService.ShowError("Не удалось загрузить данные. ");
            }
            finally
            {
                if (finSourcesQuery != null)
                    finSourcesQuery.Dispose();
                if (employeesQuery != null)
                    employeesQuery.Dispose();
                BusyMediator.Deactivate();
            }
        }

        private async void LoadResult()
        {           
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;

            #region Get Data
            var recordsQuery = statisticsService.GetRecords(beginDate, endDate, selectedFinSourceId, isCompleted, isInProgress, isAmbulatory, isStationary, isDayStationary, selectedEmployeeId);
            RecordDTO[] recordsResult = await Task.Factory.StartNew(() =>
            {
                return recordsQuery.Select(x => new RecordDTO
                {
                    Id = x.Id,
                    RecordTypeId = x.RecordTypeId,
                    Name = x.RecordType.Name,
                    ContractId = x.RecordContractId,
                    FinancingSourceId = x.RecordContract.FinancingSourceId,
                    ContractName = (x.RecordContract.FinancingSource.Options.Contains(OptionValues.Pay) ? (x.RecordContract.Number.HasValue ? "№" + x.RecordContract.Number.Value + " - " : string.Empty) + x.RecordContract.ContractName + " - " : string.Empty) + x.RecordContract.FinancingSource.ShortName,
                    BeginDate = x.BeginDateTime,
                    EndDate = x.EndDateTime,
                    ActualDateTime = x.ActualDateTime,
                    MKB = x.MKB,
                    MKBId = x.MKBId.HasValue ? x.MKBId.Value : (int?)null,
                    RoomId = x.RoomId,
                    Room = x.Room.Name,
                    UrgentlyId = x.UrgentlyId,
                    UrgentlyName = x.Urgently.Name,
                    Code = x.RecordType.Code,
                    PaymentType = x.RecordContract.PaymentType.Name,
                    PersonBirthDate = x.Person.BirthDate,
                    PatientFIO = x.Person.FullName + ", " + x.Person.BirthDate.Year + " г.р.",
                    RelativeFIO = x.Person.PersonRelatives.Where(a => a.IsRepresentative).Select(a => a.Person1.FullName).FirstOrDefault(),
                    CardNumber = "А/К №" + x.Person.AmbNumberString,  // или И/Б
                    BranchName = "???",
                    Executor = "???", // из бригады
                    ExecutionPlaceId = x.ExecutionPlaceId,
                    ExecutionPlace = x.ExecutionPlace.Name,
                    ExecutionPlaceOption = x.ExecutionPlace.Options,
                    IsAnalyse = x.RecordType.IsAnalyse,
                })
                .OrderBy(x => x.ActualDateTime)
                .ToArray();
            }, token);

            if (IsPlanned)
            {
                var assignmentsQuery = statisticsService.GetAssignments(beginDate, endDate, selectedFinSourceId, isAmbulatory, isStationary, isDayStationary);
                RecordDTO[] assignmentsResult = await Task.Factory.StartNew(() =>
                {
                    return assignmentsQuery.Select(x => new RecordDTO
                    {
                        Id = x.Id,
                        RecordTypeId = x.RecordTypeId,
                        Name = x.RecordType.Name,
                        FinancingSourceId = x.FinancingSourceId,
                        ContractName = x.FinancingSource.ShortName,
                        AssignDateTime = x.AssignDateTime,
                        ActualDateTime = x.AssignDateTime,
                        RoomId = x.RoomId,
                        Room = x.Room.Name,
                        UrgentlyId = x.UrgentlyId,
                        UrgentlyName = x.Urgently.Name,
                        Code = x.RecordType.Code,
                        PersonBirthDate = x.Person.BirthDate,
                        PatientFIO = x.Person.FullName + ", " + x.Person.BirthDate.Year + " г.р.",
                        RelativeFIO = x.Person.PersonRelatives.Where(a => a.IsRepresentative).Select(a => a.Person1.FullName).FirstOrDefault(),
                        CardNumber = "А/К №" + x.Person.AmbNumberString,  // или И/Б
                        BranchName = "???",
                        Executor = "???", // из бригады
                        ExecutionPlaceId = x.ExecutionPlaceId,
                        ExecutionPlace = x.ExecutionPlace.Name,
                        ExecutionPlaceOption = x.ExecutionPlace.Options,
                        IsAnalyse = x.RecordType.IsAnalyse,
                    })
                    .OrderBy(x => x.ActualDateTime)
                    .ToArray();
                }, token);

                recordsResult = recordsResult.Union(assignmentsResult).ToArray();
            }
            #endregion

            #region Fill Grid

            Source.Clear();
            Details.Clear();
            
            foreach (RecordTypesTree rtc in recordTypeTree.GetAllChilds())
            {
                var records = recordsResult.Where(x => rtc.Childs.Contains(x.RecordTypeId)).OrderBy(x => x.ActualDateTime).ToList();
                if (records.Any())
                {
                    DataGridRowDefinition row = new DataGridRowDefinition()
                    {
                        Id = rtc.Id,
                        ParentId = rtc.ParentId,
                        Level = rtc.Level,
                        Children = rtc.Childs,
                        IsExpanded = false,
                        HasChildren = rtc.Childs.Count(x => x != rtc.Id) > 0,
                        IsVisible = !rtc.ParentId.HasValue,
                        Details = new List<DataGridRowDefinition>()
                    };

                    double totalCost = 0.0;
                    foreach (var record in records)
                    {
                        var recordCost = statisticsService.GetRecordTypeCost(record.RecordTypeId, record.FinancingSourceId, record.ActualDateTime, record.PersonBirthDate.AddYears(18) > record.ActualDateTime);
                        DataGridRowDefinition detailRow = new DataGridRowDefinition()
                        {
                            Id = record.Id,
                            IsVisible = true,
                            Cells = new ObservableCollectionEx<string>()
                            {
                                record.ActualDateTime.ToShortDateString(),
                                record.MKB,
                                record.CardNumber,
                                record.PatientFIO,
                                record.RelativeFIO,
                                record.ContractName,
                                record.ExecutionPlace,
                                record.BranchName,
                                record.Executor,
                                recordCost.ToString(),
                                record.PaymentType
                            }
                        };
                        row.Details.Add(detailRow);
                        totalCost += recordCost;
                    }

                    row.Cells = new ObservableCollectionEx<string>() 
                        { 
                            rtc.Name, 
                            rtc.Code, 
                            records.Count.ToSafeString(),
                            records.Count(x => x.ExecutionPlaceOption == OptionValues.Ambulatory).ToString(),
                            records.Count(x => x.ExecutionPlaceOption == OptionValues.Stationary).ToString(),
                            records.Count(x => x.ExecutionPlaceOption == OptionValues.DayStationary).ToString(),
                            totalCost.ToString()
                        };

                    Source.Add(row);
                }
            }

            DataGridRowDefinition.RowExpanding += new Action<DataGridRowDefinition>(RowDef_RowExpanding);
            DataGridRowDefinition.RowCollapsing += new Action<DataGridRowDefinition>(RowDef_RowCollapsing);
            #endregion            
        }      

        #region Properties

        private readonly DelegateCommand loadResultCommand;
        public ICommand LoadResultCommand { get { return loadResultCommand; } }

        ObservableCollectionEx<DataGridRowDefinition> source;
        public ObservableCollectionEx<DataGridRowDefinition> Source
        {
            get { return source; }
            set { SetProperty(ref source, value); }
        }

        DataGridRowDefinition selectedSource;
        public DataGridRowDefinition SelectedSource
        {
            get { return selectedSource; }
            set 
            {
                if (SetProperty(ref selectedSource, value) && value != null)
                {
                    Details = new ObservableCollectionEx<DataGridRowDefinition>(value.Details);
                }
            }
        }

        ObservableCollectionEx<DataGridRowDefinition> details;
        public ObservableCollectionEx<DataGridRowDefinition> Details
        {
            get { return details; }
            set { SetProperty(ref details, value); }
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
            set { SetProperty(ref selectedFinSourceId, value); }
        }

        private ObservableCollectionEx<FieldValue> employees;
        public ObservableCollectionEx<FieldValue> Employees
        {
            get { return employees; }
            set { SetProperty(ref employees, value); }
        }

        private int selectedEmployeeId;
        public int SelectedEmployeeId
        {
            get { return selectedEmployeeId; }
            set { SetProperty(ref selectedEmployeeId, value); }
        }

        private DateTime beginDate;
        public DateTime BeginDate
        {
            get { return beginDate; }
            set { SetProperty(ref beginDate, value); }
        }

        private DateTime endDate;
        public DateTime EndDate
        {
            get { return endDate; }
            set { SetProperty(ref endDate, value); }
        }

        private bool isCompleted;
        public bool IsCompleted
        {
            get { return isCompleted; }
            set { SetProperty(ref isCompleted, value); }
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set { SetProperty(ref isInProgress, value); }
        }

        private bool isPlanned;
        public bool IsPlanned
        {
            get { return isPlanned; }
            set { SetProperty(ref isPlanned, value); }
        }

        private bool isAmbulatory;
        public bool IsAmbulatory
        {
            get { return isAmbulatory; }
            set { SetProperty(ref isAmbulatory, value); }
        }

        private bool isStationary;
        public bool IsStationary
        {
            get { return isStationary; }
            set { SetProperty(ref isStationary, value); }
        }

        private bool isDayStationary;
        public bool IsDayStationary
        {
            get { return isDayStationary; }
            set { SetProperty(ref isDayStationary, value); }
        }  

        #endregion

        #region

        public void Dispose()
        {
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await InitialLoadingAsync();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        #endregion
    }
}
