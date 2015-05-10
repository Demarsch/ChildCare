using System;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Registry
{
    public class ScheduleEditorRoomDayViewModel : ObservableObject
    {
        private readonly IDialogService dialogService;

        public ScheduleEditorRoomDayViewModel(int roomId, int dayOfWeek, IDialogService dialogService)
        {
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            this.dialogService = dialogService;
            RoomId = roomId;
            DayOfWeek = dayOfWeek;
            RecordTypes = new ObservalbeCollectionEx<ScheduleEditorRecordTypeViewModel>();
            AddRecordTypeCommand = new RelayCommand(AddRecordType);
        }

        public int RoomId { get; private set; }

        public int DayOfWeek { get; private set; }

        public ObservalbeCollectionEx<ScheduleEditorRecordTypeViewModel> RecordTypes { get; private set; }

        public RelayCommand AddRecordTypeCommand { get; private set; }

        private void AddRecordType()
        {
            dialogService.ShowMessage("В появившемся окне можно выбрать рекорд тайп!");
        }
    }
}