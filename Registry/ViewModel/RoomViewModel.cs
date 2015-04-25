using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core;
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
            assignments = new ScheduledAssignmentViewModel[0];
        }

        public int Id { get { return room.Id; } }

        public string Number { get { return room.Number; } }

        public string Name { get { return room.Name; } }

        private IEnumerable<ScheduledAssignmentViewModel> assignments;

        public IEnumerable<ScheduledAssignmentViewModel> Assignments
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
    }
}