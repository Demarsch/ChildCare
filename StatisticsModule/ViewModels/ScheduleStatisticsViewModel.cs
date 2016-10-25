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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace StatisticsModule.ViewModels
{
    public class ScheduleStatisticsViewModel : BindableBase, IDisposable, INavigationAware
    {
        private readonly IStatisticsService statisticsService;

        private readonly ILog logService;
        
        private readonly IDialogService messageService;

        public BusyMediator BusyMediator { get; set; }

        private CancellationTokenSource currentLoadingToken;

        private bool dataSourcesLoaded = false;

        public ScheduleStatisticsViewModel(IStatisticsService statisticsService, IDialogService messageService, ILog logService)
        {
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (statisticsService == null)
            {
                throw new ArgumentNullException("statisticsService");
            }           
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            this.statisticsService = statisticsService;
            this.logService = logService;
            this.messageService = messageService;
            loadResultCommand = new DelegateCommand(LoadResult);
            BusyMediator = new BusyMediator();
            FinSources = new ObservableCollectionEx<FieldValue>();
            Employees = new ObservableCollectionEx<FieldValue>();            
            Source = new ObservableCollectionEx<ScheduleStatisticsRow>();
            Details = new ObservableCollectionEx<RecordDTO>();
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

                BeginDate = DateTime.Now.Date;
                EndDate = new DateTime(BeginDate.Year + 1, 1, 1);
                logService.InfoFormat("Data sources are successfully loaded");
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources");
                messageService.ShowError("Не удалось загрузить данные. ");
            }
            finally
            {
                dataSourcesLoaded = true;
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

            var assignmentsQuery = statisticsService.GetAssignments(beginDate, endDate, selectedEmployeeId);
            RecordDTO[] assignmentsResult = await Task.Factory.StartNew(() =>
            {
                return assignmentsQuery.Select(x => new RecordDTO
                {
                    Id = x.Id,
                    RecordTypeId = x.RecordTypeId,
                    Name = x.RecordType.Name,
                    FinancingSourceId = x.FinancingSourceId,                    
                    FinancingSourceName = x.FinancingSource.ShortName,
                    AssignDateTime = x.AssignDateTime,   
                    UrgentlyId = x.UrgentlyId,
                    UrgentlyName = x.Urgently.Name,                   
                    PatientFIO = x.Person.FullName + ", " + x.Person.BirthDate.Year + " г.р.",
                    CardNumber = "А/К №" + x.Person.AmbNumberString,  // или И/Б 
                    AssignByRegistrator = x.User.Person.PersonStaffs.Any(a => a.Staff.Options.Contains(OptionValues.Registrator))
                })
                .OrderBy(x => x.AssignDateTime)
                .ToArray();
            }, token);

            Source.Clear();
            Columns = new ObservableCollectionEx<DataGridColumn>();

            var registratorCells = new List<ScheduleStatisticsCell>();
            registratorCells.Add(new ScheduleStatisticsCell() { ColumnName = "Записано", CellValue = "Регистратурой" });
            foreach (var group in assignmentsResult.GroupBy(x => x.FinancingSourceName))
                registratorCells.Add(new ScheduleStatisticsCell() { ColumnName = group.Key, CellValue = assignmentsResult.Count(x => x.AssignByRegistrator && x.FinancingSourceId == group.First().FinancingSourceId).ToString(), Details = assignmentsResult.Where(x => x.AssignByRegistrator && x.FinancingSourceId == group.First().FinancingSourceId).ToArray() });

            var doctorCells = new List<ScheduleStatisticsCell>();
            doctorCells.Add(new ScheduleStatisticsCell() { ColumnName = "Записано", CellValue = "Врачами с приема" });
            foreach (var group in assignmentsResult.GroupBy(x => x.FinancingSourceName))
                doctorCells.Add(new ScheduleStatisticsCell() { ColumnName = group.Key, CellValue = assignmentsResult.Count(x => !x.AssignByRegistrator && x.FinancingSourceId == group.First().FinancingSourceId).ToString(), Details = assignmentsResult.Where(x => !x.AssignByRegistrator && x.FinancingSourceId == group.First().FinancingSourceId).ToArray() });
            
            var records = new ScheduleStatisticsRow[] 
            {
                new ScheduleStatisticsRow(registratorCells.ToArray()),
                new ScheduleStatisticsRow(doctorCells.ToArray())
            };

            var columns = records.First().Properties.Select((x, i) => new { ColumnName = x.ColumnName, CellValue = x.CellValue, Details = x.Details, Index = i }).ToArray();
            foreach (var column in columns)
            {
                var binding = new Binding(string.Format("Properties[{0}].CellValue", column.Index));
                Columns.Add(new DataGridTextColumn() { Header = column.ColumnName, Binding = binding, IsReadOnly = true, FontWeight = (column.Index == 0 ? FontWeights.SemiBold : FontWeights.Normal) });
            }           
            Source.AddRange(records);
            Details.Clear();
        }      

        #region Properties

        private readonly DelegateCommand loadResultCommand;
        public ICommand LoadResultCommand { get { return loadResultCommand; } }

        public ObservableCollectionEx<RecordDTO> Details { get; set; }

        private ObservableCollectionEx<ScheduleStatisticsRow> source;
        public ObservableCollectionEx<ScheduleStatisticsRow> Source
        {
            get { return source; }
            set { SetProperty(ref source, value); }
        }

        private DataGridCellInfo selectedCell;
        public DataGridCellInfo SelectedCell
        {
            get { return selectedCell; }
            set
            { 
                if (SetProperty(ref selectedCell, value) && value.Item != null && value.Column != null)
                {
                    Details.Clear();
                    if (value.Column.DisplayIndex > 0)
                        Details.AddRange((value.Item as ScheduleStatisticsRow).Properties[value.Column.DisplayIndex].Details as RecordDTO[]);
                }
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

        private ObservableCollectionEx<DataGridColumn> columns;
        public ObservableCollectionEx<DataGridColumn> Columns
        {
            get { return columns; }
            set { SetProperty(ref columns, value); }
        }
        #endregion

        #region

        public void Dispose()
        {
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!dataSourcesLoaded)
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
