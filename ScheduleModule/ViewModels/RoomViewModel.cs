using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using Core.Data;
using Core.Extensions;
using Core.Misc;
using Core.Wpf.Mvvm;
using Prism.Mvvm;
using ScheduleModule.Services;

namespace ScheduleModule.ViewModels
{
    public class RoomViewModel : BindableBase
    {
        private readonly IScheduleService scheduleService;

        public RoomViewModel(Room room, IScheduleService scheduleService)
        {
            if (room == null)
            {
                throw new ArgumentNullException("room");
            }
            if (scheduleService == null)
            {
                throw new ArgumentNullException("scheduleService");
            }
            this.scheduleService = scheduleService;
            Room = room;
            //TODO: these values are used to avoid exception when TimelinePanel.Height becomes negative when bound to the same default DateTime value
            //TODO: probably worth using fallback value on binding side
            openTime = DateTime.Today.AddHours(8.0);
            closeTime = DateTime.Today.AddHours(17.0);
            workingTimes = new WorkingTimeViewModel[0];
            TimeSlots = new ObservableCollectionEx<ITimeInterval>();
        }

        public Room Room { get; private set; }

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
                if (string.IsNullOrEmpty(Room.Number))
                {
                    return Room.Name;
                }
                return string.Format("№{0} - {1}", Room.Number, Room.Name);
            }
        }

        private IEnumerable<WorkingTimeViewModel> workingTimes;

        public IEnumerable<WorkingTimeViewModel> WorkingTimes
        {
            get { return workingTimes; }
            set { SetProperty(ref workingTimes, value); }
        }

        public bool AllowsRecordType(RecordType recordType)
        {
            if (recordType == null)
            {
                return false;
            }
            return workingTimes.Any(x => x.RecordType.IsSameOrParentOf(recordType));
        }

        public void BuildScheduleGrid(DateTime date, Room selectedRoom, RecordType selectedRecordType)
        {
            foreach (var freeTimeSlot in TimeSlots.OfType<FreeTimeSlotViewModel>())
            {
                freeTimeSlot.AssignmentCreationRequested -= FreeTimeSlotOnAssignmentCreationRequested;
            }
            if (selectedRecordType == null || (selectedRoom != null && selectedRoom != Room) || !AllowsRecordType(selectedRecordType))
            {
                TimeSlots.RemoveWhere(x => x is FreeTimeSlotViewModel);
                return;
            }
            var availableTimeIntervals = scheduleService.GetAvailableTimeIntervals(workingTimes.Where(x => x.RecordType.IsSameOrParentOf(selectedRecordType)),
                                                                                   TimeSlots.OfType<OccupiedTimeSlotViewModel>(),
                                                                                   selectedRecordType.Duration,
                                                                                   selectedRecordType.MinDuration);
            var freeTimeSlots = availableTimeIntervals.Select(x => new FreeTimeSlotViewModel(date.Add(x.StartTime), date.Add(x.EndTime), selectedRecordType, Room)).ToArray();
            foreach (var freeTimeSlot in freeTimeSlots)
            {
                freeTimeSlot.AssignmentCreationRequested += FreeTimeSlotOnAssignmentCreationRequested;
            }
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