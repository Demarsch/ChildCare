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
            CloseCommand = new RelayCommand<bool>(Close);
            AddRecordTypeCommand = new RelayCommand(AddRecordType, CanAddRecordType);
            ClearRecordTypesCommand = new RelayCommand(ClearRecordTypes);
            RemoveRecordTypeCommand = new RelayCommand<ScheduleEditorEditRecordTypeViewModel>(RemoveRecordType);
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

        public ICollection<RecordType> AssignableRecordTypes { get; private set; }

        public IEnumerable<ScheduleEditorScheduleItemViewModel> ScheduleItems
        {
            get
            {
                var scheduleItems = AllowedRecordTypes.SelectMany(x => x.TimeIntervals.Select(y => new ScheduleItem
                {
                    StartTime = y.StartTime,
                    EndTime = y.EndTime,
                    RecordTypeId = x.RecordTypeId
                })).ToList();
                foreach (var scheduleItem in scheduleItems)
                {
                    scheduleItem.BeginDate = currentDate;
                    scheduleItem.EndDate = isThisDayOnly ? currentDate : DateTime.MaxValue;
                    scheduleItem.RoomId = currentRoomId;
                    scheduleItem.DayOfWeek = currentDate.GetDayOfWeek();
                }
                if (scheduleItems.Count == 0)
                {
                    scheduleItems.Add(new ScheduleItem
                    {
                        BeginDate = currentDate,
                        EndDate = isThisDayOnly ? currentDate : DateTime.MaxValue,
                        RoomId = currentRoomId,
                        DayOfWeek = currentDate.GetDayOfWeek(),
                        StartTime = TimeSpan.Zero,
                        EndTime = TimeSpan.Zero,
                        RecordTypeId = null
                    });
                }
                return scheduleItems.Select(x => new ScheduleEditorScheduleItemViewModel(x, cacheService)).ToArray();
            }
            set
            {
                AllowedRecordTypes.Replace(value.Where(x => x.RecordTypeId != 0).GroupBy(x => x.RecordTypeId).Select(x => new ScheduleEditorEditRecordTypeViewModel { RecordTypeId = x.Key, TimeIntervals = x, IsChanged = false }));
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
                return string.Format("#{0} {1}: расписание на {2:dddd}{3} {4:d MMMM}",
                    currentRoom.Number, 
                    currentRoom.Name, 
                    currentDate, 
                    isThisDayOnly ? ", " : ", начиная с",
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
        
        private void Close(bool validate)
        {
            if (validate)
            {
                if (AllowedRecordTypes.Any(x => !string.IsNullOrEmpty(x.Error)))
                {
                    return;
                }
                OnCloseRequested(new ReturnEventArgs<bool>(true));
            }
            else
            {
                OnCloseRequested(new ReturnEventArgs<bool>(false));
            }
        }

        public ICommand AddRecordTypeCommand { get; private set; }

        private void AddRecordType()
        {
            var usedRecordTypes = new HashSet<int>(AllowedRecordTypes.Select(x => x.RecordTypeId));
            var firstUnusedRecordType = AssignableRecordTypes.Select(x => x.Id).FirstOrDefault(x => !usedRecordTypes.Contains(x));
            AllowedRecordTypes.Add(new ScheduleEditorEditRecordTypeViewModel { RecordTypeId = firstUnusedRecordType, Times = "8:00-12:00, 13:00-17:00" });
            (AddRecordTypeCommand as RelayCommand).RaiseCanExecuteChanged();
        }

        private bool CanAddRecordType()
        {
            return AllowedRecordTypes.Select(x => x.RecordTypeId).Distinct().Count() < AssignableRecordTypes.Count;
        }

        public ICommand ClearRecordTypesCommand { get; private set; }

        private void ClearRecordTypes()
        {
            AllowedRecordTypes.Clear();
        }

        public ICommand RemoveRecordTypeCommand { get; private set; }

        private void RemoveRecordType(ScheduleEditorEditRecordTypeViewModel recordType)
        {
            AllowedRecordTypes.Remove(recordType);
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
