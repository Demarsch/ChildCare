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
using Shared.Patient.Misc;

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

        private readonly MKBGroup unselectedMKBGroup;

        private bool isLoaded;

        public RecordsStatisticsViewModel(IStatisticsService statisticsService,
                                        IRecordTypesTree recordTypeTree,
                                      IDialogServiceAsync dialogService,
                                      IDialogService messageService,
                                      ILog logService,
                                      IAddressSuggestionProvider addressSuggestionProvider)
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
            if (addressSuggestionProvider == null)
            {
                throw new ArgumentNullException("addressSuggestionProvider");
            }
            this.statisticsService = statisticsService;
            this.recordTypeTree = recordTypeTree;
            this.logService = logService;
            this.dialogService = dialogService;
            this.messageService = messageService;
            loadResultCommand = new DelegateCommand(LoadResult);
            AddressSuggestionProvider = addressSuggestionProvider;
            BusyMediator = new BusyMediator();
            FinSources = new ObservableCollectionEx<FieldValue>();
            Employees = new ObservableCollectionEx<FieldValue>();
            Source = new ObservableCollectionEx<DataGridRowDefinition>();
            Details = new ObservableCollectionEx<DataGridRowDefinition>();
            unselectedMKBGroup = new MKBGroup { Name = "Все нозологические группы" };
            isLoaded = false;
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

        internal async Task InitialLoadingDataSources()
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

                await Task.Run(() => AddressSuggestionProvider.EnsureDataSourceLoadedAsync());

                var mkbGroupsSource = await Task.Run((Func<IEnumerable<MKBGroup>>)statisticsService.GetMKBGroups);
                MKBGroups = new[] { unselectedMKBGroup }.Concat(mkbGroupsSource).ToArray();
                SelectedMKBGroup = unselectedMKBGroup;

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
                isLoaded = true;
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

            //выполненные услуги
            var recordsQuery = statisticsService.GetRecords(beginDate, endDate, selectedFinSourceId, (region != null ? region.CodeOKATO : string.Empty), isCompleted, isInProgress, isAmbulatory, isStationary, isDayStationary, selectedEmployeeId);
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
                    Brigade = x.RecordMembers.Select(a => new BrigadeWrapper { EmployeeName = a.PersonStaff.Person.ShortName, StaffName = a.PersonStaff.Staff.ShortName }), // из бригады
                    ExecutionPlaceId = x.ExecutionPlaceId,
                    ExecutionPlace = x.ExecutionPlace.Name,
                    ExecutionPlaceOption = x.ExecutionPlace.Options,
                    IsAnalyse = x.RecordType.IsAnalyse,
                })
                .OrderBy(x => x.ActualDateTime)
                .ToArray();                
            }, token);

            if (recordsResult.Any() && !string.IsNullOrEmpty(selectedMKBGroup.MKBmax) && !string.IsNullOrEmpty(selectedMKBGroup.MKBmin))
                recordsResult = recordsResult.Where(x => statisticsService.MKBFilter(selectedMKBGroup.MKBmin + "-" + selectedMKBGroup.MKBmax, x.MKB)).ToArray();

            //случаи
            var visitsQuery = statisticsService.GetVisits(beginDate, endDate, selectedFinSourceId, (region != null ? region.CodeOKATO : string.Empty), isCompleted, isInProgress, isPlanned, isAmbulatory, isStationary, isDayStationary);
            VisitDTO[] visitsResult = await Task.Factory.StartNew(() =>
            {
                return visitsQuery.Select(x => new VisitDTO
                {
                    Id = x.Id,
                    VisitTemplateId = x.VisitTemplateId,
                    Name = x.VisitTemplate.Name,
                    ContractName = x.VisitTemplate.FinancingSource.ShortName,
                    BeginDate = x.BeginDateTime,
                    EndDate = x.EndDateTime,
                    MKB = x.MKB,
                    UrgentlyName = x.VisitTemplate.Urgently.Name,
                    PaymentType = x.VisitTemplate.RecordContract.PaymentType.Name,
                    PersonBirthDate = x.Person.BirthDate,
                    PatientFIO = x.Person.FullName + ", " + x.Person.BirthDate.Year + " г.р.",
                    RelativeFIO = x.Person.PersonRelatives.Where(a => a.IsRepresentative).Select(a => a.Person1.FullName).FirstOrDefault(),
                    CardNumber = "А/К №" + x.Person.AmbNumberString,  // или И/Б
                    ExecutionPlaceId = x.ExecutionPlaceId,
                    ExecutionPlace = x.ExecutionPlace.Name,
                    ExecutionPlaceOption = x.ExecutionPlace.Options,
                })
                .OrderBy(x => x.BeginDate)
                .ToArray();
            }, token);

            //назначенные услуги
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
                                record.Brigade != null && record.Brigade.Any() ? record.Brigade.Select(x => x.StaffName + ": " + x.EmployeeName).Aggregate((a,b) => a + "\r\n" + b) : "???",
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

            //Добавление случаев
            if (visitsResult.Any())
            {
                DataGridRowDefinition visitHeaderRow = new DataGridRowDefinition()
                {
                    Id = -1,
                    ParentId = null,
                    Level = 0,
                    Children = visitsResult.Select(x => x.Id).ToList(),
                    IsExpanded = false,
                    HasChildren = true,
                    IsVisible = true,
                    Details = visitsResult.Select(x => new DataGridRowDefinition()
                    {
                        Id = x.Id,
                        IsVisible = true,
                        Cells = new ObservableCollectionEx<string>() { x.BeginDate.ToShortDateString(), x.MKB, x.CardNumber, x.PatientFIO, x.RelativeFIO, x.ContractName, x.ExecutionPlace, "???", "???", "???", x.PaymentType }
                    }).ToList()
                };
                visitHeaderRow.Cells = new ObservableCollectionEx<string>()  { "Случаи обращения", string.Empty, visitsResult.Count().ToSafeString(), visitsResult.Count(x => x.ExecutionPlaceOption == OptionValues.Ambulatory).ToString(), visitsResult.Count(x => x.ExecutionPlaceOption == OptionValues.Stationary).ToString(), visitsResult.Count(x => x.ExecutionPlaceOption == OptionValues.DayStationary).ToString(), string.Empty };
                Source.Add(visitHeaderRow);

                foreach (var visitGroup in visitsResult.GroupBy(x => x.VisitTemplateId))
                {
                    DataGridRowDefinition row = new DataGridRowDefinition()
                    {
                        ParentId = -1,
                        Level = 1,
                        Children = visitGroup.Select(x => x.Id).ToList(),
                        IsExpanded = false,
                        HasChildren = false,
                        IsVisible = false,
                        Details = visitGroup.Select(x => new DataGridRowDefinition()
                        {
                            Id = x.Id,
                            IsVisible = true,
                            Cells = new ObservableCollectionEx<string>() { x.BeginDate.ToShortDateString(), x.MKB, x.CardNumber, x.PatientFIO, x.RelativeFIO, x.ContractName, x.ExecutionPlace, "???", "???", "???", x.PaymentType }
                        }).ToList()
                    };
                    row.Cells = new ObservableCollectionEx<string>() { visitGroup.First().Name, string.Empty, visitGroup.Count().ToSafeString(), visitGroup.Count(x => x.ExecutionPlaceOption == OptionValues.Ambulatory).ToString(), visitGroup.Count(x => x.ExecutionPlaceOption == OptionValues.Stationary).ToString(), visitGroup.Count(x => x.ExecutionPlaceOption == OptionValues.DayStationary).ToString(), string.Empty };
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
        public IAddressSuggestionProvider AddressSuggestionProvider { get; private set; }

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

        private Okato region;
        public Okato Region
        {
            get { return region; }
            set
            {
                if (SetProperty(ref region, value))
                {
                    AddressSuggestionProvider.SelectedRegion = value;
                };
            }
        }

        private IEnumerable<MKBGroup> mkbGroups;
        public IEnumerable<MKBGroup> MKBGroups
        {
            get { return mkbGroups; }
            private set
            {
                if (SetProperty(ref mkbGroups, value))
                {
                    SelectedMKBGroup = unselectedMKBGroup;
                }
            }
        }

        private MKBGroup selectedMKBGroup;
        public MKBGroup SelectedMKBGroup
        {
            get { return selectedMKBGroup; }
            set
            {
                value = value ?? unselectedMKBGroup;
                SetProperty(ref selectedMKBGroup, value);
            }
        }

        #endregion

        #region

        public void Dispose()
        {
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!isLoaded)
                await InitialLoadingDataSources();
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
