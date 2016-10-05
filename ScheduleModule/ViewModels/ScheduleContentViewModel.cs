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

namespace ScheduleModule.ViewModels
{
    public class ScheduleContentViewModel : BindableBase, INavigationAware, IDisposable
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

        public ScheduleContentViewModel(OverlayAssignmentCollectionViewModel overlayAssignmentCollectionViewModel,
                                        PatientAssignmentListViewModel patientAssignmentListViewModel,
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
            OverlayAssignmentCollectionViewModel = overlayAssignmentCollectionViewModel;
            PatientAssignmentListViewModel = patientAssignmentListViewModel;
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
            filteredRooms = new CollectionViewSource();
            selectedDate = overlayAssignmentCollectionViewModel.CurrentDate = DateTime.Today;
            openTime = overlayAssignmentCollectionViewModel.CurrentDateRoomsOpenTime = selectedDate.AddHours(8.0);
            closeTime = overlayAssignmentCollectionViewModel.CurrentDateRoomsOpenTime = selectedDate.AddHours(17.0);
            CancelAssignmentMovementCommand = new DelegateCommand(CancelAssignmentMovement);
            ChangeDateCommand = new DelegateCommand<int?>(ChangeDate);
            ClearFiltersCommand = new DelegateCommand(ClearFilters, CanClearFilters);
            unseletedRoom = new RoomViewModel(new Room { Name = "Выберите кабинет" }, scheduleService);
            unselectedRecordType = new RecordType { Name = "Выберите услугу", Assignable = true };
            initialLoadingCommandWrapper = new CommandWrapper { Command = new DelegateCommand(async () => await InitialLoadingAsync()) };
            loadAssignmentsCommandWrapper = new CommandWrapper { Command = new DelegateCommand(async () => await LoadAssignmentsAsync(selectedDate)) };
            SubscribeToEvents();
            firstTimeLoad = true;
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
                var workingTimes =
                    (await Task<IEnumerable<ScheduleItem>>.Factory.StartNew(x => scheduleService.GetRoomsWorkingTimeForDay((DateTime)x), date)).Where(x => x.RecordTypeId.HasValue)
                                                                                                                                               .ToLookup(x => x.RoomId);
                var assignments = await Task<ILookup<int, ScheduledAssignmentDTO>>.Factory.StartNew(x => scheduleService.GetRoomsAssignments((DateTime)x), date);
                if (workingTimes.Count == 0)
                {
                    //TODO: store these settings in DB. These are just default values used for displaying purposes
                    OpenTime = OverlayAssignmentCollectionViewModel.CurrentDateRoomsOpenTime = date.AddHours(8.0);
                    CloseTime = OverlayAssignmentCollectionViewModel.CurrentDateRoomsCloseTime = date.AddHours(17.0);
                }
                else
                {
                    OpenTime = OverlayAssignmentCollectionViewModel.CurrentDateRoomsOpenTime = date.Add(workingTimes.Min(x => x.Min(y => y.StartTime)));
                    CloseTime = OverlayAssignmentCollectionViewModel.CurrentDateRoomsCloseTime = date.Add(workingTimes.Max(x => x.Max(y => y.EndTime)));
                }
                UpdateTimeTickers();
                foreach (var roomViewModel in rooms)
                {
                    roomViewModel.OpenTime = OpenTime;
                    roomViewModel.CloseTime = CloseTime;
                    foreach (var occupiedTimeSlot in roomViewModel.TimeSlots.OfType<OccupiedTimeSlotViewModel>().Where(x => x != movedAssignment))
                    {
                        occupiedTimeSlot.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                        occupiedTimeSlot.UpdateRequested -= RoomOnAssignmentUpdateRequested;
                        occupiedTimeSlot.MoveRequested -= RoomOnAssignmentMoveRequested;
                        occupiedTimeSlot.CompleteRequested -= RoomOnCompleteRequested;
                    }
                    roomViewModel.TimeSlots.Replace(assignments[roomViewModel.Room.Id].Select(x => new OccupiedTimeSlotViewModel(x, environment, securityService, cacheService)));
                    foreach (var occupiedTimeSlot in roomViewModel.TimeSlots.OfType<OccupiedTimeSlotViewModel>())
                    {
                        occupiedTimeSlot.CancelOrDeleteRequested += RoomOnAssignmentCancelOrDeleteRequested;
                        occupiedTimeSlot.UpdateRequested += RoomOnAssignmentUpdateRequested;
                        occupiedTimeSlot.MoveRequested += RoomOnAssignmentMoveRequested;
                        occupiedTimeSlot.CompleteRequested += RoomOnCompleteRequested;
                    }
                    roomViewModel.WorkingTimes = workingTimes[roomViewModel.Room.Id].Select(x => new WorkingTimeViewModel(x, date, cacheService)).ToArray();
                }
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
                var oldAssignmentRoom = rooms.FirstOrDefault(x => x.Room.Id == e.OldItem.RoomId);
                if (oldAssignmentRoom == null)
                {
                    log.InfoFormat("Notified about assignment {0} but didn't find owner room", e.OldItem.Id);
                }
                else
                {
                    var occupiedTimeSlot = oldAssignmentRoom.TimeSlots.OfType<OccupiedTimeSlotViewModel>().FirstOrDefault(x => x.Id == e.OldItem.Id);
                    if (occupiedTimeSlot == null)
                    {
                        log.InfoFormat("Notified about assignment {0} but didn't find it in its room", e.OldItem.Id);
                    }
                    else
                    {
                        occupiedTimeSlot.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                        occupiedTimeSlot.UpdateRequested -= RoomOnAssignmentUpdateRequested;
                        occupiedTimeSlot.MoveRequested -= RoomOnAssignmentMoveRequested;
                        occupiedTimeSlot.CompleteRequested -= RoomOnCompleteRequested;
                        oldAssignmentRoom.TimeSlots.Remove(occupiedTimeSlot);
                    }
                }
            }
            if (e.IsCreate || (e.IsUpdate && e.NewItem.CancelDateTime == null))
            {
                var scheduledAssignment = await scheduleService.GetAssignmentAsync(e.NewItem.Id, SpecialValues.NonExistingId);
                var newAssignmentRoom = rooms.FirstOrDefault(x => x.Room.Id == e.NewItem.RoomId);
                if (newAssignmentRoom == null)
                {
                    log.InfoFormat("Notified about assignment {0} but didn't find owner room", e.NewItem.Id);
                }
                else
                {
                    newAssignmentRoom.TimeSlots.Add(new OccupiedTimeSlotViewModel(scheduledAssignment, environment, securityService, cacheService));
                }
            }
            var assignment = e.OldItem ?? e.NewItem;
            if (assignment.PersonId == currentPatientId && OverlayAssignmentCollectionViewModel.ShowCurrentPatientAssignments)
            {
                OverlayAssignmentCollectionViewModel.UpdateAssignmentAsync((e.OldItem ?? e.NewItem).Id);
            }
            RebuildScheduleGrid();
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
                Rooms = new[] { unseletedRoom }.Concat(newRooms.Select(x => new RoomViewModel(x, scheduleService))).ToArray();
                foreach (var room in Rooms)
                {
                    room.AssignmentCreationRequested += RoomOnAssignmentCreationRequested;
                }
                var newRecordTypes = await Task.Run((Func<IEnumerable<RecordType>>)scheduleService.GetRecordTypes);
                RecordTypes = new[] { unselectedRecordType }.Concat(newRecordTypes).ToArray();
                log.InfoFormat("Loaded {0} rooms and {1} record types", (Rooms as RoomViewModel[]).Length, (RecordTypes as RecordType[]).Length);
                SelectedRecordType = unselectedRecordType;
                SelectedRoom = unseletedRoom;
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

