using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Windows.Input;
using System.Windows.Navigation;
using Core.Data;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Mvvm;
using Prism.Commands;
using Prism.Mvvm;
using Shared.Schedule.Services;
using Shell.Shared;

namespace ScheduleEditorModule.ViewModels
{
    public class ScheduleEditorEditDayViewModel : BindableBase, IDialogViewModel, IDisposable
    {
        private readonly ICacheService cacheService;

        public ScheduleEditorEditDayViewModel(IScheduleServiceBase scheduleServiceBase, ICacheService cacheService)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (scheduleServiceBase == null)
            {
                throw new ArgumentNullException("scheduleServiceBase");
            }
            this.cacheService = cacheService;
            AllowedRecordTypes = new ObservableCollectionEx<ScheduleEditorEditRecordTypeViewModel>();
            AllowedRecordTypes.CollectionChanged += OnAllowedRecordTypesChanged;
            AssignableRecordTypes = scheduleServiceBase.GetRecordTypes();
            allAssignableRecortTypes = AssignableRecordTypes.SelectMany(x => x.GetAllChildren(true)).ToArray();
            CloseCommand = new DelegateCommand<bool?>(Close);
            addRecordTypeCommand = new DelegateCommand(AddRecordType, CanAddRecordType);
            ClearRecordTypesCommand = new DelegateCommand(ClearRecordTypes);
            RemoveRecordTypeCommand = new DelegateCommand<ScheduleEditorEditRecordTypeViewModel>(RemoveRecordType);
        }

        private void OnAllowedRecordTypesChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            IsChanged = true;
        }

        private bool isChanged;

        public bool IsChanged
        {
            get { return isChanged; }
            set { SetProperty(ref isChanged, value); }
        }

        public ObservableCollectionEx<ScheduleEditorEditRecordTypeViewModel> AllowedRecordTypes { get; private set; }

        public IEnumerable<RecordType> AssignableRecordTypes { get; private set; }

        private readonly RecordType[] allAssignableRecortTypes; 

        public IEnumerable<ScheduleEditorScheduleItemViewModel> ScheduleItems
        {
            get
            {
                var scheduleItems = AllowedRecordTypes.SelectMany(x => x.TimeIntervals.Select(y => new ScheduleItem
                                                                                                   {
                                                                                                       StartTime = y.StartTime,
                                                                                                       EndTime = y.EndTime,
                                                                                                       RecordTypeId = x.RecordType.Id
                                                                                                   })).ToList();
                foreach (var scheduleItem in scheduleItems)
                {
                    scheduleItem.BeginDate = currentDate;
                    scheduleItem.EndDate = isThisDayOnly ? currentDate : DateTime.MaxValue.Date;
                    scheduleItem.RoomId = currentRoomId;
                    scheduleItem.DayOfWeek = currentDate.GetDayOfWeek();
                }
                if (scheduleItems.Count == 0)
                {
                    scheduleItems.Add(new ScheduleItem
                                      {
                                          BeginDate = currentDate,
                                          EndDate = isThisDayOnly ? currentDate : DateTime.MaxValue.Date,
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
                AllowedRecordTypes.Replace(value.Where(x => x.RecordTypeId != 0)
                                                .GroupBy(x => x.RecordTypeId)
                                                .Select(x => new ScheduleEditorEditRecordTypeViewModel
                                                             {
                                                                 RecordType = cacheService.GetItemById<RecordType>(x.Key), 
                                                                 TimeIntervals = x, 
                                                                 IsChanged = false
                                                             }));
            }
        }

        private DateTime currentDate;

        public DateTime CurrentDate
        {
            get { return currentDate; }
            set
            {
                if (SetProperty(ref currentDate, value))
                {
                    OnPropertyChanged(() => Title);
                }
            }
        }

        private int currentRoomId;

        public int CurrentRoomId
        {
            get { return currentRoomId; }
            set
            {
                if (SetProperty(ref currentRoomId, value))
                {
                    OnPropertyChanged(() => Title);
                }
            }
        }

        private bool isThisDayOnly;

        public bool IsThisDayOnly
        {
            get { return isThisDayOnly; }
            set
            {
                if (SetProperty(ref isThisDayOnly, value))
                {
                    OnPropertyChanged(() => Title);
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
                return string.Format("{0}, {1} #{2}",
                                     currentDate.ToString("dddd d MMMM").FirstLetterToUpper(),
                                     currentRoom.Name,
                                     currentRoom.Number);
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

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private void Close(bool? validate)
        {
            if (validate == true)
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
        
        private readonly DelegateCommand addRecordTypeCommand;

        public ICommand AddRecordTypeCommand
        {
            get { return addRecordTypeCommand; }
        }

        private void AddRecordType()
        {
            var usedRecordTypes = new HashSet<RecordType>(AllowedRecordTypes.Select(x => x.RecordType));
            var firstUnusedRecordType = allAssignableRecortTypes.FirstOrDefault(x => !usedRecordTypes.Contains(x));
            AllowedRecordTypes.Add(new ScheduleEditorEditRecordTypeViewModel
                                   {
                                       RecordType = firstUnusedRecordType, 
                                       Times = ConfigurationManager.AppSettings[ApplicationSettings.DefaultRecordTypeTime]
                                   });
            addRecordTypeCommand.RaiseCanExecuteChanged();
        }

        private bool CanAddRecordType()
        {
            return AllowedRecordTypes.Select(x => x.RecordType).Distinct().Count() < allAssignableRecortTypes.Length;
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