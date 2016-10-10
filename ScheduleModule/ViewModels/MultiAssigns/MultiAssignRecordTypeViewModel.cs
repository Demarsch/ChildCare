using Core.Data;
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
            addSelectedTimeCommand = new DelegateCommand<ITimeInterval>(AddSelectedTime, CanAddSelectedTime);
            SelectedTimes = new List<ITimeInterval>();
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
        public IList<ITimeInterval> SelectedTimes { get; private set; }
        #endregion

        #region Events
        public event EventHandler<SelectedTimesEventArg> SelectedTimesChanged;
        #endregion

        #region Methods
        public async Task Initialize(RecordType recordType, IList<DateTime> dates, Room room = null)
        {
            RecordType = recordType;
            RecordTypeName = recordType.Name;
            Dates.Clear();
            foreach (var date in dates)
            {
                var task = scheduleService.GetAvailiableTimeSlots(date, recordType, room);
                var availiableTimesTaskResult = await task;
                Dates.Add(new ScheduleDateTimesDTO() { Date = date, Times = availiableTimesTaskResult });
            }
        }

        public void Dispose()
        {

        }

        private bool CanAddSelectedTime(ITimeInterval timeInterval)
        {
            return true;
        }

        private void AddSelectedTime(ITimeInterval timeInterval)
        {
            bool isAdded = false;
            if (SelectedTimes.Contains(timeInterval))
                SelectedTimes.Remove(timeInterval);
            else
            {
                SelectedTimes.Add(timeInterval);
                isAdded = true;
            }
            if (SelectedTimesChanged != null)
                SelectedTimesChanged(this, new SelectedTimesEventArg() { TimeInterval = timeInterval, RecordType = RecordType, IsAdded = isAdded });
        }
        #endregion

        #region Commands

        private DelegateCommand<ITimeInterval> addSelectedTimeCommand;
        public ICommand AddSelectedTimeCommand { get { return addSelectedTimeCommand; } }

        #endregion
    }
}
