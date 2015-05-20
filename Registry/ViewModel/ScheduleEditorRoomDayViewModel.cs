using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Registry
{
    public class ScheduleEditorRoomDayViewModel : ObservableObject, IDisposable
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
            CloseCommand = new RelayCommand<bool>(Close, CanClose);
            isRoomDayClosed = true;
        }

        private void OnScheduleItemsChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            IsThisDayOnly = ScheduleItems.Count != 0 && ScheduleItems.Any(x => x.BeginDate == x.EndDate);
            IsRoomDayClosed = ScheduleItems.Count == 0 || ScheduleItems.Any(x => x.RecordTypeId == 0);
            State = RoomDayState.ChangedDirectly;
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

        private DateTime relatedDate;

        public DateTime RelatedDate
        {
            get { return relatedDate; }
            set { Set("RelatedDate", ref relatedDate, value); }
        }

        private bool isThisDayOnly;

        public bool IsThisDayOnly
        {
            get { return isThisDayOnly; }
            private set { Set("IsThisDayOnly", ref isThisDayOnly, value); }
        }

        private bool isRoomDayClosed;

        public bool IsRoomDayClosed
        {
            get { return isRoomDayClosed; }
            private set { Set("IsRoomDayClosed", ref isRoomDayClosed, value); }
        }

        private void EditRoomDay()
        {
            OnEditRequested();
        }

        public ICommand CloseCommand { get; private set; }

        private void Close(bool thisWeekOnly)
        {
            OnCloseRequested(thisWeekOnly);
        }

        private bool CanClose(bool thisWeekOnly)
        {
            return thisWeekOnly ? !isRoomDayClosed : !isRoomDayClosed || isThisDayOnly;
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(bool thisDayOnly)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, new ReturnEventArgs<bool>(thisDayOnly));
            }
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

        public void Dispose()
        {
            ScheduleItems.CollectionChanged -= OnScheduleItemsChanged;
        }

        private RoomDayState state;

        public RoomDayState State
        {
            get { return state; }
            set { Set("State", ref state, value); }
        }
    }

    public enum RoomDayState
    {
        Unchanged,
        ChangedDirectly,
        ChangedIndirectly
    }
}