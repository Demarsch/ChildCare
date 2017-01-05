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
    public class RoomCapacityStatisticsViewModel : BindableBase, IDisposable, INavigationAware
    {
        private readonly IStatisticsService statisticsService;

        private readonly IRecordTypesTree recordTypeTree;

        private readonly ILog logService;

        private readonly IDialogServiceAsync dialogService;

        private readonly IDialogService messageService;

        public BusyMediator BusyMediator { get; set; }

        private CancellationTokenSource currentLoadingToken;
        
        private bool isLoaded;

        public RoomCapacityStatisticsViewModel(IStatisticsService statisticsService,
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
            Source = new ObservableCollectionEx<DataGridRowDefinition>();
            Details = new ObservableCollectionEx<DataGridRowDefinition>();
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
            BusyMediator.Activate("Загрузка данных...");
            logService.Info("Loading data sources...");
            IDisposableQueryable<FinancingSource> finSourcesQuery = null;
            IDisposableQueryable<PersonStaff> employeesQuery = null;
            try
            {           
                BeginDate = DateTime.Now.Date;
                EndDate = new DateTime(BeginDate.Year + 1, 1, 1);               
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
                        
            //назначенные услуги            
            var assignmentsQuery = statisticsService.GetAssignments(beginDate, endDate, -1, isAmbulatory, isStationary, isDayStationary);
            RoomScheduleDTO[] result = await Task.Factory.StartNew(() =>
            {
                return assignmentsQuery.Select(x => new RoomScheduleDTO
                {
                    Id = x.Id,
                    RecordTypeId = x.RecordTypeId,
                    Name = x.RecordType.Name,                    
                    AssignDateTime = x.AssignDateTime,
                    RoomId = x.RoomId,
                    Room = x.Room.Name,                    
                    PatientFIO = x.Person.FullName + ", " + x.Person.BirthDate.Year + " г.р.",                    
                    CardNumber = "А/К №" + x.Person.AmbNumberString,  // или И/Б
                    ExecutionPlaceId = x.ExecutionPlaceId,
                    ExecutionPlace = x.ExecutionPlace.Name
                })
                .OrderBy(x => x.AssignDateTime)
                .ToArray();
            }, token);
                        
            #endregion

            #region Fill Grid

            Source.Clear();
            Details.Clear();
            
            foreach (RecordTypesTree rtc in recordTypeTree.GetAllChilds())
            {
                var records = result.Where(x => rtc.Childs.Contains(x.RecordTypeId)).OrderBy(x => x.AssignDateTime).ToList();
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
                        Cells = new ObservableCollectionEx<string>() 
                        { 
                            rtc.Name, 
                            //records.Count.ToSafeString(),  Всего мест
                            //records.Count(x => x.ExecutionPlaceOption == OptionValues.Ambulatory).ToString(),  Записано
                            //records.Count(x => x.ExecutionPlaceOption == OptionValues.Stationary).ToString(),  Свободно
                        },
                        Details = records.Select(x => new DataGridRowDefinition()
                                                {
                                                    Id = x.Id,
                                                    IsVisible = true,
                                                    Cells = new ObservableCollectionEx<string>()
                                                    {
                                                        x.AssignDateTime.ToShortDateString(),                                
                                                        x.CardNumber,
                                                        x.PatientFIO,
                                                        x.ExecutionPlace
                                                    }
                                                }).ToList()                       
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
