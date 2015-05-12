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
            ScheduleItems = new ObservalbeCollectionEx<ScheduleEditorScheduleItemViewModel>();
            ScheduleItems.CollectionChanged += OnScheduleItemsChanged;
            var defaultView = CollectionViewSource.GetDefaultView(ScheduleItems);
            defaultView.GroupDescriptions.Add(new PropertyGroupDescription("RecordType"));
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
            return scheduleItem != null && !string.IsNullOrEmpty(scheduleItem.RecordType);
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
            dialogService.ShowMessage("В появившемся окне можно выбрать рекорд тайп!");
        }
    }
}