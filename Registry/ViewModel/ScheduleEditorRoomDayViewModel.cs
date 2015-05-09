using Core;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class ScheduleEditorRoomDayViewModel : ObservableObject
    {
        public ScheduleEditorRoomDayViewModel(int roomId, int dayOfWeek)
        {
            RoomId = roomId;
            DayOfWeek = dayOfWeek;
        }

        public int RoomId { get; private set; }

        public int DayOfWeek { get; private set; }

        public ObservalbeCollectionEx<ScheduleEditorRecordTypeViewModel> RecordTypes { get; private set; }
    }
}