        private DateTime openTime;

        public DateTime OpenTime
        {
            get { return openTime; }
            private set { SetProperty(ref openTime, value); }
        }

        private DateTime closeTime;

        public DateTime CloseTime
        {
            get { return closeTime; }
            private set { SetProperty(ref closeTime, value); }
        }

        private ICollection<RoomViewModel> rooms;

        public ICollection<RoomViewModel> Rooms
        {
            get { return rooms; }
            private set
            {
                filteredRooms.Source = value;
                if (!SetProperty(ref rooms, value))
                {
                    return;
                }
                filteredRooms.View.Filter = FilterRooms;
                SelectedRoom = unseletedRoom;
                OnPropertyChanged(() => FilteredRooms);
            }
        }

        private readonly CollectionViewSource filteredRooms;

        public ICollectionView FilteredRooms
        {
            get { return filteredRooms.View; }
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

        #region Assignments

        public PatientAssignmentListViewModel PatientAssignmentListViewModel { get; private set; }

        private async void RoomOnAssignmentCreationRequested(object sender, ReturnEventArgs<FreeTimeSlotViewModel> e)
        {
            var room = sender as RoomViewModel;
            var freeTimeSlot = e.Result;
            if (isMovingAssignment)
            {
                await MoveAssignmentAsync(movedAssignment, freeTimeSlot);
            }
            else
            {
                await CreateNewAssignmentAsync(room, freeTimeSlot);
            }
        }

        private async Task MoveAssignmentAsync(OccupiedTimeSlotViewModel whatToMove, FreeTimeSlotViewModel whereToMove)
        {
            try
            {
                log.InfoFormat("Trying to move assignment (Id = {0}) from {1:dd.MM HH:mm} (Room Id = {2}) to {3:dd.MM HH:mm} (Room Id = {4}) ",
                               whatToMove.Id,
                               whatToMove.StartTime,
                               whatToMove.RoomId,
                               whereToMove.StartTime,
                               whereToMove.Room.Id);
                BusyMediator.Activate("Перенос назначения...");
                await
                    scheduleService.MoveAssignmentAsync(whatToMove.Id, whereToMove.StartTime, (int)whereToMove.EndTime.Subtract(whereToMove.StartTime).TotalMinutes, whereToMove.Room,
                                                        assignmentChangeSubscription);
                log.Info("Successfully saved to database");
                var whereToMoveRoom = rooms.First(x => x.Room == whereToMove.Room);
                var whatToMoveRoom = rooms.First(x => x.Room == whatToMove.Room);
                whereToMove.AssignmentCreationRequested -= whereToMoveRoom.FreeTimeSlotOnAssignmentCreationRequested;
                whereToMoveRoom.TimeSlots.Remove(whereToMove);
                if (whatToMove.Room != whereToMove.Room || whatToMove.StartTime.Date != whereToMove.StartTime.Date)
                {
                    whatToMoveRoom.TimeSlots.Remove(whatToMove);
                    whereToMoveRoom.TimeSlots.Add(whatToMove);
                }
                whatToMove.UpdateTime(whereToMove.StartTime, whereToMove.EndTime);
                whatToMove.RoomId = whereToMove.Room.Id;
                CancelAssignmentMovement();
                log.Info("Movement completed");
                RebuildScheduleGrid();
                if (OverlayAssignmentCollectionViewModel.ShowCurrentPatientAssignments)
                {
                    OverlayAssignmentCollectionViewModel.UpdateAssignmentAsync(whatToMove.Id);
                }
            }
            catch (AssignmentConflictException ex)
            {
                FailureMediator.Activate(ex.Message, true);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to move assignment (Id = {0}) to the time slot {1:dd.MM HH:mm}", whatToMove.Id, whereToMove.StartTime), ex);
                FailureMediator.Activate("При попытке перенести назначение возникла ошибка. Пожалуйста, попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки",
                                         exception: ex,
                                         canBeDeactivated: true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private async Task CreateNewAssignmentAsync(RoomViewModel selectedRoom, FreeTimeSlotViewModel freeTimeSlot)
        {
            BusyMediator.Activate("Занимаем слот в расписании...");
            var assignment = await CreateTemporaryAssignmentAsync(selectedRoom, freeTimeSlot);
            BusyMediator.Deactivate();
            if (assignment == null)
            {
                return;
            }
            var newAssignmentDTO = new ScheduledAssignmentDTO
                                   {
                                       Id = assignment.Id,
                                       IsCompleted = false,
                                       PersonShortName = currentPatientShortName,
                                       RecordTypeId = assignment.RecordTypeId,
                                       RoomId = assignment.RoomId,
                                       StartTime = assignment.AssignDateTime,
                                       Duration = assignment.Duration,
                                       IsTemporary = true,
                                       AssignUserId = assignment.AssignUserId,
                                       AssignLpuId = assignment.AssignLpuId,
                                       Note = assignment.Note,
                                       FinancingSourceId = assignment.FinancingSourceId
                                   };
            var newAssignment = new OccupiedTimeSlotViewModel(newAssignmentDTO, environment, securityService, cacheService);
            newAssignment.CancelOrDeleteRequested += RoomOnAssignmentCancelOrDeleteRequested;
            newAssignment.UpdateRequested += RoomOnAssignmentUpdateRequested;
            newAssignment.MoveRequested += RoomOnAssignmentMoveRequested;
            newAssignment.CompleteRequested += RoomOnCompleteRequested;
            selectedRoom.TimeSlots.Remove(freeTimeSlot);
            selectedRoom.TimeSlots.Add(newAssignment);
            var dialogViewModel = new ScheduleAssignmentUpdateViewModel(scheduleService, cacheService, true);
            dialogViewModel.SelectedFinancingSource = dialogViewModel.FinancingSources.First(x => x.Id == assignment.FinancingSourceId);
            var dialogResult = await dialogService.ShowDialogAsync(dialogViewModel);
            if (dialogResult != true)
            {
                BusyMediator.Activate("Освобождение занятого слота...");
                try
                {
                    log.Info("User canceled saving thus new assignment must be deleted");
                    await scheduleService.DeleteAssignmentAsync(assignment.Id, assignmentChangeSubscription);
                    selectedRoom.TimeSlots.Remove(newAssignment);
                    selectedRoom.TimeSlots.Add(freeTimeSlot);
                    newAssignment.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                    newAssignment.UpdateRequested -= RoomOnAssignmentUpdateRequested;
                    newAssignment.MoveRequested -= RoomOnAssignmentMoveRequested;
                    newAssignment.CompleteRequested -= RoomOnCompleteRequested;
                    log.Info("New assignment was successfully deleted");
                    if (OverlayAssignmentCollectionViewModel.ShowCurrentPatientAssignments)
                    {
                        OverlayAssignmentCollectionViewModel.UpdateAssignmentAsync(assignment.Id);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed to manually delete temporary assignment (Id = {0})", assignment.Id), ex);
                    FailureMediator.Activate(
                                             "Не удалось освободить занятый слот. Попробуйте освободить его вручную через контекстное меню или оставьте его, и через некоторое время оно будет удалено автоматически",
                                             true);
                }
                finally
                {
                    BusyMediator.Deactivate();
                }
            }
            else
            {
                BusyMediator.Activate("Сохранение дополнительной информации...");
                try
                {
                    log.Info("User has chosen to save assignment, trying to update assignment...");
                    assignment.IsTemporary = false;
                    assignment.FinancingSourceId = dialogViewModel.SelectedFinancingSource.Id;
                    assignment.Note = dialogViewModel.Note;
                    assignment.AssignLpuId = dialogViewModel.IsSelfAssigned || dialogViewModel.SelectedAssignLpu == null ? null : (int?)dialogViewModel.SelectedAssignLpu.Id;
                    await scheduleService.SaveAssignmentAsync(assignment, assignmentChangeSubscription);
                    newAssignment.IsTemporary = false;
                    newAssignment.FinancingSourceId = assignment.FinancingSourceId;
                    newAssignment.Note = assignment.Note;
                    newAssignment.AssignLpuId = assignment.AssignLpuId;
                    log.Info("New assignment was updated and moved from temporary state");
                    if (OverlayAssignmentCollectionViewModel.ShowCurrentPatientAssignments)
                    {
                        OverlayAssignmentCollectionViewModel.UpdateAssignmentAsync(assignment.Id);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed to save additional data for temporary assignment (Id ={0})", assignment.Id), ex);
                    FailureMediator.Activate("Не удалось обновить данные назначения. Пожалуйста, попробуйте еще раз или отмените операцию",
                                             exception: ex,
                                             canBeDeactivated: true);
                }
                finally
                {
                    BusyMediator.Deactivate();
                }
            }
        }

        private async Task<Assignment> CreateTemporaryAssignmentAsync(RoomViewModel selectedRoom, FreeTimeSlotViewModel freeTimeSlot)
        {
            log.InfoFormat("Trying to create new assignment: room Id  {0}, start time is {1:dd.MM HH:mm}, proposed duration {2}, record type Id {3}",
                           freeTimeSlot.Room.Id,
                           freeTimeSlot.StartTime,
                           (int)freeTimeSlot.EndTime.Subtract(freeTimeSlot.StartTime).TotalMinutes,
                           freeTimeSlot.RecordType.Id);
            var financingSource = GetFinancingSource();
            if (financingSource == 0)
            {
                log.Error("There are no financing sources in database");
                FailureMediator.Activate("В базе данных отсутствует информация о доступных источниках финансирования. Обратитесь в службу поддержки", true);
                return null;
            }
            var plannedUrgency = cacheService.GetItems<Urgently>().FirstOrDefault(x => x.IsDefault);
            if (plannedUrgency == null)
            {
                FailureMediator.Activate("В базе данных отсутствует информация о плановой срочности назначения. Обратитесь в службу поддержки", true);
                return null;
            }
            var polyclinicPlace = cacheService.GetItems<ExecutionPlace>().FirstOrDefault(x => x.IsPolyclynic);
            if (polyclinicPlace == null)
            {
                FailureMediator.Activate("В базе данных отсутствует информация об амбулаторном месте выполнения назначения. Обратитесь в службу поддержки", true);
                return null;
            }
            var assignment = new Assignment
                             {
                                 AssignDateTime = freeTimeSlot.StartTime,
                                 Duration = (int)freeTimeSlot.EndTime.Subtract(freeTimeSlot.StartTime).TotalMinutes,
                                 AssignUserId = environment.CurrentUser.Id,
                                 Note = string.Empty,
                                 FinancingSourceId = financingSource,
                                 PersonId = currentPatientId,
                                 RecordTypeId = freeTimeSlot.RecordType.Id,
                                 RoomId = selectedRoom.Room.Id,
                                 IsTemporary = true,
                                 UrgentlyId = plannedUrgency.Id,
                                 ExecutionPlaceId = polyclinicPlace.Id
                             };
            try
            {
                await scheduleService.SaveAssignmentAsync(assignment, assignmentChangeSubscription);
                log.InfoFormat("New assignment (Id = {0}) is saved as temporary", assignment.Id);
            }
            catch (AssignmentConflictException ex)
            {
                FailureMediator.Activate(ex.Message, true);
                return null;
            }
            catch (Exception ex)
            {
                log.Error("Failed to save new assignment", ex);
                FailureMediator.Activate("Не удалось создать назначение. Попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки",
                                         exception: ex,
                                         canBeDeactivated: true);
                return null;
            }
            if (OverlayAssignmentCollectionViewModel.ShowCurrentPatientAssignments)
            {
                OverlayAssignmentCollectionViewModel.UpdateAssignmentAsync(assignment.Id);
            }
            return assignment;
        }

        private async void RoomOnAssignmentCancelOrDeleteRequested(object sender, EventArgs e)
        {
            var assignment = (OccupiedTimeSlotViewModel)sender;
            if (assignment.IsTemporary)
            {
                try
                {
                    log.InfoFormat("Trying to manually delete temporary assignment (Id = {0})", assignment.Id);
                    await scheduleService.DeleteAssignmentAsync(assignment.Id, assignmentChangeSubscription);
                    assignment.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                    assignment.UpdateRequested -= RoomOnAssignmentUpdateRequested;
                    assignment.MoveRequested -= RoomOnAssignmentMoveRequested;
                    assignment.CompleteRequested -= RoomOnCompleteRequested;
                    rooms.First(x => x.Room == assignment.Room).TimeSlots.Remove(assignment);
                    RebuildScheduleGrid();
                    log.Info("Temporary assignment was manually deleted");
                    if (OverlayAssignmentCollectionViewModel.ShowCurrentPatientAssignments)
                    {
                        OverlayAssignmentCollectionViewModel.UpdateAssignmentAsync(assignment.Id);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed to manually delete temporary assignment (Id = {0})", assignment.Id), ex);
                    FailureMediator.Activate("Не удалось удалить временное назначение. Пожалуйста, попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки", true);
                }
            }
            else
            {
                try
                {
                    var confirmation = new ConfirmationDialogViewModel
                                       {
                                           Question = string.Format("Вы действительно хотите отменить назначение пациента {0} на {1:d MMMM HH:mm}?",
                                                                    assignment.PersonShortName,
                                                                    assignment.StartTime)
                                       };
                    var result = await dialogService.ShowDialogAsync(confirmation);
                    if (result != true)
                    {
                        return;
                    }
                    log.InfoFormat("Trying to cancel assignment (Id = {0})", assignment.Id);
                    await scheduleService.CancelAssignmentAsync(assignment.Id, assignmentChangeSubscription);
                    rooms.First(x => x.Room == assignment.Room).TimeSlots.Remove(assignment);
                    RebuildScheduleGrid();
                    log.Info("Assignment was canceled");
                    if (OverlayAssignmentCollectionViewModel.ShowCurrentPatientAssignments)
                    {
                        OverlayAssignmentCollectionViewModel.UpdateAssignmentAsync(assignment.Id);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed to cancel assignment (Id = {0})", assignment.Id), ex);
                    FailureMediator.Activate("Не удалось отменить назначение. Пожалуйста, попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки", exception: ex);
                }
            }
        }

        private void RoomOnAssignmentMoveRequested(object sender, EventArgs e)
        {
            var assignmentViewModel = sender as OccupiedTimeSlotViewModel;
            movedAssignment = assignmentViewModel;
            IsMovingAssignment = true;
        }

        private bool isMovingAssignment;

        public bool IsMovingAssignment
        {
            get { return isMovingAssignment; }
            set
            {
                if (!SetProperty(ref isMovingAssignment, value))
                {
                    return;
                }
                CancelAssignmentMovementCommand.RaiseCanExecuteChanged();
                movedAssignment.IsBeingMoved = value;
                if (value)
                {
                    log.InfoFormat("Entering movement mode for assignment (Id = {0})", movedAssignment.Id);
                    previousSelectedRecordType = selectedRecordType;
                    previousSelectedRoom = selectedRoom;
                    SelectedRecordType = recordTypes.SelectMany(x => x.GetAllChildren(true)).First(x => x.Id == movedAssignment.RecordTypeId);
                    CollectionViewSource.GetDefaultView(recordTypes).Filter = x => ((RecordType)x).Id == SelectedRecordType.Id;
                    SelectedRoom = unseletedRoom;
                }
                else
                {
                    SelectedRoom = previousSelectedRoom;
                    SelectedRecordType = previousSelectedRecordType;
                    CollectionViewSource.GetDefaultView(recordTypes).Filter = null;
                    log.InfoFormat("Leaving movement mode for assignment (Id = {0})", movedAssignment.Id);
                }
                UpdateAssignmentsReadOnlyMode();
                ClearFiltersCommand.RaiseCanExecuteChanged();
            }
        }

        private void UpdateAssignmentsReadOnlyMode()
        {
            foreach (var occupiedTimeSlot in rooms.SelectMany(x => x.TimeSlots.OfType<OccupiedTimeSlotViewModel>()))
            {
                occupiedTimeSlot.IsInReadOnlyMode = isMovingAssignment;
                if (isMovingAssignment && occupiedTimeSlot.Id == movedAssignment.Id)
                {
                    occupiedTimeSlot.IsBeingMoved = true;
                }
            }
        }

        private RecordType previousSelectedRecordType;

        private RoomViewModel previousSelectedRoom;

        private OccupiedTimeSlotViewModel movedAssignment;

        public DelegateCommand CancelAssignmentMovementCommand { get; private set; }

        private void CancelAssignmentMovement()
        {
            IsMovingAssignment = false;
            movedAssignment = null;
        }

        public DelegateCommand ClearFiltersCommand { get; private set; }

        private void ClearFilters()
        {
            if (!CanClearFilters())
            {
                return;
            }
            SelectedRecordType = unselectedRecordType;
            SelectedRoom = unseletedRoom;
        }

        private bool CanClearFilters()
        {
            return (SelectedRecordType != unselectedRecordType
                    || SelectedRoom != unseletedRoom)
                   && !IsMovingAssignment;
        }

        private async void RoomOnCompleteRequested(object sender, EventArgs e)
        {
            var assignment = (OccupiedTimeSlotViewModel)sender;
            var dialogViewModel = new DateTimeSelectionDialogViewModel();
            var dialogResult = await dialogService.ShowDialogAsync(dialogViewModel);
            if (dialogResult != true)
            {
                return;
            }
            try
            {
                log.InfoFormat("Trying to mark assignment (Id = {0}) as completed", assignment.Id);
                await scheduleService.MarkAssignmentCompletedAsync(assignment.Id, dialogViewModel.SelectedDateTime, assignmentChangeSubscription);
                assignment.IsCompleted = true;
                log.Info("Successfully completed assignment");
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to mark assignment (Id ={0}) as completed", assignment.Id), ex);
                FailureMediator.Activate("Не удалось пометить назначение как выполненное. Пожалуйста, попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки");
            }
        }

        private async void RoomOnAssignmentUpdateRequested(object sender, EventArgs e)
        {
            var assignment = (OccupiedTimeSlotViewModel)sender;
            var dialogViewModel = new ScheduleAssignmentUpdateViewModel(scheduleService, cacheService, false) { Note = assignment.Note };
            dialogViewModel.SelectedFinancingSource = dialogViewModel.FinancingSources.FirstOrDefault(x => x.Id == assignment.FinancingSourceId);
            dialogViewModel.SelectedAssignLpu = dialogViewModel.AssignLpuList.FirstOrDefault(x => x.Id == assignment.AssignLpuId);
            var dialogResult = await dialogService.ShowDialogAsync(dialogViewModel);
            if (dialogResult != true)
            {
                return;
            }
            try
            {
                log.InfoFormat("Trying to update information on assignment (Id = {0})", assignment.Id);
                var newAssignLpuId = dialogViewModel.IsSelfAssigned || dialogViewModel.SelectedAssignLpu == null ? null : (int?)dialogViewModel.SelectedAssignLpu.Id;
                await scheduleService.UpdateAssignmentAsync(assignment.Id, dialogViewModel.SelectedFinancingSource.Id, dialogViewModel.Note, newAssignLpuId, assignmentChangeSubscription);
                assignment.FinancingSourceId = dialogViewModel.SelectedFinancingSource.Id;
                assignment.Note = dialogViewModel.Note;
                assignment.AssignLpuId = newAssignLpuId;
                assignment.IsTemporary = false;
                log.Info("Successfully updated information");
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to update information for existing assignment (Id ={0})", assignment.Id), ex);
                FailureMediator.Activate("Не удалось обновить данные назначения. Пожалуйста, попробуйте еще раз или отмените операцию");
            }
        }

        private int GetFinancingSource()
        {
            var result = cacheService.GetItems<FinancingSource>().Where(x => !x.IsActive && !string.IsNullOrEmpty(x.Options)).Select(x => x.Id).FirstOrDefault();
            if (result == 0)
            {
                result = cacheService.GetItems<FinancingSource>().Where(x => !string.IsNullOrEmpty(x.Options)).Select(x => x.Id).FirstOrDefault();
            }
            return result;
        }

        #endregion

        #region Filter

        private bool noRoomIsFound;

        public bool NoRoomIsFound
        {
            get { return noRoomIsFound; }
            private set { SetProperty(ref noRoomIsFound, value); }
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
                OverlayAssignmentCollectionViewModel.CurrentDate = value;
                ClearScheduleGrid();
                LoadAssignmentsAsync(selectedDate).ContinueWith(x => UpdateRoomFilter(), TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private RoomViewModel selectedRoom;

        public RoomViewModel SelectedRoom
        {
            get { return selectedRoom; }
            set
            {
                value = value ?? unseletedRoom;
                SetProperty(ref selectedRoom, value);
                IsRoomSelected = !ReferenceEquals(selectedRoom, unseletedRoom);
                UpdateRoomFilter();
                ClearFiltersCommand.RaiseCanExecuteChanged();
            }
        }

        private bool isRoomSelected;

        public bool IsRoomSelected
        {
            get { return isRoomSelected; }
            private set { SetProperty(ref isRoomSelected, value); }
        }

        private RecordType selectedRecordType;

        public RecordType SelectedRecordType
        {
            get { return selectedRecordType; }
            set
            {
                value = value ?? unselectedRecordType;
                SetProperty(ref selectedRecordType, value);
                IsRecordTypeSelected = selectedRecordType != null && !ReferenceEquals(selectedRecordType, unselectedRecordType);
                UpdateRoomFilter();
                ClearFiltersCommand.RaiseCanExecuteChanged();
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
                PatientAssignmentListViewModel.PatientId = currentPatientId = value;
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
                RebuildScheduleGrid();
            }
        }

        private void UpdateRoomFilter()
        {
            FilteredRooms.Filter = FilterRooms;
            NoRoomIsFound = Rooms.Count > 1 && FilteredRooms.IsEmpty && (SelectedRoom != unseletedRoom || SelectedRecordType != unselectedRecordType);
            RebuildScheduleGrid();
        }

        private void RebuildScheduleGrid()
        {
            ClearScheduleGrid();
            BuildScheduleGrid();
        }

        private void BuildScheduleGrid()
        {
            //This means schedule is not yet loaded
            if (rooms == null)
            {
                return;
            }
            if (currentPatientId.IsNewOrNonExisting() && !IsMovingAssignment)
            {
                return;
            }
            if (!securityService.HasPermission(Permission.EditAssignments))
            {
                return;
            }
            try
            {
                FailureMediator.Deactivate();
                foreach (var room in rooms)
                {
                    room.BuildScheduleGrid(selectedDate, isRoomSelected ? selectedRoom.Room : null, isRecordTypeSelected ? selectedRecordType : null);
                }
                UpdateAssignmentsReadOnlyMode();
                log.Info("Schedule grid is built");
            }
            catch (Exception ex)
            {
                FailureMediator.Activate(
                                         string.Format(
                                                       "Не удалось определить доступные для назначений временные интервалы. Возможно в базе не содержится информация о номинальной или минимальной длительности такого типа назначений как {0}. Пожалуйста, обратитесь в службу поддержки",
                                                       selectedRecordType.Name));
                log.Error("Failed to build schedule grid", ex);
            }
        }

        private void ClearScheduleGrid()
        {
            //This means schedule is not yet loaded
            if (rooms == null)
            {
                return;
            }
            foreach (var room in rooms)
            {
                room.TimeSlots.RemoveWhere(x => x is FreeTimeSlotViewModel);
            }
            log.Info("Schedule grid is cleared");
        }

        private bool FilterRooms(object obj)
        {
            var room = obj as RoomViewModel;
            if (room == null || ReferenceEquals(room, unseletedRoom))
            {
                return false;
            }
            return (!isRoomSelected || room == selectedRoom) && (!isRecordTypeSelected || room.AllowsRecordType(selectedRecordType));
        }

        #endregion

        #region Time tickers

        private IEnumerable<TimeTickerViewModel> timeTickers;

        public IEnumerable<TimeTickerViewModel> TimeTickers
        {
            get { return timeTickers; }
            set { SetProperty(ref timeTickers, value); }
        }

        private void UpdateTimeTickers()
        {
            var result = new List<TimeTickerViewModel>();
            var currentTime = openTime;
            while (currentTime < closeTime)
            {
                result.Add(new TimeTickerViewModel(currentTime));
                currentTime = currentTime.AddHours(1.0);
            }
            TimeTickers = result;
        }

        #endregion

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
    }
}