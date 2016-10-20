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
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Regions;
using System.Windows.Media;
using Core.Notification;
using System.Data.Entity;
using Core.Misc;
using PolyclinicModule.Services;
using PolyclinicModule.DTO;
using Core.Wpf.Events;
using Shared.PatientRecords.Events;
using Shell.Shared;
using Shared.PatientRecords.ViewModels;

namespace PolyclinicModule.ViewModels
{
    public class PolyclinicPersonListViewModel : BindableBase, INavigationAware
    {
        #region Fields
     
        private readonly IPolyclinicService polyclinicService;
        
        private readonly ILog logService;

        private readonly IDialogServiceAsync dialogService;

        private readonly IDialogService messageService;

        private readonly IUserService userService;

        private readonly IEventAggregator eventAggregator;

        private readonly INotificationService notificationService;
        
        private CancellationTokenSource currentLoadingToken;

        private TaskCompletionSource<bool> completionTaskSource;

        private readonly IRegionManager regionManager;

        private readonly IViewNameResolver viewNameResolver;

        private bool sourcesLoaded;

        #endregion

        #region  Constructors

        public PolyclinicPersonListViewModel(IRegionManager regionManager, IViewNameResolver viewNameResolver, ILog logService, IDialogServiceAsync dialogService, IDialogService messageService,
                                        IUserService userService, INotificationService notificationService, IEventAggregator eventAggregator, IPolyclinicService polyclinicService)
        {            
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            if (regionManager == null)
            {
                throw new ArgumentNullException("regionManager");
            }
            if (viewNameResolver == null)
            {
                throw new ArgumentNullException("viewNameResolver");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (messageService == null)
            {
                throw new ArgumentNullException("messageService");
            }
            if (userService == null)
            {
                throw new ArgumentNullException("userService");
            }
            if (notificationService == null)
            {
                throw new ArgumentNullException("notificationService");
            }
            if (polyclinicService == null)
            {
                throw new ArgumentNullException("polyclinicService");
            }
            this.viewNameResolver = viewNameResolver;
            this.regionManager = regionManager;
            this.dialogService = dialogService;
            this.userService = userService;
            this.messageService = messageService;
            this.eventAggregator = eventAggregator;
            this.logService = logService;
            this.notificationService = notificationService;
            this.polyclinicService = polyclinicService;           

            BusyMediator = new BusyMediator();
            Rooms = new ObservableCollectionEx<FieldValue>();
            Source = new ObservableCollectionEx<PolyclinicPersonListItemViewModel>();
            sourcesLoaded = false;
        }

        #endregion

        #region Properties
        public BusyMediator BusyMediator { get; set; }

        ObservableCollectionEx<PolyclinicPersonListItemViewModel> source;
        public ObservableCollectionEx<PolyclinicPersonListItemViewModel> Source
        {
            get { return source; }
            set { SetProperty(ref source, value); }
        }

        int targetPersonId = SpecialValues.NonExistingId;
        int? targetAssignmentId = SpecialValues.NonExistingId;
        int? targetRecordId = SpecialValues.NonExistingId;

        PolyclinicPersonListItemViewModel selectedSource;
        public PolyclinicPersonListItemViewModel SelectedSource
        {
            get { return selectedSource; }
            set
            {
                if (SetProperty(ref selectedSource, value))
                {
                    if (value != null && !SpecialValues.IsNewOrNonExisting(value.PersonId) && (value.PersonId != targetPersonId || value.AssignmentId != targetAssignmentId || value.RecordId != targetRecordId))
                    {                        
                        targetPersonId = value.PersonId;
                        targetAssignmentId = value.AssignmentId;
                        targetRecordId = value.RecordId;
                        var navigationParameters = new NavigationParameters { { "PatientId", targetPersonId } };
                        regionManager.RequestNavigate(RegionNames.ModuleContent, viewNameResolver.Resolve<PersonRecordsViewModel>(), navigationParameters);                        
                        eventAggregator.GetEvent<PolyclinicPersonListChangedEvent>().Publish(new object[] { targetPersonId, targetAssignmentId, targetRecordId });
                    }
                }
            }
        }

        private ObservableCollectionEx<FieldValue> rooms;
        public ObservableCollectionEx<FieldValue> Rooms
        {
            get { return rooms; }
            set { SetProperty(ref rooms, value); }
        }

        private int selectedRoomId;
        public int SelectedRoomId
        {
            get { return selectedRoomId; }
            set
            {
                if (SetProperty(ref selectedRoomId, value) && !SpecialValues.IsNewOrNonExisting(value) && SelectedDate != null)
                {
                    LoadPersonList();
                }
            }
        }

        private DateTime selectedDate;
        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                if (SetProperty(ref selectedDate, value) && value != null && !SpecialValues.IsNewOrNonExisting(SelectedRoomId))
                {
                    LoadPersonList();
                }
            }
        }

