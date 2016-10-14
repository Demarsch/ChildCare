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

namespace ScheduleModule.ViewModels
{
    public class MultiAssignRecordTypeViewModel : BindableBase, IDisposable
    {
        #region Fields
        private readonly ILog log;

        private readonly IScheduleService scheduleService;

        private readonly ICacheService cacheService;

        private IList<RecordTypeDateTimeInterval> selectedDateTimes;

        private readonly string unselectedDoctorName = "Врач не выбран";
        private readonly string unselectedRoomName = "Кабинет не выбран";
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
            addSelectedTimeCommand = new DelegateCommand<RecordTypeDateTimeInterval>(AddSelectedTime, CanAddSelectedTime);
            selectedDateTimes = new List<RecordTypeDateTimeInterval>();
            SelectedTimes = new List<RecordTypeDateTimeInterval>();
            Dates = new ObservableCollectionEx<ScheduleDateTimesDTO>();
            SelectedDoctorName = unselectedDoctorName;
            SelectedRoomName = unselectedRoomName;
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

        private string selectedDoctorName;
        public string SelectedDoctorName
        {
            get { return selectedDoctorName; }
            set { SetProperty(ref selectedDoctorName, value); }
        }

        private string selectedRoomName;
        public string SelectedRoomName
        {
            get { return selectedRoomName; }
            set { SetProperty(ref selectedRoomName, value); }
        }

        private IEnumerable<WorkingTimeViewModel> workingTimes;
        public IEnumerable<WorkingTimeViewModel> WorkingTimes
        {
            get { return workingTimes; }
            set { SetProperty(ref workingTimes, value); }
        }

        public ObservableCollectionEx<ScheduleDateTimesDTO> Dates { get; private set; }
        public IList<RecordTypeDateTimeInterval> SelectedTimes { get; private set; }
        #endregion

        #region Events
        public event EventHandler<SelectedTimesEventArg> SelectedTimesChanged;
        #endregion

        #region Methods
        public async Task Initialize(RecordType recordType, IList<DateTime> dates, Room room = null)
        {
            RecordType = recordType;
            RecordTypeName = recordType.Name;
            SelectedTimes.Clear();
            selectedDateTimes.Clear();
            Dates.Clear();
            foreach (var date in dates)
            {
                var task = scheduleService.GetAvailiableTimeSlots(date, recordType, room, !recordType.IsAnalyse);
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
                        }).ToArray().Distinct()
                });
            }
        }

        public void Dispose()
        {

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
