using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Registry
{
    public class ScheduleEditorRoomDayViewModel : ObservableObject
    {
        public ScheduleEditorRoomDayViewModel(int roomId, int dayOfWeek)
        {
            RoomId = roomId;
            DayOfWeek = dayOfWeek;
            ScheduleItems = new ObservalbeCollectionEx<ScheduleEditorScheduleItemViewModel>();
            ScheduleItems.CollectionChanged += OnScheduleItemsChanged;
            var defaultView = CollectionViewSource.GetDefaultView(ScheduleItems);
            defaultView.GroupDescriptions.Add(new PropertyGroupDescription("RecordTypeName"));
            defaultView.Filter = HideEmptyItems;
            EditRoomDayCommand = new RelayCommand(EditRoomDay);
        }

        private void OnScheduleItemsChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            IsThisDayOnly = ScheduleItems.Count != 0 && ScheduleItems.Any(x => x.BeginDate == x.EndDate);
        }

        private bool HideEmptyItems(object obj)
        {
            var scheduleItem = obj as ScheduleEditorScheduleItemViewModel;
            return scheduleItem != null && !string.IsNullOrEmpty(scheduleItem.RecordTypeName);
        }

        public int RoomId { get; private set; }

        public int DayOfWeek { get; private set; }

        public ObservalbeCollectionEx<ScheduleEditorScheduleItemViewModel> ScheduleItems { get; private set; }

        public RelayCommand EditRoomDayCommand { get; private set; }

        public DateTime RelatedDate { get; set; }

        private bool isThisDayOnly;

        public bool IsThisDayOnly
        {
            get { return isThisDayOnly; }
            private set { Set("IsThisDayOnly", ref isThisDayOnly, value); }
        }

        private void EditRoomDay()
        {
            OnEditRequested();
        }

        public event EventHandler EditRequested;

        protected virtual void OnEditRequested()
        {
            var handler = EditRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}