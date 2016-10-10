using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Navigation;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Extensions;
using Core.Notification;
using Core.Services;
using Core.Wpf.Events;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using Core.Wpf.ViewModels;
using log4net;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using ScheduleModule.DTO;
using ScheduleModule.Exceptions;
using ScheduleModule.Misc;
using ScheduleModule.Services;
using Shared.Patient.ViewModels;
using Shared.Schedule.Events;
using System.Windows.Input;

namespace ScheduleModule.ViewModels
{
    public class MultiAssignsContentViewModel : BindableBase, INavigationAware, IDisposable
    {
        private readonly ILog log;

        private readonly IScheduleService scheduleService;

        private readonly RoomViewModel unseletedRoom;

        private readonly RecordType unselectedRecordType;

        private readonly ICacheService cacheService;

        private readonly IEnvironment environment;

        private readonly IDialogServiceAsync dialogService;

        private readonly INotificationService notificationService;

        private readonly ISecurityService securityService;

        private readonly IEventAggregator eventAggregator;

        private readonly Func<MultiAssignRecordTypeViewModel> multiAssignRecordTypeViewModelFactory;

        private readonly int daysCount = 7;

        public MultiAssignsContentViewModel(OverlayAssignmentCollectionViewModel overlayAssignmentCollectionViewModel,
                                        PatientAssignmentListViewModel patientAssignmentListViewModel,
                                        Func<MultiAssignRecordTypeViewModel> multiAssignRecordTypeViewModelFactory,
                                        INotificationService notificationService,
                                        IScheduleService scheduleService,
                                        ILog log,
                                        ICacheService cacheService,
                                        IEnvironment environment,
                                        IDialogServiceAsync dialogService,
                                        ISecurityService securityService,
                                        IEventAggregator eventAggregator)
        {
            if (notificationService == null)
            {
                throw new ArgumentNullException("notificationService");
            }
            if (scheduleService == null)
            {
                throw new ArgumentNullException("scheduleService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (securityService == null)
            {
                throw new ArgumentNullException("securityService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if (overlayAssignmentCollectionViewModel == null)
            {
                throw new ArgumentNullException("overlayAssignmentCollectionViewModel");
            }
            if (patientAssignmentListViewModel == null)
            {
                throw new ArgumentNullException("patientAssignmentListViewModel");
            }
            if (multiAssignRecordTypeViewModelFactory == null)
            {
                throw new ArgumentNullException("multiAssignRecordTypeViewModelFactory");
            }
            this.multiAssignRecordTypeViewModelFactory = multiAssignRecordTypeViewModelFactory;
            OverlayAssignmentCollectionViewModel = overlayAssignmentCollectionViewModel;
            this.notificationService = notificationService;
            this.securityService = securityService;
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            this.environment = environment;
            this.cacheService = cacheService;
            this.log = log;
            this.scheduleService = scheduleService;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            SelectedTimeIntervalWrapper = new SelectedTimeIntervalWrapper();
            SelectedRecordTypes = new ObservableCollectionEx<MultiAssignRecordTypeViewModel>();
            addRecordTypeCommand = new DelegateCommand(AddRecordType);
            selectedDate = overlayAssignmentCollectionViewModel.CurrentDate = DateTime.Today;
            overlayAssignmentCollectionViewModel.CurrentDateRoomsOpenTime = selectedDate.AddHours(8.0);
            overlayAssignmentCollectionViewModel.CurrentDateRoomsOpenTime = selectedDate.AddHours(17.0);
            ChangeDateCommand = new DelegateCommand<int?>(ChangeDate);
            unseletedRoom = new RoomViewModel(new Room { Name = "Выберите кабинет" }, scheduleService);
            unselectedRecordType = new RecordType { Name = "Выберите услугу", Assignable = true };
            initialLoadingCommandWrapper = new CommandWrapper { Command = new DelegateCommand(async () => await InitialLoadingAsync()) };
            loadAssignmentsCommandWrapper = new CommandWrapper { Command = new DelegateCommand(async () => await LoadAssignmentsAsync(selectedDate)) };
            SubscribeToEvents();
            firstTimeLoad = true;
        }

        private async void AddRecordType()
        {
            var newItem = multiAssignRecordTypeViewModelFactory();
            await newItem.Initialize(SelectedRecordType, Dates);
            newItem.SelectedTimesChanged += selectedRecordType_SelectedTimesChanged;
            SelectedRecordTypes.Add(newItem);
        }



        private void selectedRecordType_SelectedTimesChanged(object sender, SelectedTimesEventArg e)
        {
            if (e.IsAdded)
                SelectedTimeIntervalWrapper.AddTimeInterval(e.TimeInterval, e.Date);
            else
                SelectedTimeIntervalWrapper.RemoveTimeInterval(e.TimeInterval, e.Date);
        }

        private async void DateChanged()
        {
            List<Task> tasks = new List<Task>();
            var recType = SelectedRecordType;
            var dates = Dates;
            foreach (var selectedRecordType in SelectedRecordTypes)
            {
                selectedRecordType.SelectedTimesChanged -= selectedRecordType_SelectedTimesChanged;
                await selectedRecordType.Initialize(selectedRecordType.RecordType, dates);
                selectedRecordType.SelectedTimesChanged += selectedRecordType_SelectedTimesChanged;
                //var task = selectedRecordType.Initialize(selectedRecordType.RecordType, dates);
                //task.Start();
                //tasks.Add(task);
            }
            //await Task.WhenAll(tasks);
        }

        private readonly CommandWrapper loadAssignmentsCommandWrapper;

        private void SubscribeToEvents()
        {
            eventAggregator.GetEvent<ScheduleChangedEvent>().Subscribe(OnScheduleChanged);
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Subscribe(OnPatientSelected);
        }

        private void UnsubscribeFromEvents()
        {
            eventAggregator.GetEvent<ScheduleChangedEvent>().Unsubscribe(OnScheduleChanged);
            eventAggregator.GetEvent<SelectionChangedEvent<Person>>().Unsubscribe(OnPatientSelected);
        }

        private async void OnScheduleChanged(object obj)
        {
            await LoadAssignmentsAsync(selectedDate);
        }

        private void OnPatientSelected(int patientId)
        {
            CurrentPatientId = patientId;
        }

        public BusyMediator BusyMediator { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        public OverlayAssignmentCollectionViewModel OverlayAssignmentCollectionViewModel { get; private set; }

        private INotificationServiceSubscription<Assignment> assignmentChangeSubscription;

        public ObservableCollectionEx<MultiAssignRecordTypeViewModel> SelectedRecordTypes { get; private set; }

        public SelectedTimeIntervalWrapper SelectedTimeIntervalWrapper { get; private set; }

        private RecordType selectedRecordType;

        public RecordType SelectedRecordType
        {
            get { return selectedRecordType; }
            set
            {
                value = value ?? unselectedRecordType;
                SetProperty(ref selectedRecordType, value);
                IsRecordTypeSelected = selectedRecordType != null && !ReferenceEquals(selectedRecordType, unselectedRecordType);
                //UpdateRoomFilter();
            }
        }

        private bool isRecordTypeSelected;

        public bool IsRecordTypeSelected
        {
            get { return isRecordTypeSelected; }
            private set { SetProperty(ref isRecordTypeSelected, value); }
        }

        private string currentPatientShortName;

        public string CurrentPatientShortName
        {
            get { return currentPatientShortName; }
            private set { SetProperty(ref currentPatientShortName, value); }
        }

        private int currentPatientId;

        public int CurrentPatientId
        {
            get { return currentPatientId; }
            set
            {
                //PatientAssignmentListViewModel.PatientId = currentPatientId = value;
                if (currentPatientId.IsNewOrNonExisting())
                {
                    CurrentPatientShortName = string.Empty;
                    return;
                }
                var query = scheduleService.GetPatientQuery(currentPatientId);
                try
                {
                    CurrentPatientShortName = query.Select(x => x.ShortName).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    log.ErrorFormatEx(ex, "Failed to load short name for patient with Id {0}", currentPatientId);
                    FailureMediator.Activate("Не удалось получить Ф.И.О. выбранного пациента", exception: ex, canBeDeactivated: true);
                }
                finally
                {
                    query.Dispose();
                }
                OverlayAssignmentCollectionViewModel.CurrentPatient = value;
                //RebuildScheduleGrid();
            }
        }

        private DateTime selectedDate;

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                if (!SetProperty(ref selectedDate, value))
                {
                    return;
                }
                OnPropertyChanged(() => Dates);
                DateChanged();
                //ClearScheduleGrid();
                //LoadAssignmentsAsync(selectedDate).ContinueWith(x => UpdateRoomFilter(), TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public IList<DateTime> Dates
        {
            get
            {
                List<DateTime> listDates = new List<DateTime>();
                var daysCountCur = daysCount;
                var curDate = SelectedDate;
                while (daysCountCur > 0)
                {
                    listDates.Add(curDate);
                    curDate = curDate.AddDays(1);
                    daysCountCur--;
                }
                return listDates;
            }
        }

        private async Task LoadAssignmentsAsync(DateTime date)
        {
            if (!await InitialLoadingAsync())
            {
                return;
            }
            UnsubscribeFromAssignmentChanges();
            date = date.Date;
            try
            {
                BusyMediator.Activate("Загрузка расписания...");
                FailureMediator.Deactivate();
                await Task.Delay(TimeSpan.FromSeconds(1.0));
                //var workingTimes =
                //    (await Task<IEnumerable<ScheduleItem>>.Factory.StartNew(x => scheduleService.GetRoomsWorkingTimeForDay((DateTime)x), date)).Where(x => x.RecordTypeId.HasValue)
                //                                                                                                                               .ToLookup(x => x.RoomId);
                //var assignments = await Task<ILookup<int, ScheduledAssignmentDTO>>.Factory.StartNew(x => scheduleService.GetRoomsAssignments((DateTime)x), date);
                //foreach (var roomViewModel in rooms)
                //{
                //    roomViewModel.OpenTime = OpenTime;
                //    roomViewModel.CloseTime = CloseTime;
                //    foreach (var occupiedTimeSlot in roomViewModel.TimeSlots.OfType<OccupiedTimeSlotViewModel>().Where(x => x != movedAssignment))
                //    {
                //        occupiedTimeSlot.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                //        occupiedTimeSlot.UpdateRequested -= RoomOnAssignmentUpdateRequested;
                //        occupiedTimeSlot.MoveRequested -= RoomOnAssignmentMoveRequested;
                //        occupiedTimeSlot.CompleteRequested -= RoomOnCompleteRequested;
                //    }
                //    roomViewModel.TimeSlots.Replace(assignments[roomViewModel.Room.Id].Select(x => new OccupiedTimeSlotViewModel(x, environment, securityService, cacheService)));
                //    foreach (var occupiedTimeSlot in roomViewModel.TimeSlots.OfType<OccupiedTimeSlotViewModel>())
                //    {
                //        occupiedTimeSlot.CancelOrDeleteRequested += RoomOnAssignmentCancelOrDeleteRequested;
                //        occupiedTimeSlot.UpdateRequested += RoomOnAssignmentUpdateRequested;
                //        occupiedTimeSlot.MoveRequested += RoomOnAssignmentMoveRequested;
                //        occupiedTimeSlot.CompleteRequested += RoomOnCompleteRequested;
                //    }
                //    roomViewModel.WorkingTimes = workingTimes[roomViewModel.Room.Id].Select(x => new WorkingTimeViewModel(x, date, cacheService)).ToArray();
                //}
                SubscribeToAssignmentChanges();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to load schedule for {0:dd-MM-yyyy}", date), ex);
                FailureMediator.Activate(
                                         "При попытке загрузить расписание возникла ошибка. Попробуйте обновить расписание или выбрать другую дату. Если ошибка повторится, обратитесь в службу поддержки",
                                         loadAssignmentsCommandWrapper,
                                         ex);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private void SubscribeToAssignmentChanges()
        {
            assignmentChangeSubscription = notificationService.Subscribe<Assignment>(x => x.AssignDateTime.Date == SelectedDate);
            if (assignmentChangeSubscription != null)
            {
                assignmentChangeSubscription.Notified += OnAssignmentNotificationRecievedAsync;
            }
        }

        private void UnsubscribeFromAssignmentChanges()
        {
            if (assignmentChangeSubscription != null)
            {
                assignmentChangeSubscription.Notified -= OnAssignmentNotificationRecievedAsync;
                assignmentChangeSubscription.Dispose();
            }
        }

        private async void OnAssignmentNotificationRecievedAsync(object sender, NotificationEventArgs<Assignment> e)
        {
            if (e.IsDelete || e.IsUpdate)
            {

            }
            if (e.IsCreate || (e.IsUpdate && e.NewItem.CancelDateTime == null))
            {

            }
            var assignment = e.OldItem ?? e.NewItem;
            if (assignment.PersonId == currentPatientId && OverlayAssignmentCollectionViewModel.ShowCurrentPatientAssignments)
            {
                OverlayAssignmentCollectionViewModel.UpdateAssignmentAsync((e.OldItem ?? e.NewItem).Id);
            }
            //RebuildScheduleGrid();
        }

        private TaskCompletionSource<bool> initialLoadingTaskSource;

        private readonly CommandWrapper initialLoadingCommandWrapper;

        private async Task<bool> InitialLoadingAsync()
        {
            if (initialLoadingTaskSource != null)
            {
                return await initialLoadingTaskSource.Task;
            }
            initialLoadingTaskSource = new TaskCompletionSource<bool>();
            log.Info("Loading data source for schedule...");
            FailureMediator.Deactivate();
            BusyMediator.Activate("Загрузка общих данных...");
            try
            {
                await Task.Run((Action)FillCache);
                var newRooms = await Task.Run((Func<IEnumerable<Room>>)scheduleService.GetRooms);
                var newRecordTypes = await Task.Run((Func<IEnumerable<RecordType>>)scheduleService.GetRecordTypes);
                RecordTypes = new[] { unselectedRecordType }.Concat(newRecordTypes).ToArray();
                SelectedRecordType = unselectedRecordType;
                initialLoadingTaskSource.SetResult(true);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Failed to load datasources for schedule from database", ex);
                FailureMediator.Activate("При попытке загрузить списки кабинетов и услуг возникла ошибка. Попробуйте перезапустить приложение. Если ошибка повторится, обратитесь в службу поддержки",
                                         initialLoadingCommandWrapper,
                                         ex);
                initialLoadingTaskSource.SetResult(false);
                return false;
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private void FillCache()
        {
            cacheService.GetItems<Room>();
            cacheService.GetItems<RecordType>();
            cacheService.GetItems<ExecutionPlace>();
            cacheService.GetItems<Urgently>();
        }

        private IEnumerable<RecordType> recordTypes;

        public IEnumerable<RecordType> RecordTypes
        {
            get { return recordTypes; }
            private set
            {
                if (SetProperty(ref recordTypes, value))
                {
                    SelectedRecordType = unselectedRecordType;
                }
            }
        }

        public DelegateCommand<int?> ChangeDateCommand { get; private set; }

        private void ChangeDate(int? days)
        {
            SelectedDate = days.HasValue ? SelectedDate.AddDays(days.Value) : DateTime.Today;
        }

        private bool firstTimeLoad;

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (firstTimeLoad)
            {
                firstTimeLoad = false;
                await LoadAssignmentsAsync(selectedDate);
            }
            else
            {
                await InitialLoadingAsync();
            }
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PatientId] ?? SpecialValues.NonExistingId;
            CurrentPatientId = targetPatientId;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //TODO: put here logic for current view being deactivated
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
            initialLoadingCommandWrapper.Dispose();
            loadAssignmentsCommandWrapper.Dispose();
            UnsubscribeFromAssignmentChanges();
        }

        public DelegateCommand addRecordTypeCommand;
        public ICommand AddRecordTypeCommand
        {
            get { return addRecordTypeCommand; }
        }
    }
}