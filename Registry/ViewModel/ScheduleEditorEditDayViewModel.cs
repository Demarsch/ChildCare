using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using System.Windows.Navigation;
using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Registry
{
    public class ScheduleEditorEditDayViewModel : ObservableObject, IDialogViewModel, IDisposable
    {
        private readonly ICacheService cacheService;

        public ScheduleEditorEditDayViewModel(ICacheService cacheService)
        {
            if (cacheService == null)
                throw new ArgumentNullException("cacheService");
            this.cacheService = cacheService;
            AllowedRecordTypes = new ObservalbeCollectionEx<ScheduleEditorEditRecordTypeViewModel>();
            AllowedRecordTypes.CollectionChanged += OnAllowedRecordTypesChanged;
            AssignableRecordTypes = cacheService.GetItems<RecordType>().Where(x => x.Assignable.GetValueOrDefault()).ToArray();
            AddRecordTypeCommand = new RelayCommand(AddRecordType);
        }

        private void OnAllowedRecordTypesChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            IsChanged = true;
        }

        private bool isChanged;

        public bool IsChanged
        {
            get { return isChanged; }
            set { Set("IsChanged", ref isChanged, value); }
        }

        public ObservalbeCollectionEx<ScheduleEditorEditRecordTypeViewModel> AllowedRecordTypes { get; private set; }

        public IEnumerable<RecordType> AssignableRecordTypes { get; private set; }

        public IEnumerable<ScheduleEditorScheduleItemViewModel> ScheduleItems
        {
            get
            {
                var scheduleItems = AllowedRecordTypes.SelectMany(x => x.TimeIntervals.Select(y => new ScheduleItem
                {
                    StartTime = y.StartTime,
                    EndTime = y.EndTime,
                    RecordTypeId = x.RecordTypeId
                })).ToArray();
                foreach (var scheduleItem in scheduleItems)
                {
                    scheduleItem.BeginDate = currentDate;
                    scheduleItem.EndDate = isThisDayOnly ? currentDate : DateTime.MaxValue;
                    scheduleItem.RoomId = currentRoomId;
                    scheduleItem.DayOfWeek = currentDate.GetDayOfWeek();
                }
                return scheduleItems.Select(x => new ScheduleEditorScheduleItemViewModel(x, cacheService)).ToArray();
            }
            set
            {
                AllowedRecordTypes.Replace(value.GroupBy(x => x.RecordTypeId).Select(x => new ScheduleEditorEditRecordTypeViewModel { RecordTypeId = x.Key, TimeIntervals = x, IsChanged = false }));
            }
        }

        private DateTime currentDate;

        public DateTime CurrentDate
        {
            get { return currentDate; }
            set
            {
                if (Set("CurrentDate", ref currentDate, value))
                {
                    RaisePropertyChanged("Title");
                }
            }
        }

        private int currentRoomId;

        public int CurrentRoomId
        {
            get { return currentRoomId; }
            set
            {
                if (Set("CurrentRoomId", ref currentRoomId, value))
                {
                    RaisePropertyChanged("Title");
                }
            }
        }

        private bool isThisDayOnly;

        public bool IsThisDayOnly
        {
            get { return isThisDayOnly; }
            set
            {
                if (Set("IsThisDayOnly", ref isThisDayOnly, value))
                {
                    RaisePropertyChanged("Title");
                    IsChanged = true;
                }
            }
        }

        #region IDialogViewModel

        public string Title
        {
            get
            {
                var currentRoom = cacheService.GetItemById<Room>(currentRoomId);
                return string.Format("#{0} {1}, расписание на {2:dddd} {3} {4:d MMMM}",
                    currentRoom.Number, 
                    currentRoom.Name, 
                    currentDate, 
                    isThisDayOnly ? " на " : " начиная с ",
                    currentDate);
            }
        }

        public string ConfirmButtonText
        {
            get { return "Применить"; }
        }

        public string CancelButtonText
        {
            get { return "Отмена"; }
        }

        public RelayCommand<bool> CloseCommand { get; private set; }

        public ICommand AddRecordTypeCommand { get; private set; }

        private void AddRecordType()
        {
            AllowedRecordTypes.Add(new ScheduleEditorEditRecordTypeViewModel { RecordTypeId = AssignableRecordTypes.Select(x => x.Id).FirstOrDefault(), Times = "8:00-12:00, 13:00-17:00" });
        }

        public event EventHandler<ReturnEventArgs<bool>> CloseRequested;

        protected virtual void OnCloseRequested(ReturnEventArgs<bool> e)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        public void Dispose()
        {
            AllowedRecordTypes.CollectionChanged -= OnAllowedRecordTypesChanged;
        }
    }
}
