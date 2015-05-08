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
        }

        public string Name { get { return room.Name; } }
    }
}