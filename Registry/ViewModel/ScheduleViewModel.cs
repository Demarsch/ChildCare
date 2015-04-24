using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Core;
using DataLib;
using log4net;

namespace Registry
{
    public class ScheduleViewModel : BasicViewModel
    {
        private readonly ILog log;

        private readonly IScheduleService scheduleService;

        private readonly ICacheService cacheService;

        public ScheduleViewModel(IScheduleService scheduleService, ICacheService cacheService, ILog log)
        {
            if (scheduleService == null)
                throw new ArgumentNullException("scheduleService");
            if (log == null)
                throw new ArgumentNullException("log");
            if (cacheService == null)
                throw new ArgumentNullException("cacheService");
            this.cacheService = cacheService;
            this.log = log;
            this.scheduleService = scheduleService;
            //TODO: remove this fake data
            selectedDate = new DateTime(2015, 4, 1);
            openTime = selectedDate.AddHours(8.0);
            closeTime = selectedDate.AddHours(17.0);
            LoadRoomsAsync().ContinueWith((x, date) => LoadAssignmentsAsync((DateTime)date), selectedDate);
        }

        private async Task LoadAssignmentsAsync(DateTime date)
        {
            if (!RoomsAreLoaded)
                return;
            try
            {
                BusyStatus = "Загрузка расписания...";
                FailReason = null;
                await Task.Delay(TimeSpan.FromSeconds(1.0));
                var workingTimes = await Task<ILookup<int, WorkingTime>>.Factory.StartNew(x => scheduleService.GetRoomsWorkingTime((DateTime)x), date);
                var assignments = await Task<ILookup<int, ScheduledAssignmentDTO>>.Factory.StartNew(x => scheduleService.GetRoomsAssignments((DateTime)x), date);
                OpenTime = workingTimes.Count == 0 ? date.Date : workingTimes.Min(x => x.Min(y => y.From));
                CloseTime = workingTimes.Count == 0 ? date.Date.AddDays(1.0).AddMinutes(-1.0) : workingTimes.Max(x => x.Max(y => y.To));
                UpdateTimeTickers();
                foreach (var room in rooms)
                {
                    room.OpenTime = OpenTime;
                    room.CloseTime = CloseTime;
                    room.Assignments = assignments[room.Id].Select(x => new ScheduledAssignmentViewModel(x)).ToArray();
                    room.WorkingTimes = workingTimes[room.Id];
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

        private async Task LoadRoomsAsync()
        {
            try
            {
                BusyStatus = "Загрузка списка кабинетов...";
                await Task.Delay(TimeSpan.FromSeconds(1.0));
                var roomList = await Task<ICollection<Room>>.Factory.StartNew(() => scheduleService.GetRooms());
                Rooms = roomList.Select(x => new RoomViewModel(x)).ToArray();
                RoomsAreLoaded = true;
            }
            catch (Exception ex)
            {
                log.Error("Failed to load rooms from database", ex);
                FailReason = "При попытке загрузить список кабинетов возникла ошибка. Попробуйте перезапустить приложение. Если ошибка повторится, обратитесь в службу поддержки";
                BusyStatus = null;
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

        private bool roomsAreLoaded;

        public bool RoomsAreLoaded
        {
            get { return roomsAreLoaded; }
            set { Set("RoomsAreLoaded", ref roomsAreLoaded, value); }
        }

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

        private IEnumerable<RoomViewModel> rooms;

        public IEnumerable<RoomViewModel> Rooms
        {
            get { return rooms; }
            set { Set("Rooms", ref rooms, value); }
        }

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
