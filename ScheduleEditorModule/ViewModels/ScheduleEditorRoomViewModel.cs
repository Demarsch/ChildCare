using System;
using System.Windows.Input;
using Core.Data;
using Prism.Commands;
using Prism.Mvvm;

namespace ScheduleEditorModule.ViewModels
{
    public class ScheduleEditorRoomViewModel : BindableBase, IDisposable
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
            CloseRoomThisWeekCommand = new DelegateCommand(CloseRoomThisWeek, CanCloseRoomThisWeek);
            CloseRoomCommand = new DelegateCommand(CloseRoom, CanCloseRoom);
        }

        public int Id { get { return room.Id; } }

        public string Name { get { return room.Name; } }

        public string Number { get { return "№" + room.Number; } }

        public ScheduleEditorRoomDayViewModel[] Days { get; private set; }

        public ICommand CloseRoomThisWeekCommand { get; private set; }

        private void CloseRoomThisWeek()
        {
            Close(true);
        }

        private bool CanCloseRoomThisWeek()
        {
            return CanClose(true);
        }

        public ICommand CloseRoomCommand { get; private set; }

        private void CloseRoom()
        {
            Close(false);
        }

        private bool CanCloseRoom()
        {
            return CanClose(false);
        }

        private void Close(bool thisWeek)
        {
            foreach (var day in Days)
            {
                day.CloseCommand.Execute(thisWeek);
            }
        }

        private bool CanClose(bool thisWeek)
        {
            var result = false;
            foreach (var day in Days)
            {
                result = result || day.CloseCommand.CanExecute(thisWeek);
            }
            return result;
        }

        public ICommand CopyCommand { get; set; }

        public ICommand PasteCommand { get; set; }

        public void Dispose()
        {
            foreach (var day in Days)
            {
                day.Dispose();
            }
        }
    }
}