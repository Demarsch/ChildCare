using Core.Data;
using System.Data.Entity;
using log4net;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;
using Core.Wpf.Mvvm;
using ScheduleModule.Services;
using ScheduleModule.DTO;
using Core.Services;
using Core.Misc;
using System.Windows.Input;
using Prism.Commands;
using ScheduleModule.Misc;
using Core.Data.Misc;
using System.Threading;

namespace ScheduleModule.ViewModels
{
    public class MultiAssignRecordTypeViewModel : BindableBase, IDisposable
    {
        #region Fields
        private readonly ILog log;

        private readonly IScheduleService scheduleService;

        private readonly ICacheService cacheService;

        private IList<RecordTypeDateTimeInterval> selectedDateTimes;

        private CancellationTokenSource cancellationTokenSource;

        private IList<DateTime> dateTimes;

        private readonly CommonIdName unselectedDoctor = new CommonIdName { Name = "Врач не выбран", Id = SpecialValues.NonExistingId };
        private readonly CommonIdName unselectedRoom = new CommonIdName { Name = "Кабинет не выбран", Id = SpecialValues.NonExistingId };
        #endregion

        #region Constructors
        public MultiAssignRecordTypeViewModel(IScheduleService scheduleService, ICacheService cacheService, ILog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (scheduleService == null)
            {
                throw new ArgumentNullException("scheduleService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.cacheService = cacheService;
            this.scheduleService = scheduleService;
            this.log = log;
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            addSelectedTimeCommand = new DelegateCommand<RecordTypeDateTimeInterval>(AddSelectedTime, CanAddSelectedTime);
            dateTimes = new List<DateTime>();
            selectedDateTimes = new List<RecordTypeDateTimeInterval>();
            SelectedTimes = new List<RecordTypeDateTimeInterval>();
            Rooms = new ObservableCollectionEx<CommonIdName>();
            Dates = new ObservableCollectionEx<ScheduleDateTimesDTO>();
            SelectedDoctorId = SpecialValues.NonExistingId;
            SelectedRoomId = SpecialValues.NonExistingId;
        }

        #endregion

        #region Properties
        public RecordType RecordType { get; private set; }
        public DateTime Date { get; private set; }

        private string recordTypeName;
        public string RecordTypeName
        {
            get { return recordTypeName; }
            set { SetProperty(ref recordTypeName, value); }
        }

        private int selectedDoctorId;
        public int SelectedDoctorId
        {
            get { return selectedDoctorId; }
            set { SetProperty(ref selectedDoctorId, value); }
        }

        private int selectedRoomId = int.MinValue;
        public int SelectedRoomId
        {
            get { return selectedRoomId; }
            set
            {
                SetProperty(ref selectedRoomId, value);
                if (RecordType != null && SelectedRoomId != int.MinValue)
                    LoadScheduleTimes(dateTimes, selectedRoomId, RecordType.IsAnalyse);
            }
        }

        public ObservableCollectionEx<CommonIdName> Rooms { get; private set; }

        private IEnumerable<WorkingTimeViewModel> workingTimes;
        public IEnumerable<WorkingTimeViewModel> WorkingTimes
        {
            get { return workingTimes; }
            set { SetProperty(ref workingTimes, value); }
        }

        public ObservableCollectionEx<ScheduleDateTimesDTO> Dates { get; private set; }
        public IList<RecordTypeDateTimeInterval> SelectedTimes { get; private set; }

        public BusyMediator BusyMediator { get; set; }
        public FailureMediator FailureMediator { get; set; }
        #endregion

        #region Events
        public event EventHandler<SelectedTimesEventArg> SelectedTimesChanged;
        #endregion

        #region Methods

        public async void LoadScheduleTimes(IList<DateTime> dates, int roomId, bool isAnalyse)
        {
            BusyMediator.Activate("Загрузка данных...");
            log.Info(String.Format("Loading schedule for roomId = {0}, from date {1}...", roomId, dates[0]));
            ClearSelectedTimes();
            selectedDateTimes.Clear();
            Dates.Clear();
            try
            {
                var selectedRoom = scheduleService.GetRooms().FirstOrDefault(x => x.Id == roomId);
                foreach (var date in dates)
                {
                    var task = scheduleService.GetAvailiableTimeSlots(date, RecordType, selectedRoom, !isAnalyse);
                    var availiableTimesTaskResult = await task;
                    Dates.Add(new ScheduleDateTimesDTO()
                    {
                        Date = date,
                        Times = availiableTimesTaskResult.GroupBy(x => x.Value).Select(x => new RecordTypeDateTimeInterval()
                        {
                            RecordType = this.RecordType,
                            Date = date,
                            StartTime = x.Key.StartTime,
                            EndTime = x.Key.EndTime,
                            RoomIds = x.Select(y => y.Key).ToArray()
                        }).Distinct().ToArray()
                    });
                }
                log.Info(String.Format("Loading for schedule for roomId = {0}, from date {1} is completed!", roomId, dates[0]));
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Loading for schedule for roomId = {0}, from date {1} is completed!", roomId, dates[0]);
                FailureMediator.Activate("Ошибка при попытке получить расписание для услуги", null, ex, true);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        public async Task Initialize(RecordType recordType, IList<DateTime> dates)
        {
            BusyMediator.Activate("Загрузка данных...");
            if (recordType == null) return;
            log.Info(String.Format("Initializing MultiAssignRecordType for recordTypeId = {0} - {1}, from date {2}...", recordType.Id, recordType.Name, dates[0]));
            RecordType = recordType;
            RecordTypeName = recordType.Name;
            dateTimes = dates;

            Rooms.Clear();

            if (cancellationTokenSource == null)
                cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            DisposableQueryable<Room> roomsQuery = null;
            Rooms.Add(unselectedRoom);
            try
            {
                var resRooms = await scheduleService.GetRoomsforRecordType(recordType.Id, dates).Select(x => new CommonIdName { Id = x.Id, Name = x.Number + " - " + x.Name }).ToArrayAsync();
                Rooms.AddRange(resRooms);
                if (SelectedRoomId == SpecialValues.NonExistingId)
                    SelectedRoomId = int.MinValue;
                SelectedRoomId = SpecialValues.NonExistingId;
                //LoadScheduleTimes(dates, SelectedRoomId, recordType.IsAnalyse);
            }
            catch (Exception ex)
            {
                log.ErrorFormatEx(ex, "Initializing MultiAssignRecordType for recordTypeId = {0} - {1}, from date {2} is completed!", recordType.Id, recordType.Name, dates[0]);
                FailureMediator.Activate("Ошибка при попытке получить расписание для услуги", null, ex, true);
            }
            finally
            {
                BusyMediator.Deactivate();
                if (roomsQuery != null)
                    roomsQuery.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var time in SelectedTimes)
            {
                if (SelectedTimesChanged != null)
                    SelectedTimesChanged(this, new SelectedTimesEventArg() { Date = time, IsAdded = false });
            }
        }

        private void ClearSelectedTimes()
        {
            for (int i = SelectedTimes.Count - 1; i >= 0; i--)
            {
                if (SelectedTimesChanged != null)
                    SelectedTimesChanged(this, new SelectedTimesEventArg() { Date = SelectedTimes[i], IsAdded = false });
                SelectedTimes.Remove(SelectedTimes[i]);
            }
        }

        public void CheckCanAddSelectedTime(IList<RecordTypeDateTimeInterval> selectedItems)
        {
            selectedDateTimes = selectedItems;
            addSelectedTimeCommand.RaiseCanExecuteChanged();
        }

        private bool CanAddSelectedTime(RecordTypeDateTimeInterval dateTime)
        {
            return /*selectedDateTimes.Any(x => dateTime.RecordType == x.RecordType && dateTime.Date == x.Date && x.EndTime == dateTime.StartTime && x.StartTime == dateTime.EndTime) ||*/
                !selectedDateTimes.Any(x => dateTime.RecordType != x.RecordType && dateTime.Date == x.Date && x.EndTime > dateTime.StartTime && x.StartTime < dateTime.EndTime);
        }

        private void AddSelectedTime(RecordTypeDateTimeInterval dateTime)
        {
            bool isAdded = false;
            if (SelectedTimes.Contains(dateTime))
                SelectedTimes.Remove(dateTime);
            else
            {
                SelectedTimes.Add(dateTime);
                isAdded = true;
            }
            if (SelectedTimesChanged != null)
                SelectedTimesChanged(this, new SelectedTimesEventArg() { Date = dateTime, IsAdded = isAdded });
        }

        #endregion

        #region Commands

        private DelegateCommand<RecordTypeDateTimeInterval> addSelectedTimeCommand;
        public ICommand AddSelectedTimeCommand { get { return addSelectedTimeCommand; } }

        #endregion
    }
}
