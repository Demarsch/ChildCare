﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using Core;
using DataLib;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;
using Environment = System.Environment;

namespace Registry
{
    public class ScheduleEditorViewModel : BasicViewModel, IDialogViewModel, IDisposable
    {
        private readonly ILog log;

        private readonly IScheduleService scheduleService;

        private readonly ICacheService cacheService;

        private readonly IDialogService dialogService;

        private readonly HashSet<ScheduleItem> unsavedItems;

        public ScheduleEditorViewModel(IScheduleService scheduleService, ILog log, ICacheService cacheService, IDialogService dialogService)
        {
            if (scheduleService == null)
            {
                throw new ArgumentNullException("scheduleService");
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
            this.dialogService = dialogService;
            this.cacheService = cacheService;
            this.log = log;
            this.scheduleService = scheduleService;
            unsavedItems = new HashSet<ScheduleItem>();
            CloseCommand = new RelayCommand<bool>(Close);
            ChangeDateCommand = new RelayCommand<int>(ChangeDate);
            CloseDayThisWeekCommand = new RelayCommand<int>(CloseDayThisWeek, CanCloseDayThisWeek);
            CloseDayCommand = new RelayCommand<int>(CloseDay, CanCloseDay);
            CopyCommand = new RelayCommand<object>(Copy);
            PasteCommand = new RelayCommand<object>(Paste, CanPaste);
            Rooms = scheduleService.GetRooms().Select(x => new ScheduleEditorRoomViewModel(x) { CopyCommand = CopyCommand, PasteCommand = PasteCommand }).ToArray();
            SelectedDate = DateTime.Today;
            foreach (var roomDay in Rooms.SelectMany(x => x.Days))
            {
                roomDay.CopyCommand = CopyCommand;
                roomDay.PasteCommand = PasteCommand;
                roomDay.EditRequested += RoomDayOnEditRequested;
                roomDay.CloseRequested += RoomDayOnCloseRequested;
            }
            clipboardContentDescription = "Буфер обмена пуст";
        }

        private ScheduleItem[] clipboardContent;

        private Type clipboardContentType;

        private string clipboardContentDescription;

        public string ClipboardContentDescription
        {
            get { return clipboardContentDescription; }
            private set { Set("ClipboardContentDescription", ref clipboardContentDescription, value); }
        }

        private void RoomDayOnEditRequested(object sender, EventArgs e)
        {
            var roomDay = sender as ScheduleEditorRoomDayViewModel;
            using (var viewModel = new ScheduleEditorEditDayViewModel(cacheService))
            {
                viewModel.CurrentDate = roomDay.RelatedDate;
                viewModel.CurrentRoomId = roomDay.RoomId;
                viewModel.IsThisDayOnly = roomDay.IsThisDayOnly;
                viewModel.ScheduleItems = roomDay.ScheduleItems;
                viewModel.IsChanged = false;
                if (dialogService.ShowDialog(viewModel) == true && (viewModel.IsChanged || viewModel.AllowedRecordTypes.Any(x => x.IsChanged)))
                {
                    UpdateUnsavedItems(viewModel.ScheduleItems.ToArray(), roomDay);
                }
            }
        }

        private void UpdateUnsavedItems(ICollection<ScheduleEditorScheduleItemViewModel> newScheduleItems, ScheduleEditorRoomDayViewModel roomDay)
        {
            var sampleItem = newScheduleItems.First();
            unsavedItems.RemoveWhere(x => x.RoomId == roomDay.RoomId && x.DayOfWeek == roomDay.DayOfWeek && x.BeginDate == sampleItem.BeginDate);
            roomDay.ScheduleItems.Replace(newScheduleItems);
            unsavedItems.UnionWith(newScheduleItems.Select(x => x.GetScheduleItem().Clone()).Cast<ScheduleItem>());
        }

        private void RoomDayOnCloseRequested(object sender, ReturnEventArgs<bool> e)
        {
            var roomDay = sender as ScheduleEditorRoomDayViewModel;
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
            UpdateUnsavedItems(newScheduleItems, roomDay);
        }

        public ICommand CloseDayThisWeekCommand { get; private set; }

        private void CloseDayThisWeek(int dayOfWeek)
        {
            CloseSchedule(dayOfWeek, true);
        }

        private bool CanCloseDayThisWeek(int dayOfWeek)
        {
            return CanCloseSchedule(dayOfWeek, true);
        }

        public ICommand CloseDayCommand { get; private set; }

        private void CloseDay(int dayOfWeek)
        {
            CloseSchedule(dayOfWeek, false);
        }

        private bool CanCloseDay(int dayOfWeek)
        {
            return CanCloseSchedule(dayOfWeek, false);
        }

        private void CloseSchedule(int dayOfWeek, bool thisWeek)
        {
            if (dayOfWeek == 0)
            {
                return;
            }
            foreach (var day in Rooms.Select(x => x.Days[dayOfWeek - 1]))
            {
                day.CloseCommand.Execute(thisWeek);
            }
        }

        private bool CanCloseSchedule(int dayOfWeek, bool thisWeek)
        {
            if (dayOfWeek == 0)
            {
                return false;
            }
            var result = false;
            foreach (var day in Rooms.Select(x => x.Days[dayOfWeek - 1]))
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
                ClipboardContentDescription = string.Format("В буфере обмена находится {0:dd.MM.yyyy}/{1}", roomDay.RelatedDate, cacheService.GetItemById<Room>(roomDay.RoomId).Name);
                return;
            }
            var dayOfWeek = source as DayOfWeekViewModel;
            if (dayOfWeek != null)
            {
                clipboardContent = Rooms.SelectMany(x => x.Days[dayOfWeek.DayOfWeek - 1].ScheduleItems).Select(x => x.GetScheduleItem()).ToArray();
                ClipboardContentDescription = string.Format("В буфере обмена находится {0:dddd dd.MM.yyyy}", dayOfWeek.Date);
                return;
            }
            var room = source as ScheduleEditorRoomViewModel;
            if (room != null)
            {
                clipboardContent = room.Days.SelectMany(x => x.ScheduleItems).Select(x => x.GetScheduleItem()).ToArray();
                ClipboardContentDescription = string.Format("В буфере обмена находится {0} {1}", room.Name, room.Number);
                return;
            }
            log.ErrorFormat("Failed to copy part of schedule. Unexpected type of content - {0}", clipboardContentType);
            dialogService.ShowError("Не удалось скопировать часть расписания. Тип содержимого неизвестен");
        }

