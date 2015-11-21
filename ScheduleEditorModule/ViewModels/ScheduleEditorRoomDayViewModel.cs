using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using Core.Wpf.Mvvm;
using Prism.Commands;
using Prism.Mvvm;

namespace ScheduleEditorModule.ViewModels
{
    public class ScheduleEditorRoomDayViewModel : BindableBase, IDisposable
    {
        public ScheduleEditorRoomDayViewModel(int roomId, int dayOfWeek)
        {
            RoomId = roomId;
            DayOfWeek = dayOfWeek;
            ScheduleItems = new ObservableCollectionEx<ScheduleEditorScheduleItemViewModel>();
            ScheduleItems.CollectionChanged += OnScheduleItemsChanged;
            var defaultView = CollectionViewSource.GetDefaultView(ScheduleItems);
            defaultView.GroupDescriptions.Add(new PropertyGroupDescription("RecordTypeName"));
            defaultView.Filter = HideEmptyItems;
            EditRoomDayCommand = new DelegateCommand(EditRoomDay);
            CloseCommand = new DelegateCommand<bool?>(Close, CanClose);
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

        public int DayOfWeek
        {
            get { return dayOfWeek; }
            private set { dayOfWeek = value; }
        }

        public ObservableCollectionEx<ScheduleEditorScheduleItemViewModel> ScheduleItems { get; private set; }

        public DelegateCommand EditRoomDayCommand { get; private set; }

        private DateTime relatedDate;

        public DateTime RelatedDate
        {
            get { return relatedDate; }
            set { SetProperty(ref relatedDate, value); }
        }

        private bool isThisDayOnly;

        public bool IsThisDayOnly
        {
            get { return isThisDayOnly; }
            private set { SetProperty(ref isThisDayOnly, value); }
        }

        private bool isRoomDayClosed;

        public bool IsRoomDayClosed
        {
            get { return isRoomDayClosed; }
            private set { SetProperty(ref isRoomDayClosed, value); }
        }

        private void EditRoomDay()
        {
            OnEditRequested();
        }

        public ICommand CloseCommand { get; private set; }

        private void Close(bool? thisWeekOnly)
        {
            OnCloseRequested(thisWeekOnly);
        }

        private bool CanClose(bool? thisWeekOnly)
        {
            return thisWeekOnly.GetValueOrDefault() ? !isRoomDayClosed : !isRoomDayClosed || isThisDayOnly;
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(bool? thisDayOnly)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, new ReturnEventArgs<bool>(thisDayOnly.GetValueOrDefault()));
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

        private int dayOfWeek;

        public RoomDayState State
        {
            get { return state; }
            set { SetProperty(ref state, value); }
        }

        public ICommand CopyCommand { get; set; }

        public ICommand PasteCommand { get; set; }
    }

    public enum RoomDayState
    {
        Unchanged,
        ChangedDirectly,
        ChangedIndirectly
    }
}