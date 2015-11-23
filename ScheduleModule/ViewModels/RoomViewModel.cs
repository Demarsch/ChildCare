using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using Core.Data;
using Core.Misc;
using Core.Services;
using Core.Wpf.Mvvm;
using Prism.Mvvm;
using ScheduleModule.Services;

namespace ScheduleModule.ViewModels
{
    public class RoomViewModel : BindableBase
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
            //TODO: these values are used to avoid exception when TimelinePanel.Height becomes negative when bound to the same default DateTime value
            //TODO: probably worth using fallback value on binding side
            openTime = DateTime.Today.AddHours(8.0);
            closeTime = DateTime.Today.AddHours(17.0);
            workingTimes = new WorkingTimeViewModel[0];
            TimeSlots = new ObservableCollectionEx<ITimeInterval>();
        }

        public int Id { get { return room.Id; } }

        public string Number { get { return room.Number; } }

        public string Name { get { return room.Name; } }

        public ObservableCollectionEx<ITimeInterval> TimeSlots { get; private set; }

        private DateTime openTime;

        public DateTime OpenTime
        {
            get { return openTime; }
            set { SetProperty(ref openTime, value); }
        }

        private DateTime closeTime;

        public DateTime CloseTime
        {
            get { return closeTime; }
            set { SetProperty(ref closeTime, value); }
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

        private IEnumerable<WorkingTimeViewModel> workingTimes;

        public IEnumerable<WorkingTimeViewModel> WorkingTimes
        {
            get { return workingTimes; }
            set { SetProperty(ref workingTimes, value); }
        }

        public bool AllowsRecordType(int recordTypeId)
        {
            return workingTimes.Any(x => x.RecordTypeId == recordTypeId);
        }

        public void BuildScheduleGrid(DateTime date, int? selectedRoomId, int? selectedRecordTypeId)
        {
            foreach (var freeTimeSlot in TimeSlots.OfType<FreeTimeSlotViewModel>())
            {
                freeTimeSlot.AssignmentCreationRequested -= FreeTimeSlotOnAssignmentCreationRequested;
            }
            if (!selectedRecordTypeId.HasValue || (selectedRoomId.HasValue && selectedRoomId.Value != Id) || !AllowsRecordType(selectedRecordTypeId.Value))
            {
                TimeSlots.RemoveWhere(x => x is FreeTimeSlotViewModel);
                return;
            }
            var recordType = cacheService.GetItemById<RecordType>(selectedRecordTypeId.Value);
            var availableTimeIntervals = scheduleService.GetAvailableTimeIntervals(
                workingTimes.Where(x => x.RecordTypeId == selectedRecordTypeId.Value),
                TimeSlots.OfType<OccupiedTimeSlotViewModel>(), 
                recordType.Duration, 
                recordType.MinDuration);
            var freeTimeSlots = availableTimeIntervals.Select(x => new FreeTimeSlotViewModel(date.Add(x.StartTime), date.Add(x.EndTime), selectedRecordTypeId.Value, Id)).ToArray();
            foreach (var freeTimeSlot in freeTimeSlots)
                freeTimeSlot.AssignmentCreationRequested += FreeTimeSlotOnAssignmentCreationRequested;
            TimeSlots.AddRange(freeTimeSlots);
        }

        internal void FreeTimeSlotOnAssignmentCreationRequested(object sender, EventArgs eventArgs)
        {
            OnAssignmentCreationRequested(new ReturnEventArgs<FreeTimeSlotViewModel>(sender as FreeTimeSlotViewModel));
        }

        public event EventHandler<ReturnEventArgs<FreeTimeSlotViewModel>> AssignmentCreationRequested;

        protected virtual void OnAssignmentCreationRequested(ReturnEventArgs<FreeTimeSlotViewModel> e)
        {
            var handler = AssignmentCreationRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}