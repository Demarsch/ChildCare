using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Navigation;
using Core;
using DataLib;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using Environment = System.Environment;

namespace Registry
{
    public class ScheduleViewModel : BasicViewModel
    {
        private readonly ILog log;

        private readonly IScheduleService scheduleService;

        private readonly RoomViewModel unseletedRoom;

        private readonly RecordType unselectedRecordType;

        private readonly ICacheService cacheService;

        private readonly IEnvironment environment;

        private readonly IDialogService dialogService;

        private readonly ISecurityService securityService;

        public ScheduleViewModel(CurrentPatientAssignmentsViewModel currentPatientAssignmentsViewModel, IScheduleService scheduleService, ILog log, ICacheService cacheService, IEnvironment environment, IDialogService dialogService, ISecurityService securityService)
        {
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
            if (currentPatientAssignmentsViewModel == null)
            {
                throw new ArgumentNullException("currentPatientAssignmentsViewModel");
            }
            CurrentPatientAssignmentsViewModel = currentPatientAssignmentsViewModel;
            this.securityService = securityService;
            this.dialogService = dialogService;
            this.environment = environment;
            this.cacheService = cacheService;
            this.log = log;
            this.scheduleService = scheduleService;
            filteredRooms = new CollectionViewSource();
            selectedDate = currentPatientAssignmentsViewModel.CurrentDate = DateTime.Today;
            openTime = currentPatientAssignmentsViewModel.CurrentDateRoomsOpenTime = selectedDate.AddHours(8.0);
            closeTime = currentPatientAssignmentsViewModel.CurrentDateRoomsOpenTime = selectedDate.AddHours(17.0);
            isInReadOnlyMode = true;
            ChangeModeCommand = new RelayCommand(ChangeMode, CanChangeMode);
            CancelAssignmentMovementCommand = new RelayCommand(CancelAssignmentMovement);
            OpenScheduleEditorCommand = new RelayCommand(OpenScheduleEditor, CanOpenScheduleEditor);
            unseletedRoom = new RoomViewModel(new Room { Name = "Выберите кабинет" }, scheduleService, cacheService);
            unselectedRecordType = new RecordType { Name = "Выберите услугу" };
            LoadDataSources();
            LoadAssignmentsAsync(selectedDate);
        }

        public CurrentPatientAssignmentsViewModel CurrentPatientAssignmentsViewModel { get; private set; }

