using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class RoomViewModel : ObservableObject
    {
        private readonly Room room;

        public RoomViewModel(Room room)
        {
            if (room == null)
                throw new ArgumentNullException("room");
            this.room = room;
            openTime = DateTime.Today.AddHours(8.0);
            closeTime = DateTime.Today.AddHours(17.0);
            assignments = new ScheduledAssignmentDTO[0];
            workingTimes = new ScheduleItemViewModel[0];
        }

        public int Id { get { return room.Id; } }

        public string Number { get { return room.Number; } }

        public string Name { get { return room.Name; } }

        private IEnumerable<ScheduledAssignmentDTO> assignments;

        public IEnumerable<ScheduledAssignmentDTO> Assignments
        {
            get { return assignments; }
            set { Set("Assignments", ref assignments, value); }
        }

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

        public string NumberAndName { get { return string.Format("№{0} - {1}", Number, Name); } }

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
    }
}