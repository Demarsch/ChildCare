using System;
using Core;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class ScheduleEditorRoomViewModel : ObservableObject
    {
        private readonly Room room;

        public ScheduleEditorRoomViewModel(Room room, IDialogService dialogService)
        {
            if (room == null)
            {
                throw new ArgumentNullException("room");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            this.room = room;
            Days = new []
            {
                new ScheduleEditorRoomDayViewModel(room.Id, 1, dialogService),
                new ScheduleEditorRoomDayViewModel(room.Id, 2, dialogService),
                new ScheduleEditorRoomDayViewModel(room.Id, 3, dialogService),
                new ScheduleEditorRoomDayViewModel(room.Id, 4, dialogService),
                new ScheduleEditorRoomDayViewModel(room.Id, 5, dialogService),
                new ScheduleEditorRoomDayViewModel(room.Id, 6, dialogService),
                new ScheduleEditorRoomDayViewModel(room.Id, 7, dialogService), 
            };
        }

        public int Id { get { return room.Id; } }

        public string Name { get { return room.Name; } }

        public string Number { get { return room.Number; } }

        public ScheduleEditorRoomDayViewModel[] Days { get; private set; }
    }
}