        #endregion

        #region Methods

        internal async Task InitialLoadingAsync()
        {           
            Rooms.Clear();
            SelectedDate = DateTime.MinValue;
            SelectedRoomId = -1;
            BusyMediator.Activate("Загрузка пациентов...");
            logService.Info("Loading ambulatory person list data sources...");
            IDisposableQueryable<Room> roomQuery = null;
            try
            {
                SelectedDate = DateTime.Now.Date;

                roomQuery = polyclinicService.GetPolyclinicRooms();
                var roomSelectQuery = await roomQuery.Select(x => new { x.Id, x.Name }).ToArrayAsync();
                Rooms.AddRange(roomSelectQuery.Select(x => new FieldValue { Value = x.Id, Field = x.Name }));
                if (Rooms.Any())
                {
                    SelectedRoomId = Rooms.First().Value;
                    logService.InfoFormat("Data sources are successfully loaded");
                }
            }
            catch (Exception ex)
            {
                logService.ErrorFormatEx(ex, "Failed to load data sources");
                messageService.ShowError("Не удалось загрузить данные. ");
            }
            finally
            {
                if (roomQuery != null)
                    roomQuery.Dispose();
                BusyMediator.Deactivate();
                sourcesLoaded = true;
            }
        }

        private async void LoadPersonList()
        {            
            logService.Info("Loading ambulatory person list...");
            if (currentLoadingToken != null)
            {
                currentLoadingToken.Cancel();
                currentLoadingToken.Dispose();
            }
            currentLoadingToken = new CancellationTokenSource();
            var token = currentLoadingToken.Token;

            UnsubscribeAssignmentsRecordsChanges();

            #region Get Data
            var assignmentsQuery = polyclinicService.GetAssignments(selectedDate, selectedRoomId);
            var notCompletedRecordsQuery = polyclinicService.GetRecords(selectedDate, selectedRoomId);
            RecordDTO[] assignmentsResult = await Task.Factory.StartNew(() =>
            {
                return assignmentsQuery.Select(x => new RecordDTO
                {
                    AssignmentId = x.Id,
                    PersonId = x.PersonId,
                    PatientFIO = x.Person.ShortName,
                    PersonBirthYear = x.Person.BirthDate.Year + " г.р.",
                    ResultDate = x.AssignDateTime,
                    IsCompleted = false,
                }).ToArray();
            }, token);
           
            var recordsQuery = polyclinicService.GetRecords(selectedDate, selectedRoomId);
            RecordDTO[] recordsResult = await Task.Factory.StartNew(() =>
            {
                return recordsQuery.Select(x => new RecordDTO
                {
                    RecordId = x.Id,
                    PersonId = x.PersonId,
                    PatientFIO = x.Person.ShortName,
                    PersonBirthYear = x.Person.BirthDate.Year + " г.р.",
                    ResultDate = x.BeginDateTime,
                    IsCompleted = x.IsCompleted,
                })
                .ToArray();
            }, token);

            recordsResult = recordsResult.Union(assignmentsResult).ToArray();

            #endregion

            #region Fill Grid

            Source.Clear();

            int parentId = -1;
            int index = 0;
            foreach (var group in recordsResult.OrderBy(x => x.IsCompleted).GroupBy(x => x.IsCompleted))
            {
                PolyclinicPersonListItemViewModel parentRow = new PolyclinicPersonListItemViewModel()
                {
                    Id = parentId,
                    ParentId = null,
                    Level = 0,
                    IsExpanded = true,
                    IsVisible = true,
                    HasChildren = group.Any(),
                    Index = index++
                };
                parentRow.Cells = new ObservableCollectionEx<string>() { string.Empty, (!group.Key ? "Назначенные" : "Выполненные") + " (" + group.Count()  + " услуги)", string.Empty };
                Source.Add(parentRow);

                foreach (var item in group.OrderBy(x => x.ResultDate))
                {
                    PolyclinicPersonListItemViewModel row = new PolyclinicPersonListItemViewModel()
                    {
                        Id = item.AssignmentId.HasValue ? item.AssignmentId.Value : item.RecordId.Value,
                        ParentId = parentId,
                        Level = 1,
                        IsExpanded = true,
                        IsVisible = true,
                        AssignmentId = item.AssignmentId,
                        RecordId = item.RecordId,
                        PersonId = item.PersonId,
                        Date = item.ResultDate,
                        Index = index++
                    };
                    row.Cells = new ObservableCollectionEx<string>() { item.ResultDate.ToShortTimeString() + " ", item.PatientFIO, item.PersonBirthYear };
                    Source.Add(row);
                }
                parentId++;                
            }
            PolyclinicPersonListItemViewModel.RowExpanding += new Action<PolyclinicPersonListItemViewModel>(RowDef_RowExpanding);
            PolyclinicPersonListItemViewModel.RowCollapsing += new Action<PolyclinicPersonListItemViewModel>(RowDef_RowCollapsing);

            #endregion

            await SubscribeAssignmentsRecordsChanges();
        }

