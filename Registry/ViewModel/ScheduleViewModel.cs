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
            selectedDate = new DateTime(2015, 4, 1);
            //TODO: remove this fake data
            TimeTickerStep = TimeSpan.FromMinutes(30.0);
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
                OpenTime = workingTimes.Count == 0 ? TimeSpan.Zero : workingTimes.Min(x => x.Min(y => y.From));
                CloseTime = workingTimes.Count == 0 ? WorkingTime.MaxTime : workingTimes.Max(x => x.Max(y => y.To));
                foreach (var room in rooms)
                    room.SetAssignments(assignments[room.Id]);
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
                Rooms = new ObservableCollection<RoomViewModel>(roomList.Select(x => new RoomViewModel(x, cacheService)));
                RoomsAreLoaded = true;
            }
            catch (Exception ex)
            {
                log.Error("Failed to load rooms from database", ex);
                FailReason = "При попытке загрузить список кабинетов возникла ошибка. Попробуйте перезапустить приложение. Если ошибка повторится, обратитесь в службу поддержки";
            }
            finally
            {
                BusyStatus = null;
            }
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
            set { Set("SelectedDate", ref selectedDate, value); }
        }

        private ObservableCollection<RoomViewModel> rooms;

        public ObservableCollection<RoomViewModel> Rooms
        {
            get { return rooms; }
            set { Set("Rooms", ref rooms, value); }
        }

        #region Time tickers

        private IEnumerable<TimeSpan> timeTickers;

        public IEnumerable<TimeSpan> TimeTickers
        {
            get { return timeTickers; }
            set { Set("TimeTickers", ref timeTickers, value); }
        }

        private TimeSpan openTime;

        public TimeSpan OpenTime
        {
            get { return openTime; }
            set
            {
                if (Set("OpenTime", ref openTime, value))
                    UpdateTimeTickers();
            }
        }

        private TimeSpan closeTime;

        public TimeSpan CloseTime
        {
            get { return closeTime; }
            set
            {
                if (Set("CloseTime", ref closeTime, value))
                    UpdateTimeTickers();
            }
        }

        private TimeSpan timeTickerStep;

        public TimeSpan TimeTickerStep
        {
            get { return timeTickerStep; }
            set
            {
                if (Set("TimeTickerStep", ref timeTickerStep, value))
                    UpdateTimeTickers();
            }
        }

        private void UpdateTimeTickers()
        {
            if (openTime < TimeSpan.Zero
                || closeTime < TimeSpan.Zero
                || openTime >= closeTime
                || timeTickerStep <= TimeSpan.Zero)
            {
                TimeTickers = new TimeSpan[0];
                return;
            }
            var result = new List<TimeSpan>();
            var currentTime = openTime;
            while (currentTime < closeTime)
            {
                result.Add(currentTime);
                currentTime = currentTime.Add(timeTickerStep);
            }
            TimeTickers = result;
        }

        #endregion
    }
}
