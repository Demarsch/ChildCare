using System;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class ScheduleEditorRoomViewModel : ObservableObject
    {
        private readonly Room room;

        public ScheduleEditorRoomViewModel(Room room)
        {
            if (room == null)
            {
                throw new ArgumentNullException("room");
            }
            this.room = room;
            Days = new []
            {
                new ScheduleEditorRoomDayViewModel(room.Id, 1),
                new ScheduleEditorRoomDayViewModel(room.Id, 2),
                new ScheduleEditorRoomDayViewModel(room.Id, 3),
                new ScheduleEditorRoomDayViewModel(room.Id, 4),
                new ScheduleEditorRoomDayViewModel(room.Id, 5),
                new ScheduleEditorRoomDayViewModel(room.Id, 6),
                new ScheduleEditorRoomDayViewModel(room.Id, 7), 
            };
        }

        public string Name { get { return room.Name; } }

        public string Number { get { return room.Number; } }

        public ScheduleEditorRoomDayViewModel[] Days { get; private set; }
    }
}