        private void UnsubscribeAssignmentsRecordsChanges()
        {
            if (assignmentsChangeSubscription != null)
            {
                assignmentsChangeSubscription.Notified -= OnAssignmentsNotificationRecievedAsync;
                assignmentsChangeSubscription.Dispose();
            }
            if (recordsChangeSubscription != null)
            {
                recordsChangeSubscription.Notified -= OnRecordsNotificationRecievedAsync;
                recordsChangeSubscription.Dispose();
            }
        }

        private async Task<bool> SubscribeAssignmentsRecordsChanges()
        {
            if (completionTaskSource != null)
                return await completionTaskSource.Task;
            completionTaskSource = new TaskCompletionSource<bool>();

            assignmentsChangeSubscription = notificationService.Subscribe<Assignment>(x => DbFunctions.TruncateTime(SelectedDate) == DbFunctions.TruncateTime(x.AssignDateTime) && x.RoomId == SelectedRoomId && !x.RecordId.HasValue);
            recordsChangeSubscription = notificationService.Subscribe<Record>(x => DbFunctions.TruncateTime(SelectedDate) == DbFunctions.TruncateTime(x.BeginDateTime) && x.RoomId == SelectedRoomId);

            if (assignmentsChangeSubscription != null)
                assignmentsChangeSubscription.Notified += OnAssignmentsNotificationRecievedAsync;

            if (recordsChangeSubscription != null)
                recordsChangeSubscription.Notified += OnRecordsNotificationRecievedAsync;

            completionTaskSource.SetResult(true);
            return true;
        }

        private async void OnAssignmentsNotificationRecievedAsync(object sender, NotificationEventArgs<Assignment> e)
        {
            if (e.IsDelete)
                await RemoveRecordAsync(e.OldItem.Id, true);                           
            if (e.IsUpdate)
                await UpdateRecordAsync(e.NewItem.Id, true);
            if (e.IsCreate)
                await CreateRecordAsync(e.NewItem.Id, true, e.NewItem.PersonId, e.NewItem.AssignDateTime);            
        }
        
        private async void OnRecordsNotificationRecievedAsync(object sender, NotificationEventArgs<Record> e)
        {
            if (e.IsDelete)
                await RemoveRecordAsync(e.OldItem.Id, false); 
            if (e.IsUpdate)
                await UpdateRecordAsync(e.NewItem.Id, false);
            if (e.IsCreate)
                await CreateRecordAsync(e.NewItem.Id, false, e.NewItem.PersonId, e.NewItem.BeginDateTime);
        }

        private async Task<bool> CreateRecordAsync(int id, bool isAssignment, int personId, DateTime date)
        {
            if (completionTaskSource != null)
                return await completionTaskSource.Task;
            completionTaskSource = new TaskCompletionSource<bool>();
            
            if (!Source.Any(x => x.ParentId == (isAssignment ? -1 : 0)))
            {
                PolyclinicPersonListItemViewModel parentRow = new PolyclinicPersonListItemViewModel()
                {
                    Id = isAssignment ? -1 : 0,
                    ParentId = null,
                    Level = 0,
                    IsExpanded = true,
                    IsVisible = true,
                    HasChildren = true,
                    Index = isAssignment ? 0 : source.Count,
                };
                parentRow.Cells = new ObservableCollectionEx<string>() { string.Empty, (isAssignment ? "Назначенные" : "Выполненные") + " (1 услуга)", string.Empty };
                Source.Insert(parentRow.Index, parentRow);
            }

            PolyclinicPersonListItemViewModel row = new PolyclinicPersonListItemViewModel()
            {            
                Id = id,
                ParentId = isAssignment ? -1 : 0,
                Level = 1,
                IsExpanded = true,
                IsVisible = true,
                AssignmentId = isAssignment ? id : (int?)null,
                RecordId = !isAssignment ? id : (int?)null,
                PersonId = personId,
                Date = date,
                Index = GetInsertRowIndex(isAssignment, date)
            };            
            Source.Insert(row.Index, row);
            
            completionTaskSource.SetResult(true);
            return true;
        }

