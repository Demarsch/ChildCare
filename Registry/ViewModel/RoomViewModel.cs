using System;
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
        }

        public string Number { get { return room.Number; } }

        public string Name { get { return room.Name; } }
    }
}