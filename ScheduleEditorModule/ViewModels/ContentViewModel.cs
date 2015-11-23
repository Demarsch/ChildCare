using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using Core.Data;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Mvvm;
using Core.Wpf.Services;
using log4net;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ScheduleEditorModule.Services;
using Shared.Schedule.Events;

namespace ScheduleEditorModule.ViewModels
{
    public class ContentViewModel : BindableBase, IDisposable
    {
        private readonly ILog log;

        private readonly IScheduleEditorService scheduleEditorService;

        private readonly ICacheService cacheService;

        private readonly IDialogService dialogService;

        private readonly IEventAggregator eventAggregator;

        private readonly HashSet<ScheduleItem> unsavedItems;

        public ContentViewModel(IScheduleEditorService scheduleEditorService, ILog log, ICacheService cacheService, IDialogService dialogService, IEventAggregator eventAggregator)
        {
            if (scheduleEditorService == null)
            {
                throw new ArgumentNullException("scheduleEditorService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (dialogService == null)
            {
                throw new ArgumentNullException("dialogService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            this.dialogService = dialogService;
            this.eventAggregator = eventAggregator;
            this.cacheService = cacheService;
            this.log = log;
            this.scheduleEditorService = scheduleEditorService;
            unsavedItems = new HashSet<ScheduleItem>();
            ChangeDateCommand = new DelegateCommand<int?>(ChangeDate);
            closeDayThisWeekCommand = new DelegateCommand<int?>(CloseDayThisWeek, CanCloseDayThisWeek);
            closeDayCommand = new DelegateCommand<int?>(CloseDay, CanCloseDay);
            saveChangesCommand = new DelegateCommand(SaveChangesAsync, HasChanges);
            saveChangesCommandWrapper = new CommandWrapper
                                        {
                                            Command = saveChangesCommand,
                                            CommandName = "Повторить"
                                        };
            cancelChangesCommand = new DelegateCommand(CancelChanges, HasChanges);
            CopyCommand = new DelegateCommand<object>(Copy);
            pasteCommand = new DelegateCommand<object>(Paste, CanPaste);
            Rooms = cacheService.GetItems<Room>().Select(x => new ScheduleEditorRoomViewModel(x) { CopyCommand = CopyCommand, PasteCommand = PasteCommand }).ToArray();
            foreach (var roomDay in Rooms.SelectMany(x => x.Days))
            {
                roomDay.CopyCommand = CopyCommand;
                roomDay.PasteCommand = PasteCommand;
                roomDay.EditRequested += RoomDayOnEditRequested;
                roomDay.CloseRequested += RoomDayOnCloseRequested;
            }
            clipboardContentDescription = "Буфер обмена пуст";
            BusyMediator = new BusyMediator();
            FailureMediator = new FailureMediator();
            SelectedDate = DateTime.Today;
            CommandManager.RequerySuggested += CommandManagerOnRequerySuggested;
        }

        private void CommandManagerOnRequerySuggested(object sender, EventArgs eventArgs)
        {
            closeDayThisWeekCommand.RaiseCanExecuteChanged();
            closeDayCommand.RaiseCanExecuteChanged();
            pasteCommand.RaiseCanExecuteChanged();
        }

        public BusyMediator BusyMediator { get; private set; }

        public FailureMediator FailureMediator { get; private set; }

        private ScheduleItem[] clipboardContent;

        private Type clipboardContentType;

        private string clipboardContentDescription;

        public string ClipboardContentDescription
        {
            get { return clipboardContentDescription; }
            private set { SetProperty(ref clipboardContentDescription, value); }
        }

        private void RoomDayOnEditRequested(object sender, EventArgs e)
        {
            var roomDay = (ScheduleEditorRoomDayViewModel)sender;
            using (var viewModel = new ScheduleEditorEditDayViewModel(cacheService))
            {
                viewModel.CurrentDate = roomDay.RelatedDate;
                viewModel.CurrentRoomId = roomDay.RoomId;
                viewModel.IsThisDayOnly = roomDay.IsThisDayOnly;
                viewModel.ScheduleItems = roomDay.ScheduleItems;
                viewModel.IsChanged = false;
                if (dialogService.ShowDialog(viewModel) == true && (viewModel.IsChanged || viewModel.AllowedRecordTypes.Any(x => x.IsChanged)))
                {
                    var newItems = viewModel.ScheduleItems.ToArray();
                    UpdateUnsavedItems(newItems, roomDay);
                    if (newItems.Length == 1 && newItems[0].RecordTypeId == 0)
                    {
                        log.InfoFormat("Manually closed {0:dd.MM.yy}{1} for room (Id = {2})",
                            roomDay.RelatedDate,
                            roomDay.IsThisDayOnly ? " (this day only)" : string.Empty,
                            roomDay.RoomId);
                    }
                    log.InfoFormat("Updated {0:dd.MM.yy}{1} with {2} schedule items for room (Id = {3})",
                        roomDay.RelatedDate,
                        roomDay.IsThisDayOnly ? " (this day only)" : string.Empty,
                        newItems.Length,
                        roomDay.RoomId);
                }
            }
        }

        private void UpdateUnsavedItems(ICollection<ScheduleEditorScheduleItemViewModel> newScheduleItems, ScheduleEditorRoomDayViewModel roomDay)
        {
            var sampleItem = newScheduleItems.First();
            var oldCount = unsavedItems.Count;
            unsavedItems.RemoveWhere(x => x.RoomId == roomDay.RoomId && x.DayOfWeek == roomDay.DayOfWeek && x.BeginDate == sampleItem.BeginDate);
            roomDay.ScheduleItems.Replace(newScheduleItems);
            unsavedItems.UnionWith(newScheduleItems.Select(x => x.GetScheduleItem().Clone()).Cast<ScheduleItem>());
            var newCount = unsavedItems.Count;
            log.Info(oldCount == newCount
                ? "Unsaved item count remains the same"
                : oldCount > newCount
                    ? string.Format("Removed {0} items from unsaved collection", oldCount - newCount)
                    : string.Format("Added {0} items to unsaved collection", newCount - oldCount));
            saveChangesCommand.RaiseCanExecuteChanged();
            cancelChangesCommand.RaiseCanExecuteChanged();
        }

        private void RoomDayOnCloseRequested(object sender, ReturnEventArgs<bool> e)
        {
            var roomDay = (ScheduleEditorRoomDayViewModel)sender;
            var thisDayOnly = e.Result;
            var scheduleItem = new ScheduleItem
            {
                BeginDate = roomDay.RelatedDate,
                EndDate = thisDayOnly ? roomDay.RelatedDate : DateTime.MaxValue.Date,
                DayOfWeek = roomDay.DayOfWeek,
                RecordTypeId = null,
                RoomId = roomDay.RoomId,
            };
            var newScheduleItems = new[] { new ScheduleEditorScheduleItemViewModel(scheduleItem, cacheService) };
            roomDay.ScheduleItems.Replace(newScheduleItems);
            log.InfoFormat("Closed {0:dd.MM.yy} for room (Id = {1}){2} via menu", roomDay.RelatedDate, roomDay.RoomId, thisDayOnly ? " (this day only)" : string.Empty);
            UpdateUnsavedItems(newScheduleItems, roomDay);
        }

        private DelegateCommand<int?> closeDayThisWeekCommand; 

        public ICommand CloseDayThisWeekCommand { get { return closeDayThisWeekCommand; } }

        private void CloseDayThisWeek(int? dayOfWeek)
        {
            if (dayOfWeek.HasValue)
            {
                CloseSchedule(dayOfWeek, true);
            }
        }

        private bool CanCloseDayThisWeek(int? dayOfWeek)
        {
            return CanCloseSchedule(dayOfWeek, true);
        }

        private DelegateCommand<int?> closeDayCommand; 

        public ICommand CloseDayCommand { get { return closeDayCommand; } }

        private void CloseDay(int? dayOfWeek)
        {
            CloseSchedule(dayOfWeek, false);
        }

        private bool CanCloseDay(int? dayOfWeek)
        {
            return CanCloseSchedule(dayOfWeek, false);
        }

        private void CloseSchedule(int? dayOfWeek, bool thisWeek)
        {
            if (dayOfWeek == 0 || dayOfWeek == null)
            {
                return;
            }
            log.InfoFormat("Closing {0} day of week ({1:dd.MM} - {2:dd.MM}){3}",
                dayOfWeek,
                selectedDate.GetWeekBegininng(),
                selectedDate.GetWeekEnding(),
                thisWeek ? " (this week only)" : string.Empty);
            foreach (var day in Rooms.Select(x => x.Days[dayOfWeek.Value - 1]))
            {
                day.CloseCommand.Execute(thisWeek);
            }
        }

        private bool CanCloseSchedule(int? dayOfWeek, bool thisWeek)
        {
            if (dayOfWeek == 0 || dayOfWeek == null)
            {
                return false;
            }
            var result = false;
            foreach (var day in Rooms.Select(x => x.Days[dayOfWeek.Value - 1]))
            {
                result = result || day.CloseCommand.CanExecute(thisWeek);
            }
            return result;
        }

        public ICommand CopyCommand { get; private set; }

        private void Copy(object source)
        {
            clipboardContentType = source == null ? null : source.GetType();
            var roomDay = source as ScheduleEditorRoomDayViewModel;
            if (roomDay != null)
            {
                clipboardContent = roomDay.ScheduleItems.Select(x => x.GetScheduleItem()).ToArray();
                var roomObject = cacheService.GetItemById<Room>(roomDay.RoomId);
                ClipboardContentDescription = string.Format("В буфере обмена находится {0:dd.MM.yyyy}/{1}", roomDay.RelatedDate, roomObject.Name);
                log.InfoFormat("Copied {0:dd.MM.yy}/{1} to clipboard", roomDay.RelatedDate, roomObject.Name);
                return;
            }
            var dayOfWeek = source as DayOfWeekViewModel;
            if (dayOfWeek != null)
            {
                clipboardContent = Rooms.SelectMany(x => x.Days[dayOfWeek.DayOfWeek - 1].ScheduleItems).Select(x => x.GetScheduleItem()).ToArray();
                ClipboardContentDescription = string.Format("В буфере обмена находится {0:dddd dd.MM.yyyy}", dayOfWeek.Date);
                log.InfoFormat("Copied {0:dd.MM.yy} to clipboard", dayOfWeek.Date);
                return;
            }
            var room = source as ScheduleEditorRoomViewModel;
            if (room != null)
            {
                clipboardContent = room.Days.SelectMany(x => x.ScheduleItems).Select(x => x.GetScheduleItem()).ToArray();
                ClipboardContentDescription = string.Format("В буфере обмена находится {0} {1}", room.Name, room.Number);
                log.InfoFormat("Copied room (Id = {0}) to clipboard", room.Id);
                return;
            }
            log.ErrorFormat("Failed to copy part of schedule. Unexpected type of content - {0}", clipboardContentType);
            dialogService.ShowError("Не удалось скопировать часть расписания. Тип содержимого неизвестен");
        }

        private DelegateCommand<object> pasteCommand;

        public ICommand PasteCommand
        {
            get { return pasteCommand; }
        }

        private void Paste(object destination)
        {
            var roomDay = destination as ScheduleEditorRoomDayViewModel;
            if (roomDay != null)
            {
                PasteRoomDay(roomDay);
                return;
            }
            var dayOfWeek = destination as DayOfWeekViewModel;
            if (dayOfWeek != null)
            {
                PasteDayOfWeek(dayOfWeek);
                return;
            }
            var room = destination as ScheduleEditorRoomViewModel;
            if (room != null)
            {
                PasteRoom(room);
                return;
            }
            log.ErrorFormat("Failed to copy part of schedule. Unexpected type of content - {0}", clipboardContentType);
            dialogService.ShowError("Не удалось скопировать часть расписания. Тип содержимого неизвестен");
        }

        private void PasteRoom(ScheduleEditorRoomViewModel destinationRoom)
        {
            log.InfoFormat("Pasted clipboard content to room (Id = {0})", destinationRoom.Id);
            var currentWeekBeginning = selectedDate.GetWeekBegininng();
            var newItems = clipboardContent.Select(x => x.Clone() as ScheduleItem).ToArray();
            foreach (var newItem in newItems)
            {
                var isThisDayOnly = newItem.BeginDate == newItem.EndDate;
                var currentDay = currentWeekBeginning.AddDays(newItem.DayOfWeek - 1);
                newItem.BeginDate = currentDay;
                if (isThisDayOnly)
                {
                    newItem.EndDate = currentDay;
                }
                newItem.DayOfWeek = currentDay.GetDayOfWeek();
                newItem.RoomId = destinationRoom.Id;
            }
            var groupedItems = newItems.ToLookup(x => x.DayOfWeek);
            foreach (var destinationRoomDay in Rooms.First(x => x.Id == destinationRoom.Id).Days)
            {
                var newDayItems = groupedItems[destinationRoomDay.DayOfWeek].Select(x => new ScheduleEditorScheduleItemViewModel(x, cacheService)).ToList();
                if (newDayItems.Count == 0)
                {
                    newDayItems.Add(new ScheduleEditorScheduleItemViewModel(new ScheduleItem
                    {
                        BeginDate = destinationRoomDay.RelatedDate,
                        EndDate = DateTime.MaxValue.Date,
                        DayOfWeek = destinationRoomDay.DayOfWeek,
                        RecordTypeId = null,
                        StartTime = TimeSpan.Zero,
                        EndTime = TimeSpan.Zero,
                        RoomId = destinationRoomDay.RoomId
                    }, cacheService));
                }
                UpdateUnsavedItems(newDayItems, destinationRoomDay);
            }
        }

        private void PasteDayOfWeek(DayOfWeekViewModel destinationDayOfWeek)
        {
            log.InfoFormat("Pasted clipboard content to {0:dd.MM.yy} day", destinationDayOfWeek.Date);
            var newItems = clipboardContent.Select(x => x.Clone() as ScheduleItem).ToArray();
            foreach (var newItem in newItems)
            {
                var isThisDayOnly = newItem.BeginDate == newItem.EndDate;
                newItem.BeginDate = destinationDayOfWeek.Date;
                if (isThisDayOnly)
                {
                    newItem.EndDate = destinationDayOfWeek.Date;
                }
                newItem.DayOfWeek = destinationDayOfWeek.DayOfWeek;
            }
            var groupedItems = newItems.ToLookup(x => x.RoomId);
            foreach (var destinationRoomDay in Rooms.Select(x => x.Days[destinationDayOfWeek.DayOfWeek - 1]))
            {
                var newDayItems = groupedItems[destinationRoomDay.RoomId].Select(x => new ScheduleEditorScheduleItemViewModel(x, cacheService)).ToList();
                if (newDayItems.Count == 0)
                {
                    newDayItems.Add(new ScheduleEditorScheduleItemViewModel(new ScheduleItem
                    {
                        BeginDate = destinationRoomDay.RelatedDate,
                        EndDate = DateTime.MaxValue.Date,
                        DayOfWeek = destinationRoomDay.DayOfWeek,
                        RecordTypeId = null,
                        StartTime = TimeSpan.Zero,
                        EndTime = TimeSpan.Zero,
                        RoomId = destinationRoomDay.RoomId
                    }, cacheService));
                }
                UpdateUnsavedItems(newDayItems, destinationRoomDay);
            }
        }

        private void PasteRoomDay(ScheduleEditorRoomDayViewModel destinationRoomDay)
        {
            log.InfoFormat("Pasted clipboard content to day/room {0:dd.MM.yy}/{1}", destinationRoomDay.RelatedDate, destinationRoomDay.RoomId);
            var newItems = clipboardContent.Select(x => x.Clone() as ScheduleItem).ToArray();
            foreach (var newItem in newItems)
            {
                var isThisDayOnly = newItem.BeginDate == newItem.EndDate;
                newItem.BeginDate = destinationRoomDay.RelatedDate;
                if (isThisDayOnly)
                {
                    newItem.EndDate = destinationRoomDay.RelatedDate;
                }
                newItem.DayOfWeek = destinationRoomDay.DayOfWeek;
                newItem.RoomId = destinationRoomDay.RoomId;
            }
            var newDayItems = newItems.Select(x => new ScheduleEditorScheduleItemViewModel(x, cacheService)).ToList();
            if (newDayItems.Count == 0)
            {
                newDayItems.Add(new ScheduleEditorScheduleItemViewModel(new ScheduleItem
                {
                    BeginDate = destinationRoomDay.RelatedDate,
                    EndDate = DateTime.MaxValue.Date,
                    DayOfWeek = destinationRoomDay.DayOfWeek,
                    RecordTypeId = null,
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.Zero,
                    RoomId = destinationRoomDay.RoomId
                }, cacheService));
            }
            UpdateUnsavedItems(newDayItems, destinationRoomDay);
        }

        private bool CanPaste(object destination)
        {
            var newContentType = destination == null ? null : destination.GetType();
            return clipboardContentType != null && newContentType == clipboardContentType;
        }

        public ICommand ChangeDateCommand { get; private set; }

        private void ChangeDate(int? dayCount)
        {
            if (dayCount.HasValue)
            {
                SelectedDate = selectedDate.AddDays(dayCount.Value);
            }
            else
            {
                SelectedDate = DateTime.Today;
            }
        }

        public IEnumerable<ScheduleEditorRoomViewModel> Rooms { get; private set; }

        private DayOfWeekViewModel[] weekDays;

        public DayOfWeekViewModel[] WeekDays
        {
            get { return weekDays; }
            private set { SetProperty(ref weekDays, value); }
        }

        private DateTime selectedDate;

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                var newWeekBegining = value.GetWeekBegininng();
                var differentWeek = newWeekBegining != selectedDate.GetWeekBegininng();
                SetProperty(ref selectedDate, value);
                if (differentWeek)
                {
                    log.InfoFormat("New selected date is {0:dd.MM.yy}", value);
                    WeekDays = Enumerable.Range(0, 7).Select(x => new DayOfWeekViewModel(newWeekBegining.AddDays(x))
                    {
                        CloseDayCommand = CloseDayCommand,
                        CloseDayThisWeekCommand = CloseDayThisWeekCommand,
                        CopyCommand = CopyCommand,
                        PasteCommand = PasteCommand
                    }).ToArray();
                    LoadScheduleAsync();
                }
            }
        }

        public async void LoadScheduleAsync()
        {
            try
            {
                FailureMediator.Deactivate();
                BusyMediator.Activate("Загрузка расписания");
                var selectedWeekBegining = selectedDate.GetWeekBegininng();
                log.InfoFormat("Loading schedule editor for {0:dd.MM.yyyy}-{1:dd.MM.yyyy}", selectedWeekBegining, selectedDate.GetWeekEnding());
                await Task.Delay(TimeSpan.FromSeconds(0.5));
                var scheduleItems = (await Task<IEnumerable<ScheduleItem>>.Factory.StartNew(() => scheduleEditorService.GetRoomsWorkingTimeForWeek(selectedDate))).ToLookup(x => x.RoomId);
                foreach (var room in Rooms)
                {
                    var currentRoomItems = scheduleItems[room.Id].ToLookup(x => x.DayOfWeek);
                    foreach (var day in room.Days)
                    {
                        day.RelatedDate = selectedWeekBegining.AddDays(day.DayOfWeek - 1);
                        //First we check if we previously changed schedule for the given day-room
                        var currentDayUnsavedItems = unsavedItems.Where(x => x.RoomId == day.RoomId && x.DayOfWeek == day.DayOfWeek && x.BeginDate == day.RelatedDate).ToArray();
                        if (currentDayUnsavedItems.Length != 0)
                        {
                            day.ScheduleItems.Replace(currentDayUnsavedItems.Select(x => new ScheduleEditorScheduleItemViewModel(x, cacheService)));
                            day.State = RoomDayState.ChangedDirectly;
                            continue;
                        }
                        //Now we check if we made changes for different day that influe current day
                        var previousDayUnsavedItems = unsavedItems.Where(x => x.RoomId == day.RoomId && x.DayOfWeek == day.DayOfWeek && x.BeginDate < day.RelatedDate && x.EndDate >= day.RelatedDate)
                            .GroupBy(x => x.BeginDate)
                            .OrderByDescending(x => x.Key)
                            .FirstOrDefault();
                        if (previousDayUnsavedItems != null)
                        {
                            day.ScheduleItems.Replace(previousDayUnsavedItems.Select(x => new ScheduleEditorScheduleItemViewModel(x, cacheService)));
                            day.State = RoomDayState.ChangedIndirectly;
                            continue;
                        }
                        //Otherwise we just get data from database
                        day.ScheduleItems.Replace(currentRoomItems[day.DayOfWeek].Select(x => new ScheduleEditorScheduleItemViewModel(x, cacheService)));
                        day.State = RoomDayState.Unchanged;
                    }
                }
                log.Info("Successfully loaded schedule editor");
            }
            catch (Exception ex)
            {
                log.Error("Failed to load schedule editor", ex);
                FailureMediator.Activate(string.Format("Не удалось загрузить расписание. Попробуйте выбрать другие даты. Если ошибка повторится, обратитесь в службу поддержки"), exception: ex);
            }
            finally
            {
                BusyMediator.Deactivate();
            }
        }

        private readonly DelegateCommand saveChangesCommand;

        private readonly CommandWrapper saveChangesCommandWrapper;

        public ICommand SaveChangesCommand { get { return saveChangesCommand; } }

        private async void SaveChangesAsync()
        {
            log.InfoFormat("Trying to save schedule changes (Schedule items count = {0})...", unsavedItems.Count);
            BusyMediator.Activate("Сохранение расписания");
            try
            {
                await scheduleEditorService.SaveScheduleAsync(unsavedItems);
                ClearUnsavedItems();
                eventAggregator.GetEvent<ScheduleChangedEvent>().Publish(null);
                log.Info("Schedule changes successfully saved");
            }
            catch (Exception ex)
            {
                log.Error("Failed to save schedule changes", ex);
                FailureMediator.Activate("Не удалось сохранить изменения в расписании. Попробуйте еще раз или отмените изменения. Если ошибка повторится, обратитесь в службу поддержки",
                                         saveChangesCommandWrapper,
                                         ex,
                                         true);
            }
            finally
            {
                saveChangesCommand.RaiseCanExecuteChanged();
                cancelChangesCommand.RaiseCanExecuteChanged();
                BusyMediator.Deactivate();
            }
        }

        private void ClearUnsavedItems()
        {
            unsavedItems.Clear();
            foreach (var roomDay in Rooms.SelectMany(x => x.Days))
            {
                roomDay.State = RoomDayState.Unchanged;
            }
        }

        private bool HasChanges()
        {
            return unsavedItems.Count != 0;
        }

        private readonly DelegateCommand cancelChangesCommand;

        public ICommand CancelChangesCommand { get { return cancelChangesCommand; }}

        public void CancelChanges()
        {
            ClearUnsavedItems();
            LoadScheduleAsync();
        }

        public void Dispose()
        {
            CommandManager.RequerySuggested -= CommandManagerOnRequerySuggested;
            foreach (var roomDay in Rooms.SelectMany(x => x.Days))
            {
                roomDay.EditRequested -= RoomDayOnEditRequested;
                roomDay.CloseRequested -= RoomDayOnCloseRequested;
            }
            foreach (var room in Rooms)
            {
                room.Dispose();
            }
        }
    }
}