        private int GetInsertRowIndex(bool isAssignment, DateTime date)
        {
            var subSource = source.Where(x => x.ParentId == (isAssignment ? -1 : 0));
            if (!subSource.Any())
                return (isAssignment ? 1 : source.Count);
            else
            {
                var item = subSource.FirstOrDefault(x => x.Date > date);
                if (item != null)
                    return item.Index;
                return subSource.OrderByDescending(x => x.Date).First().Index;
            }
        }

        private async Task<bool> UpdateRecordAsync(int id, bool isAssignment)
        {
            if (completionTaskSource != null)
                return await completionTaskSource.Task;
            completionTaskSource = new TaskCompletionSource<bool>();

            PolyclinicPersonListItemViewModel row = Source.FirstOrDefault(x => isAssignment ? x.AssignmentId == id : x.RecordId == id);
            if (isAssignment)
            {
                var assignment = polyclinicService.GetAssignmentById(id).First();
                row.Cells = new ObservableCollectionEx<string>() { assignment.AssignDateTime.ToShortTimeString() + " ", assignment.Person.ShortName, assignment.Person.BirthYear };
            }
            else
            {
                var record = polyclinicService.GetRecordById(id).First();
                row.Cells = new ObservableCollectionEx<string>() { record.BeginDateTime.ToShortTimeString() + " ", record.Person.ShortName, record.Person.BirthYear };
            }
            completionTaskSource.SetResult(true);
            return true;
        }

        private async Task<bool> RemoveRecordAsync(int id, bool isAssignment)
        {
            if (completionTaskSource != null)
                return await completionTaskSource.Task;
            completionTaskSource = new TaskCompletionSource<bool>();

            PolyclinicPersonListItemViewModel row = Source.FirstOrDefault(x => isAssignment ? x.AssignmentId == id : x.RecordId == id);
            int parentId = row.ParentId.Value;
            Source.Remove(row);

            if (Source.Any(x => isAssignment ? x.AssignmentId.HasValue : x.RecordId.HasValue))
                Source.First(x => x.Id == parentId).Cells = new ObservableCollectionEx<string>() { string.Empty, (isAssignment ? "Назначенные" : "Выполненные") + " (" + Source.Count(x => isAssignment ? x.AssignmentId.HasValue : x.RecordId.HasValue) + " услуги)", string.Empty };
            else
                Source.Remove(Source.First(x => x.Id == parentId));
            
            completionTaskSource.SetResult(true);
            return true;
        }

        #endregion        

        private INotificationServiceSubscription<Assignment> assignmentsChangeSubscription;
        private INotificationServiceSubscription<Record> recordsChangeSubscription;

        #region Events

        void RowDef_RowExpanding(PolyclinicPersonListItemViewModel row)
        {
            RecursiveExpanding(row);
            Source = new ObservableCollectionEx<PolyclinicPersonListItemViewModel>(Source);
        }

        void RowDef_RowCollapsing(PolyclinicPersonListItemViewModel row)
        {
            RecursiveCollapsing(row);
            Source = new ObservableCollectionEx<PolyclinicPersonListItemViewModel>(Source);
        }

        private void RecursiveExpanding(PolyclinicPersonListItemViewModel row)
        {
            foreach (var child in Source.Where(x => x.ParentId == row.Id))
            {
                child.IsVisible = true;
                RecursiveExpanding(child);
            }
        }

        private void RecursiveCollapsing(PolyclinicPersonListItemViewModel row)
        {
            foreach (var child in Source.Where(x => x.ParentId == row.Id))
            {
                child.IsVisible = false;
                RecursiveCollapsing(child);
            }
        }

        #endregion

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (sourcesLoaded && SelectedSource != null)
                eventAggregator.GetEvent<PolyclinicPersonListChangedEvent>().Publish(new object[] { SelectedSource.PersonId, SelectedSource.AssignmentId, SelectedSource.RecordId });
            else
                await InitialLoadingAsync();
        }
    }
}