        private async Task LoadAssignmentsAsync(DateTime date)
        {
            if (!DataSourcesAreLoaded)
                return;
            date = date.Date;
            try
            {
                BusyStatus = "Загрузка расписания...";
                FailReason = null;
                await Task.Delay(TimeSpan.FromSeconds(1.0));
                var workingTimes = (await Task<ICollection<ScheduleItem>>.Factory.StartNew(x => scheduleService.GetRoomsWorkingTime((DateTime)x), date)).Where(x => x.RecordTypeId.HasValue).ToLookup(x => x.RoomId);
                var assignments = await Task<ILookup<int, ScheduledAssignmentDTO>>.Factory.StartNew(x => scheduleService.GetRoomsAssignments((DateTime)x), date);
                if (workingTimes.Count == 0)
                {
                    //TODO: store these settings in DB. This are just default values used for displaying purposes
                    OpenTime = CurrentPatientAssignmentsViewModel.CurrentDateRoomsOpenTime = date.AddHours(8.0);
                    CloseTime = CurrentPatientAssignmentsViewModel.CurrentDateRoomsCloseTime = date.AddHours(17.0);
                }
                else
                {
                    OpenTime = CurrentPatientAssignmentsViewModel.CurrentDateRoomsOpenTime = date.Add(workingTimes.Min(x => x.Min(y => y.StartTime)));
                    CloseTime = CurrentPatientAssignmentsViewModel.CurrentDateRoomsCloseTime = date.Add(workingTimes.Max(x => x.Max(y => y.EndTime)));
                }
                UpdateTimeTickers();
                foreach (var room in rooms)
                {
                    room.OpenTime = OpenTime;
                    room.CloseTime = CloseTime;
                    foreach (var occupiedTimeSlot in room.TimeSlots.OfType<OccupiedTimeSlotViewModel>().Where(x => x != movedAssignment))
                    {
                        occupiedTimeSlot.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                        occupiedTimeSlot.UpdateRequested -= RoomOnAssignmentUpdateRequested;
                        occupiedTimeSlot.MoveRequested -= RoomOnAssignmentMoveRequested;
                    }
                    room.TimeSlots.Replace(assignments[room.Id].Select(x => new OccupiedTimeSlotViewModel(x, environment, securityService)));
                    foreach (var occupiedTimeSlot in room.TimeSlots.OfType<OccupiedTimeSlotViewModel>())
                    {
                        occupiedTimeSlot.CancelOrDeleteRequested += RoomOnAssignmentCancelOrDeleteRequested;
                        occupiedTimeSlot.UpdateRequested += RoomOnAssignmentUpdateRequested;
                        occupiedTimeSlot.MoveRequested += RoomOnAssignmentMoveRequested;
                    }
                    room.WorkingTimes = workingTimes[room.Id].Select(x => new ScheduleItemViewModel(x, date)).ToArray();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to load schedule for {0:dd-MM-yyyy}", date), ex);
                FailReason = "При попытке загрузить расписание возникла ошибка. Попробуйте обновить расписание или выбрать другую дату. Если ошибка повторится, обратитесь в службу поддержки";
            }
            finally
            {
                BusyStatus = null;
            }
        }

        private void LoadDataSources()
        {
            try
            {
                log.Info("Loading data source for schedule...");
                Rooms = new[] { unseletedRoom }.Concat(scheduleService.GetRooms().Select(x => new RoomViewModel(x, scheduleService, cacheService))).ToArray();
                foreach (var room in Rooms)
                    room.AssignmentCreationRequested += RoomOnAssignmentCreationRequested;
                RecordTypes = new[] { unselectedRecordType }.Concat(scheduleService.GetRecordTypes()).ToArray();
                DataSourcesAreLoaded = true;
                log.InfoFormat("Loaded {0} rooms and {1} record types", (rooms as RoomViewModel[]).Length, (RecordTypes as RecordType[]).Length);
            }
            catch (Exception ex)
            {
                log.Error("Failed to load datasources for schedule from database", ex);
                FailReason = "При попытке загрузить списки кабинетов и услуг возникла ошибка. Попробуйте перезапустить приложение. Если ошибка повторится, обратитесь в службу поддержки";
            }
        }

        private DateTime openTime;

        public DateTime OpenTime
        {
            get { return openTime; }
            private set { Set("OpenTime", ref openTime, value); }
        }

        private DateTime closeTime;

        public DateTime CloseTime
        {
            get { return closeTime; }
            private set { Set("CloseTime", ref closeTime, value); }
        }

        private bool dataSourcesAreLoaded;

        public bool DataSourcesAreLoaded
        {
            get { return dataSourcesAreLoaded; }
            private set { Set("DataSourcesAreLoaded", ref dataSourcesAreLoaded, value); }
        }

        private IEnumerable<RoomViewModel> rooms;

        public IEnumerable<RoomViewModel> Rooms
        {
            get { return rooms; }
            private set
            {
                if (Set("Rooms", ref rooms, value))
                {
                    filteredRooms.Source = value;
                    filteredRooms.View.Filter = FilterRooms;
                    RaisePropertyChanged("FilteredRooms");
                }
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
            private set { Set("ScheduleItems", ref recordTypes, value); }
        }

        public RelayCommand OpenScheduleEditorCommand { get; private set; }

        private async void OpenScheduleEditor()
        {
            using (var scheduleEditor = new ScheduleEditorViewModel(scheduleService, log, cacheService, dialogService))
            {
                if (dialogService.ShowDialog(scheduleEditor) == true)
                {
                    await LoadAssignmentsAsync(selectedDate);
                    UpdateRoomFilter();
                }
            }
        }

        private bool CanOpenScheduleEditor()
        {
            return securityService.HasPrivilege(Privileges.EditSchedule);
        }

        #region Assignments
        
        private void RoomOnAssignmentCreationRequested(object sender, ReturnEventArgs<FreeTimeSlotViewModel> e)
        {
            var room = sender as RoomViewModel;
            var freeTimeSlot = e.Result;
            if (isMovingAssignment)
            {
                MoveAssignment(movedAssignment, freeTimeSlot);
            }
            else
            {
                CreateNewAssignment(room, freeTimeSlot);
            }
        }

        private void MoveAssignment(OccupiedTimeSlotViewModel whatToMove, FreeTimeSlotViewModel whereToMove)
        {
            try
            {
                log.InfoFormat("Trying to move assignment (Id = {0}) from {1:dd.MM HH:mm} (Room Id = {2}) to {3:dd.MM HH:mm} (Room Id = {4}) ",
                    whatToMove.Id,
                    whatToMove.StartTime,
                    whatToMove.RoomId,
                    whereToMove.StartTime,
                    whereToMove.RoomId);
                scheduleService.MoveAssignment(whatToMove.Id, whereToMove.StartTime, (int)whereToMove.EndTime.Subtract(whereToMove.StartTime).TotalMinutes, whereToMove.RoomId);
                log.Info("Successfully saved to database");
                var whereToMoveRoom = rooms.First(x => x.Id == whereToMove.RoomId);
                var whatToMoveRoom = rooms.First(x => x.Id == whatToMove.RoomId);
                whereToMove.AssignmentCreationRequested -= whereToMoveRoom.FreeTimeSlotOnAssignmentCreationRequested;
                whereToMoveRoom.TimeSlots.Remove(whereToMove);
                if (whatToMove.RoomId != whereToMove.RoomId || whatToMove.StartTime.Date != whereToMove.StartTime.Date)
                {
                    whatToMoveRoom.TimeSlots.Remove(whatToMove);
                    whereToMoveRoom.TimeSlots.Add(whatToMove);
                }
                whatToMove.UpdateTime(whereToMove.StartTime, whereToMove.EndTime);
                whatToMove.RoomId = whereToMove.RoomId;
                CancelAssignmentMovement();
                log.Info("Movement completed");
                ClearScheduleGrid();
                BuildScheduleGrid();
                CurrentPatientAssignmentsViewModel.UpdateAssignmentAsync(whatToMove.Id);
            }
            catch (AssignmentConflictException ex)
            {
                dialogService.ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed to move assignment (Id = {0}) to the time slot {1:dd.MM HH:mm}", whatToMove.Id, whereToMove.StartTime), ex);
                dialogService.ShowError("При попытке перенести назначение возникла ошибка. Пожалуйста, попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки");
            }
        }
        //TODO: worth reviewing this code to probably get rid of all type of assignment objects
        private void CreateNewAssignment(RoomViewModel selectedRoom, FreeTimeSlotViewModel freeTimeSlot)
        {
            log.InfoFormat("Trying to create new assignment: room Id  {0}, start time is {1:dd.MM HH:mm}, proposed duration {2}, record type Id {3}",
                freeTimeSlot.RoomId,
                freeTimeSlot.StartTime,
                (int)freeTimeSlot.EndTime.Subtract(freeTimeSlot.StartTime).TotalMinutes,
                freeTimeSlot.RecordTypeId);
            var financingSource = GetFinancingSource();
            if (financingSource == 0)
            {
                log.Error("There are no financing sources in database");
                dialogService.ShowError("В базе данных отсутствует информация о доступных источниках финансирования. Обратитесь в службу поддержки");
                return;
            }
            var assignment = new Assignment
            {
                AssignDateTime = freeTimeSlot.StartTime,
                Duration = (int)freeTimeSlot.EndTime.Subtract(freeTimeSlot.StartTime).TotalMinutes,
                AssignUserId = environment.CurrentUser.UserId,
                Note = string.Empty,
                FinancingSourceId = financingSource,
                PersonId = currentPatient.Id,
                RecordTypeId = freeTimeSlot.RecordTypeId,
                RoomId = selectedRoom.Id,
                IsTemporary = true,
            };
            try
            {
                scheduleService.SaveAssignment(assignment);
                log.InfoFormat("New assignment (Id = {0}) is saved as temporary", assignment.Id);
            }
            catch (AssignmentConflictException ex)
            {
                dialogService.ShowError(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                log.Error("Failed to save new assignment", ex);
                dialogService.ShowError(string.Format("Не удалось создать назначение. Причина - {0}{1}Попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки", ex.Message,
                    Environment.NewLine));
                return;
            }
            CurrentPatientAssignmentsViewModel.UpdateAssignmentAsync(assignment.Id);
            var newAssignmentDTO = new ScheduledAssignmentDTO
            {
                Id = assignment.Id,
                IsCompleted = false,
                PersonShortName = currentPatient.ShortName,
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
            var newAssignment = new OccupiedTimeSlotViewModel(newAssignmentDTO, environment, securityService);
            newAssignment.CancelOrDeleteRequested += RoomOnAssignmentCancelOrDeleteRequested;
            newAssignment.UpdateRequested += RoomOnAssignmentUpdateRequested;
            newAssignment.MoveRequested += RoomOnAssignmentMoveRequested;
            selectedRoom.TimeSlots.Remove(freeTimeSlot);
            selectedRoom.TimeSlots.Add(newAssignment);
            var isFailed = false;
            do
            {
                var dialogViewModel = new ScheduleAssignmentUpdateViewModel(cacheService, true);
                dialogViewModel.SelectedFinancingSource = dialogViewModel.FinacingSources.First(x => x.Id == assignment.FinancingSourceId);
                var dialogResult = dialogService.ShowDialog(dialogViewModel);
                if (dialogResult != true)
                {
                    try
                    {
                        log.Info("User canceled saving thus new assignment must be deleted");
                        isFailed = false;
                        scheduleService.DeleteAssignment(assignment.Id);
                        selectedRoom.TimeSlots.Remove(newAssignment);
                        selectedRoom.TimeSlots.Add(freeTimeSlot);
                        newAssignment.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                        newAssignment.UpdateRequested -= RoomOnAssignmentUpdateRequested;
                        newAssignment.MoveRequested -= RoomOnAssignmentMoveRequested;
                        log.Info("New assignment was successfully deleted");
                        CurrentPatientAssignmentsViewModel.UpdateAssignmentAsync(assignment.Id);
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Failed to manually delete temporary assignment (Id = {0})", assignment.Id), ex);
                        dialogService.ShowError(
                            "Не удалось удалить временное назначение. Попробуйте удалить его вручную через контекстное меню или оставьте его, и через некоторое время оно будет удалено автоматически");
                    }
                }
                else
                {
                    try
                    {
                        log.Info("User has chosen to save assignment, trying to update assignment...");
                        assignment.IsTemporary = false;
                        assignment.FinancingSourceId = dialogViewModel.SelectedFinancingSource.Id;
                        assignment.Note = dialogViewModel.Note;
                        assignment.AssignLpuId = dialogViewModel.IsSelfAssigned || dialogViewModel.SelectedAssignLpu == null ? null : (int?)dialogViewModel.SelectedAssignLpu.Id;
                        scheduleService.SaveAssignment(assignment);
                        newAssignment.IsTemporary = false;
                        newAssignment.FinancingSourceId = assignment.FinancingSourceId;
                        newAssignment.Note = assignment.Note;
                        newAssignment.AssignLpuId = assignment.AssignLpuId;
                        isFailed = false;
                        log.Info("New assignment was updated and moved from temporary state");
                        CurrentPatientAssignmentsViewModel.UpdateAssignmentAsync(assignment.Id);
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Failed to save additional data for temporary assignment (Id ={0})", assignment.Id), ex);
                        dialogService.ShowError("Не удалось обновить данные назначения. Пожалуйста, попробуйте еще раз или отмените операцию");
                        isFailed = true;
                    }
                }
            } while (isFailed);
        }

        private void RoomOnAssignmentCancelOrDeleteRequested(object sender, EventArgs e)
        {
            var assignment = sender as OccupiedTimeSlotViewModel;
            if (assignment.IsTemporary)
            {
                try
                {
                    log.InfoFormat("Trying to manually delete temporary assignment (Id = {0})", assignment.Id);
                    scheduleService.DeleteAssignment(assignment.Id);
                    assignment.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                    assignment.UpdateRequested -= RoomOnAssignmentUpdateRequested;
                    assignment.MoveRequested -= RoomOnAssignmentMoveRequested;
                    rooms.First(x => x.Id == assignment.RoomId).TimeSlots.Remove(assignment);
                    ClearScheduleGrid();
                    BuildScheduleGrid();
                    log.Info("Temporary assignment was manually deleted");
                    CurrentPatientAssignmentsViewModel.UpdateAssignmentAsync(assignment.Id);
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed to manually delete temporary assignment (Id = {0})", assignment.Id), ex);
                    dialogService.ShowError("Не удалось удалить временное назначение. Пожалуйста, попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки");
                }
            }
            else
            {
                try
                {
                    var result = dialogService.AskUser(string.Format("Вы действительно хотите отменить назначение пациента {0} на {1:d MMMM HH:mm}?", assignment.PersonShortName, assignment.StartTime), true);
                    if (result != true)
                    {
                        return;
                    }
                    log.InfoFormat("Trying to cancel assignment (Id = {0})", assignment.Id);
                    scheduleService.CancelAssignment(assignment.Id);
                    rooms.First(x => x.Id == assignment.RoomId).TimeSlots.Remove(assignment);
                    ClearScheduleGrid();
                    BuildScheduleGrid();
                    log.Info("Assignment was canceled");
                    CurrentPatientAssignmentsViewModel.UpdateAssignmentAsync(assignment.Id);
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed to cancel assignment (Id = {0})", assignment.Id), ex);
                    dialogService.ShowError("Не удалось отменить назначение. Пожалуйста, попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки");
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
                if (Set("IsMovingAssignment", ref isMovingAssignment, value))
                {
                    CancelAssignmentMovementCommand.RaiseCanExecuteChanged();
                    movedAssignment.IsBeingMoved = value;
                    if (value)
                    {
                        log.InfoFormat("Entering movement mode for assignment (Id = {0})", movedAssignment.Id);
                        wasInReadOnlyMode = isInReadOnlyMode;
                        previousSelectedRecordType = selectedRecordType;
                        previousSelectedRoom = selectedRoom;
                        IsInReadOnlyMode = false;
                        SelectedRecordType = recordTypes.First(x => x.Id == movedAssignment.RecordTypeId);
                        CollectionViewSource.GetDefaultView(recordTypes).Filter = x => (x as RecordType).Id == SelectedRecordType.Id;
                        SelectedRoom = null;
                    }
                    else
                    {
                        SelectedRoom = previousSelectedRoom;
                        SelectedRecordType = previousSelectedRecordType;
                        IsInReadOnlyMode = wasInReadOnlyMode;
                        CollectionViewSource.GetDefaultView(recordTypes).Filter = null;
                        log.InfoFormat("Leaving movement mode for assignment (Id = {0})", movedAssignment.Id);
                    }
                    UpdateAssignmentsReadOnlyMode();
                }
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

        private bool wasInReadOnlyMode;

        private RecordType previousSelectedRecordType;

        private RoomViewModel previousSelectedRoom;

        private OccupiedTimeSlotViewModel movedAssignment;

        public RelayCommand CancelAssignmentMovementCommand { get; private set; }

        private void CancelAssignmentMovement()
        {
            IsMovingAssignment = false;
            movedAssignment = null;
        }

        private void RoomOnAssignmentUpdateRequested(object sender, EventArgs e)
        {
            var assignment = sender as OccupiedTimeSlotViewModel;
            var dialogViewModel = new ScheduleAssignmentUpdateViewModel(cacheService, false);
            dialogViewModel.Note = assignment.Note;
            dialogViewModel.SelectedFinancingSource = dialogViewModel.FinacingSources.FirstOrDefault(x => x.Id == assignment.FinancingSourceId);
            dialogViewModel.SelectedAssignLpu = dialogViewModel.AssignLpuList.FirstOrDefault(x => x.Id == assignment.AssignLpuId);
            bool isFailed;
            do
            {
                var dialogResult = dialogService.ShowDialog(dialogViewModel);
                if (dialogResult != true)
                {
                    return;
                }
                try
                {
                    log.InfoFormat("Trying to update information on assignment (Id = {0})", assignment.Id);
                    var newAssignLpuId = dialogViewModel.IsSelfAssigned || dialogViewModel.SelectedAssignLpu == null ? null : (int?)dialogViewModel.SelectedAssignLpu.Id;
                    scheduleService.UpdateAssignment(assignment.Id, dialogViewModel.SelectedFinancingSource.Id, dialogViewModel.Note, newAssignLpuId);
                    assignment.FinancingSourceId = dialogViewModel.SelectedFinancingSource.Id;
                    assignment.Note = dialogViewModel.Note;
                    assignment.AssignLpuId = newAssignLpuId;
                    isFailed = false;
                    log.Info("Successfully updated information");
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed to update information for existing assignment (Id ={0})", assignment.Id), ex);
                    dialogService.ShowError("Не удалось обновить данные назначения. Пожалуйста, попробуйте еще раз или отмените операцию");
                    isFailed = true;
                }
            } while (isFailed);
        }

        private int GetFinancingSource()
        {
            var result = cacheService.GetItems<FinacingSource>().Where(x => !x.IsActive).Select(x => x.Id).FirstOrDefault();
            if (result == 0)
            {
                result = cacheService.GetItems<FinacingSource>().Select(x => x.Id).FirstOrDefault();
            }
            return result;
        }

        #endregion

        #region Filter

        private bool noRoomIsFound;

        public bool NoRoomIsFound
        {
            get { return noRoomIsFound; }
            private set { Set("NoRoomIsFound", ref noRoomIsFound, value); }
        }

        private DateTime selectedDate;

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                if (Set("SelectedDate", ref selectedDate, value))
                {
                    CurrentPatientAssignmentsViewModel.CurrentDate = value;
                    if (!IsInReadOnlyMode)
                    {
                        ClearScheduleGrid();
                    }
                    LoadAssignmentsAsync(selectedDate)
                        .ContinueWith(x =>
                        {
                            if (!IsInReadOnlyMode)
                            {
                                UpdateRoomFilter();
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        private bool isInReadOnlyMode;

        public bool IsInReadOnlyMode
        {
            get { return isInReadOnlyMode; }
            set
            {
                Set("IsInReadOnlyMode", ref isInReadOnlyMode, value);
                if (!isInReadOnlyMode)
                    return;
                SelectedRoom = null;
                SelectedRecordType = null;
            }
        }

        public RelayCommand ChangeModeCommand { get; private set; }

        private void ChangeMode()
        {
            IsInReadOnlyMode = !IsInReadOnlyMode;
        }

        private bool CanChangeMode()
        {
            return currentPatient != null && securityService.HasPrivilege(Privileges.EditAssignments);
        }

        private RoomViewModel selectedRoom;

        public RoomViewModel SelectedRoom
        {
            get { return selectedRoom; }
            set
            {
                Set("SelectedRoom", ref selectedRoom, value);
                IsRoomSelected = selectedRoom != null && !ReferenceEquals(selectedRoom, unseletedRoom);
                UpdateRoomFilter();
            }
        }

        private bool isRoomSelected;

        public bool IsRoomSelected
        {
            get { return isRoomSelected; }
            private set { Set("IsRoomSelected", ref isRoomSelected, value); }
        }

        private RecordType selectedRecordType;

        public RecordType SelectedRecordType
        {
            get { return selectedRecordType; }
            set
            {
                Set("SelectedRecordType", ref selectedRecordType, value);
                IsRecordTypeSelected = selectedRecordType != null && !ReferenceEquals(selectedRecordType, unselectedRecordType);
                UpdateRoomFilter();
            }
        }

        private bool isRecordTypeSelected;

        public bool IsRecordTypeSelected
        {
            get { return isRecordTypeSelected; }
            private set { Set("IsRecordTypeSelected", ref isRecordTypeSelected, value); }
        }

        private PersonViewModel currentPatient;

        public PersonViewModel CurrentPatient
        {
            get { return currentPatient; }
            set
            {
                currentPatient = value;
                CurrentPatientAssignmentsViewModel.CurrentPatient = value;
                ChangeModeCommand.RaiseCanExecuteChanged();
            }
        }

        private void UpdateRoomFilter()
        {
            FilteredRooms.Filter = FilterRooms;
            NoRoomIsFound = Rooms.Any() && FilteredRooms.IsEmpty;
            ClearScheduleGrid();
            BuildScheduleGrid();
        }

        private void BuildScheduleGrid()
        {
            foreach (var room in rooms)
            {
                room.BuildScheduleGrid(selectedDate, isRoomSelected ? (int?)selectedRoom.Id : null, isRecordTypeSelected ? (int?)selectedRecordType.Id : null);
            }
            UpdateAssignmentsReadOnlyMode();
            log.Info("Schedule grid is built");
        }

        private void ClearScheduleGrid()
        {
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
            return (!isRoomSelected || room.Id == selectedRoom.Id) && (!isRecordTypeSelected || room.AllowsRecordType(selectedRecordType.Id));
        }

        #endregion

        #region Time tickers

        private IEnumerable<TimeTickerViewModel> timeTickers;

        public IEnumerable<TimeTickerViewModel> TimeTickers
        {
            get { return timeTickers; }
            set { Set("TimeTickers", ref timeTickers, value); }
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
    }
}
