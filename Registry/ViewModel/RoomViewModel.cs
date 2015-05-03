using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using Core;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class RoomViewModel : ObservableObject
    {
        private readonly Room room;

        private readonly IScheduleService scheduleService;

        private readonly ICacheService cacheService;

        public RoomViewModel(Room room, IScheduleService scheduleService, ICacheService cacheService)
        {
            if (room == null)
            {
                throw new ArgumentNullException("room");
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
            this.room = room;
            //TODO: these values are used to avoid exception when TimelinePanel.Height becomes there when bound to the same default DateTime value
            //TODO: probably worth using fallback value on binding side
            openTime = DateTime.Today.AddHours(8.0);
            closeTime = DateTime.Today.AddHours(17.0);
            workingTimes = new ScheduleItemViewModel[0];
            TimeSlots = new ObservalbeCollectionEx<ITimeInterval>();
        }

        public int Id { get { return room.Id; } }

        public string Number { get { return room.Number; } }

        public string Name { get { return room.Name; } }

        public ObservalbeCollectionEx<ITimeInterval> TimeSlots { get; private set; }

        private DateTime openTime;

        public DateTime OpenTime
        {
            get { return openTime; }
            set { Set("OpenTime", ref openTime, value); }
        }

        private DateTime closeTime;

        public DateTime CloseTime
        {
            get { return closeTime; }
            set { Set("CloseTime", ref closeTime, value); }
        }

        public string NumberAndName
        {
            get
            {
                if (Number == null)
                    return Name;
                return string.Format("№{0} - {1}", Number, Name);
            }
        }

        private IEnumerable<ScheduleItemViewModel> workingTimes;

        public IEnumerable<ScheduleItemViewModel> WorkingTimes
        {
            get { return workingTimes; }
            set { Set("WorkingTimes", ref workingTimes, value); }
        }

        public bool AllowsRecordType(int recordTypeId)
        {
            return workingTimes.Any(x => x.RecordTypeId == recordTypeId);
        }

        public void BuildScheduleGrid(DateTime date, int? selectedRoomId, int? selectedRecordTypeId)
        {
            foreach (var freeTimeSlot in TimeSlots.OfType<ScheduleCellViewModel>())
            {
                freeTimeSlot.AssignmentCreationRequested -= ScheduleCellOnAssignmentCreationRequested;
            }
            if (!selectedRecordTypeId.HasValue || (selectedRoomId.HasValue && selectedRoomId.Value != Id) || !AllowsRecordType(selectedRecordTypeId.Value))
            {
                TimeSlots.RemoveWhere(x => x is ScheduleCellViewModel);
                return;
            }
            var recordType = cacheService.GetItemById<RecordType>(selectedRecordTypeId.Value);
            var availableTimeIntervals = scheduleService.GetAvailableTimeIntervals(
                workingTimes.Where(x => x.RecordTypeId == selectedRecordTypeId.Value),
                TimeSlots.OfType<ScheduledAssignmentViewModel>(), 
                recordType.Duration, 
                recordType.MinDuration);
            var freeTimeSlots = availableTimeIntervals.Select(x => new ScheduleCellViewModel(date.Add(x.StartTime), date.Add(x.EndTime), selectedRecordTypeId.Value)).ToArray();
            foreach (var freeTimeSlot in freeTimeSlots)
                freeTimeSlot.AssignmentCreationRequested += ScheduleCellOnAssignmentCreationRequested;
            TimeSlots.AddRange(freeTimeSlots);
        }

        private void ScheduleCellOnAssignmentCreationRequested(object sender, EventArgs eventArgs)
        {
            OnAssignmentCreationRequested(new ReturnEventArgs<ScheduleCellViewModel>(sender as ScheduleCellViewModel));
        }

        public event EventHandler<ReturnEventArgs<ScheduleCellViewModel>> AssignmentCreationRequested;

        protected virtual void OnAssignmentCreationRequested(ReturnEventArgs<ScheduleCellViewModel> e)
        {
            var handler = AssignmentCreationRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}