        public ICommand PasteCommand { get; private set; }

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

        private void ChangeDate(int dayCount)
        {
            SelectedDate = selectedDate.AddDays(dayCount);
        }

        public IEnumerable<ScheduleEditorRoomViewModel> Rooms { get; private set; }

        private DayOfWeekViewModel[] weekDays;

        public DayOfWeekViewModel[] WeekDays
        {
            get { return weekDays; }
            private set { Set("WeekDays", ref weekDays, value); }
        }

        private DateTime selectedDate;

        public DateTime SelectedDate
        {
            get { return selectedDate; }
            set
            {
                var newWeekBegining = value.GetWeekBegininng();
                var differentWeek = newWeekBegining != selectedDate.GetWeekBegininng();
                Set("SelectedDate", ref selectedDate, value);
                if (differentWeek)
                {
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

        public async Task LoadScheduleAsync()
        {
            try
            {
                FailReason = null;
                BusyStatus = "Загрузка расписания";
                var selectedWeekBegining = selectedDate.GetWeekBegininng();
                log.InfoFormat("Loading schedule editor for {0:dd.MM.yyyy}-{1:dd.MM.yyyy}", selectedWeekBegining, selectedDate.GetWeekEnding());
                await Task.Delay(TimeSpan.FromSeconds(0.5));
                var scheduleItems = (await Task<ICollection<ScheduleItem>>.Factory.StartNew(() => scheduleService.GetRoomsWeeklyWorkingTime(selectedDate))).ToLookup(x => x.RoomId);
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
                FailReason = string.Format("Не удалось загрузить расписание. Попробуйте выбрать другие даты. Если ошибка повторится, обратитесь в службу поддержки");
            }
            finally
            {
                BusyStatus = null;
            }
        }

        #region IDialogViewModel

        public string Title { get { return "Редактор расписания"; } }

        public string ConfirmButtonText { get { return "Сохранить"; } }

        public string CancelButtonText { get { return "Отменить"; } }

        public RelayCommand<bool> CloseCommand { get; private set; }

        private void Close(bool save)
        {
            save = save && unsavedItems.Count > 0;
            if (TryClose(save))
            {
                OnCloseRequested(new ReturnEventArgs<bool>(save));
            }
        }

        public bool CanBeClosed()
        {
            if (unsavedItems.Count == 0)
            {
                return true;
            }
            var userPreferedToSaveChanges = dialogService.AskUser("Имеются несохраненные изменения в расписании. Сохранить их?") ?? false;
            return !userPreferedToSaveChanges || TryClose(true);
        }

        private bool TryClose(bool save)
        {
            if (!save)
            {
                return true;
            }
            try
            {
                log.InfoFormat("Trying to save schedule changes (Schedule items count = {0})...", unsavedItems.Count);
                scheduleService.SaveSchedule(unsavedItems);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Failed to save schedule changes", ex);
                dialogService.ShowError(
                    string.Format("Не удалось сохранить изменения в расписании. Причина - {0}{1}Попробуйте еще раз или отмените изменения. Если ошибка повторится, обратитесь в службу поддержки",
                        ex.Message, Environment.NewLine));
                return false;
            }
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
