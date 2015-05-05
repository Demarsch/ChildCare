using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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

        public ScheduleViewModel(IScheduleService scheduleService, ILog log, ICacheService cacheService, IEnvironment environment, IDialogService dialogService)
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
            this.dialogService = dialogService;
            this.environment = environment;
            this.cacheService = cacheService;
            this.log = log;
            this.scheduleService = scheduleService;
            filteredRooms = new CollectionViewSource();
            //TODO: remove this fake data
            selectedDate = new DateTime(2015, 4, 1);
            openTime = selectedDate.AddHours(8.0);
            closeTime = selectedDate.AddHours(17.0);
            isInReadOnlyMode = true;
            changeModeCommand = new RelayCommand(ChangeMode, CanChangeMode);
            unseletedRoom = new RoomViewModel(new Room { Name = "Выберите кабинет" }, scheduleService, cacheService);
            unselectedRecordType = new RecordType { Name = "Выберите услугу" };
            LoadDataSources();
            LoadAssignmentsAsync(selectedDate);
        }

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
                var workingTimes = (await Task<ICollection<ScheduleItemDTO>>.Factory.StartNew(x => scheduleService.GetRoomsWorkingTime((DateTime)x), date)).ToLookup(x => x.RoomId);
                var assignments = await Task<ILookup<int, ScheduledAssignmentDTO>>.Factory.StartNew(x => scheduleService.GetRoomsAssignments((DateTime)x), date);
                if (workingTimes.Count == 0)
                {
                    //TODO: store these settings in DB. This are just default values used for displaying purposes
                    OpenTime = date.AddHours(8.0);
                    closeTime = date.AddHours(17.0);
                }
                else
                {
                    OpenTime = date.Add(workingTimes.Min(x => x.Min(y => y.StartTime)));
                    CloseTime = date.Add(workingTimes.Max(x => x.Max(y => y.EndTime)));
                }
                UpdateTimeTickers();
                foreach (var room in rooms)
                {
                    room.OpenTime = OpenTime;
                    room.CloseTime = CloseTime;
                    foreach (var occupiedTimeSlot in room.TimeSlots.OfType<OccupiedTimeSlotViewModel>())
                    {
                        occupiedTimeSlot.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                    }
                    room.TimeSlots.Replace(assignments[room.Id].Select(x => new OccupiedTimeSlotViewModel(x, environment)));
                    foreach (var occupiedTimeSlot in room.TimeSlots.OfType<OccupiedTimeSlotViewModel>())
                    {
                        occupiedTimeSlot.CancelOrDeleteRequested += RoomOnAssignmentCancelOrDeleteRequested;
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
                Rooms = new[] { unseletedRoom }.Concat(scheduleService.GetRooms().Select(x => new RoomViewModel(x, scheduleService, cacheService))).ToArray();
                foreach (var room in Rooms)
                    room.AssignmentCreationRequested += RoomOnAssignmentCreationRequested;
                RecordTypes = new[] { unselectedRecordType }.Concat(scheduleService.GetRecordTypes()).ToArray();
                DataSourcesAreLoaded = true;
            }
            catch (Exception ex)
            {
                log.Error("Failed to load datasources for ScheduleViewModel from database", ex);
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
            private set { Set("RecordTypes", ref recordTypes, value); }
        }

        #region Assignments
        //TODO: worth reviewing this code to probably get rid of all type of assignment objects
        private void RoomOnAssignmentCreationRequested(object sender, ReturnEventArgs<FreeTimeSlotViewModel> e)
        {
            var financingSource = GetFinancingSource();
            if (financingSource == 0)
            {
                log.Error("There are no financing sources in database");
                dialogService.ShowError("В базе данных отсутствует информация о доступных источниках финансирования. Обратитесь в службу поддержки");
                return;
            }
            var selectedRoom = sender as RoomViewModel;
            var assignment = new Assignment
            {
                AssignDateTime = e.Result.StartTime,
                Duration = (int)e.Result.EndTime.Subtract(e.Result.StartTime).TotalMinutes,
                AssignUserId = environment.CurrentUser.UserId,
                Note = string.Empty,
                FinancingSourceId = financingSource,
                PersonId = currentPatient.Id,
                RecordTypeId = e.Result.RecordTypeId,
                RoomId = selectedRoom.Id,
                IsTemporary = true,
            };
            try
            {
                scheduleService.SaveAssignment(assignment);
            }
            catch (Exception ex)
            {
                log.Error("Failed to save new assignment", ex);
                dialogService.ShowError(string.Format("Не удалось создать назначение. Причина - {0}{1}Попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки", ex.Message, Environment.NewLine));
                return;
            }
            var newAssignmentDTO = new ScheduledAssignmentDTO
            {
                Id = assignment.Id,
                IsCompleted = false,
                PersonShortName = currentPatient.ShortName,
                RecordTypeId = assignment.RecordTypeId,
                RoomId = assignment.RoomId,
                StartTime = assignment.AssignDateTime,
                Duration = assignment.Duration.Value,
                IsTemporary = true,
                AssignUserId = assignment.AssignUserId
            };
            var newAssignment = new OccupiedTimeSlotViewModel(newAssignmentDTO, environment);
            newAssignment.CancelOrDeleteRequested += RoomOnAssignmentCancelOrDeleteRequested;
            selectedRoom.TimeSlots.Remove(e.Result);
            selectedRoom.TimeSlots.Add(newAssignment);
            var isFailed = false;
            do
            {
                var viewModel = new ScheduleAssignmentUpdateViewModel(cacheService.GetItems<FinacingSource>());
                viewModel.SelectedFinancingSource = viewModel.FinacingSources.First(x => x.Id == assignment.FinancingSourceId);
                var dialogResult = dialogService.ShowDialog(viewModel);
                if (dialogResult != true)
                {
                    try
                    {
                        isFailed = false;
                        scheduleService.DeleteAssignment(assignment.Id);
                        selectedRoom.TimeSlots.Remove(newAssignment);
                        selectedRoom.TimeSlots.Add(e.Result);
                        newAssignment.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Failed to manually delete temporary assignment (Id = {0})", assignment.Id), ex);
                        dialogService.ShowError("Не удалось удалить временное назначение. Попробуйте удалить его вручную через контекстное меню или оставьте его, и через некоторое время оно будет удалено автоматически");
                    }
                }
                else
                {
                    try
                    {
                        assignment.IsTemporary = false;
                        assignment.FinancingSourceId = viewModel.SelectedFinancingSource.Id;
                        assignment.Note = viewModel.Note;
                        scheduleService.SaveAssignment(assignment);
                        newAssignmentDTO.IsTemporary = false;
                        newAssignment.TryUpdate(newAssignmentDTO);
                        isFailed = false;
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
                    scheduleService.DeleteAssignment(assignment.Id);
                    assignment.CancelOrDeleteRequested -= RoomOnAssignmentCancelOrDeleteRequested;
                    rooms.First(x => x.Id == assignment.RoomId).TimeSlots.Remove(assignment);
                    ClearScheduleGrid();
                    BuildScheduleGrid();
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
                    scheduleService.CancelAssignment(assignment.Id);
                    rooms.First(x => x.Id == assignment.RoomId).TimeSlots.Remove(assignment);
                    ClearScheduleGrid();
                    BuildScheduleGrid();
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed to cancel assignment (Id = {0})", assignment.Id), ex);
                    dialogService.ShowError("Не удалось отменить назначение. Пожалуйста, попробуйте еще раз. Если ошибка повторится, обратитесь в службу поддержки");
                }
            }
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
                    if (!IsInReadOnlyMode)
                    {
                        ClearScheduleGrid();
                    }
                    LoadAssignmentsAsync(selectedDate)
                        .ContinueWith(x =>
                        {
                            if (!IsInReadOnlyMode)
                            {
                                BuildScheduleGrid();
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

        private readonly RelayCommand changeModeCommand;

        public ICommand ChangeModeCommand { get { return changeModeCommand; } }

        private void ChangeMode()
        {
            IsInReadOnlyMode = !IsInReadOnlyMode;
        }

        private bool CanChangeMode()
        {
            return currentPatient != null;
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
                changeModeCommand.RaiseCanExecuteChanged();
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
                room.BuildScheduleGrid(selectedDate, isRoomSelected ? (int?)selectedRoom.Id : null, isRecordTypeSelected ? (int?)selectedRecordType.Id : null);
        }

        private void ClearScheduleGrid()
        {
            foreach (var room in rooms)
                room.TimeSlots.RemoveWhere(x => x is FreeTimeSlotViewModel);
        }

        private bool FilterRooms(object obj)
        {
            var room = obj as RoomViewModel;
            if (room == null || ReferenceEquals(room, unseletedRoom))
                return false;
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
