using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Core;
using DataLib;
using GalaSoft.MvvmLight.Command;
using log4net;

namespace Registry
{
    public class ScheduleViewModel : BasicViewModel
    {
        private readonly ILog log;

        private readonly IScheduleService scheduleService;

        public ScheduleViewModel(IScheduleService scheduleService, ILog log)
        {
            if (scheduleService == null)
                throw new ArgumentNullException("scheduleService");
            if (log == null)
                throw new ArgumentNullException("log");
            this.log = log;
            this.scheduleService = scheduleService;
            //TODO: remove this fake data
            selectedDate = new DateTime(2015, 4, 1);
            openTime = selectedDate.AddHours(8.0);
            closeTime = selectedDate.AddHours(17.0);
            isInReadOnlyMode = true;
            changeModeCommand = new RelayCommand(ChangeMode, CanChangeMode);
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
                var workingTimes = await Task<RoomWorkingTimeRepository>.Factory.StartNew(x => scheduleService.GetRoomsWorkingTime((DateTime)x), date);
                var assignments = await Task<ILookup<int, ScheduledAssignmentDTO>>.Factory.StartNew(x => scheduleService.GetRoomsAssignments((DateTime)x), date);
                OpenTime = date.Add(workingTimes.GetOpenTime());
                CloseTime = date.Add(workingTimes.GetCloseTime());
                UpdateTimeTickers();
                foreach (var room in rooms)
                {
                    room.OpenTime = OpenTime;
                    room.CloseTime = CloseTime;
                    room.Assignments = assignments[room.Id].Select(x => new ScheduledAssignmentViewModel(x)).ToArray();
                    //room.WorkingTimes = workingTimes[room.Id];
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
                Rooms = scheduleService.GetRooms().Select(x => new RoomViewModel(x)).ToArray();
                RecordTypes = scheduleService.GetRecordTypes().ToList();
                DataSourcesAreLoaded = true;
            }
            catch (Exception ex)
            {
                log.Error("Failed to load rooms from database", ex);
                FailReason = "При попытке загрузить список кабинетов возникла ошибка. Попробуйте перезапустить приложение. Если ошибка повторится, обратитесь в службу поддержки";
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
            set { Set("DataSourcesAreLoaded", ref dataSourcesAreLoaded, value); }
        }

        private IEnumerable<RoomViewModel> rooms;

        public IEnumerable<RoomViewModel> Rooms
        {
            get { return rooms; }
            set { Set("Rooms", ref rooms, value); }
        }

        private IEnumerable<RecordType> recordTypes;

        public IEnumerable<RecordType> RecordTypes
        {
            get { return recordTypes; }
            set { Set("RecordTypes", ref recordTypes, value); }
        }

        #region Filter

        private DateTime selectedDate;

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                if (Set("SelectedDate", ref selectedDate, value))
                    LoadAssignmentsAsync(selectedDate);
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
                IsRoomSelected = selectedRoom != null;
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
                IsRecordTypeSelected = selectedRecordType != null;
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
