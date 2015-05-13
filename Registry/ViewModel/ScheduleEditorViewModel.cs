﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Navigation;
using Core;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;

namespace Registry
{
    public class ScheduleEditorViewModel : BasicViewModel, IDialogViewModel, IDisposable
    {
        private readonly ILog log;

        private readonly IScheduleService scheduleService;

        private readonly ICacheService cacheService;

        private readonly IDialogService dialogService;

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
            CloseCommand = new RelayCommand<bool>(x => OnCloseRequested(new ReturnEventArgs<bool>(x)));
            ChangeDateCommand = new RelayCommand<int>(ChangeDate);
            Rooms = scheduleService.GetRooms().Select(x => new ScheduleEditorRoomViewModel(x)).ToArray();
            SelectedDate = DateTime.Today;
            foreach (var roomDay in Rooms.SelectMany(x => x.Days))
            {
                roomDay.EditRequested += RoomDayOnEditRequested;
            }
        }

        private void RoomDayOnEditRequested(object sender, EventArgs eventArgs)
        {
            var roomDay = sender as ScheduleEditorRoomDayViewModel;
            var viewModel = new ScheduleEditorEditDayViewModel(cacheService);
            viewModel.CurrentDate = roomDay.RelatedDate;
            viewModel.CurrentRoomId = roomDay.RoomId;
            viewModel.IsThisDayOnly = roomDay.IsThisDayOnly;
            viewModel.ScheduleItems = roomDay.ScheduleItems;
            viewModel.IsChanged = false;
            if (dialogService.ShowDialog(viewModel) == true && (viewModel.IsChanged || !viewModel.AllowedRecordTypes.Any(x => x.IsChanged)))
            {
                roomDay.ScheduleItems.Replace(viewModel.ScheduleItems);
            }
        }

        public ICommand ChangeDateCommand { get; private set; }

        private void ChangeDate(int dayCount)
        {
            SelectedDate = selectedDate.AddDays(dayCount);
        }

        public IEnumerable<ScheduleEditorRoomViewModel> Rooms { get; private set; }

        private IEnumerable<DayOfWeekViewModel> weekDays;

        public IEnumerable<DayOfWeekViewModel> WeekDays
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
                    WeekDays = Enumerable.Range(0, 7).Select(x => new DayOfWeekViewModel(newWeekBegining.AddDays(x))).ToArray();
                    LoadSchedule();
                }
            }
        }

        public void LoadSchedule()
        {
            try
            {
                FailReason = null;
                var selectedWeekBegining = selectedDate.GetWeekBegininng();
                log.InfoFormat("Loading schedule editor for {0:dd.MM.yyyy}-{1:dd.MM.yyyy}", selectedWeekBegining, selectedDate.GetWeekEnding());
                var scheduleItems = scheduleService.GetRoomsWeeklyWorkingTime(selectedDate).ToLookup(x => x.RoomId);
                foreach (var room in Rooms)
                {
                    var currentRoomItems = scheduleItems[room.Id].ToLookup(x => x.DayOfWeek);
                    foreach (var day in room.Days)
                    {
                        day.ScheduleItems.Replace(currentRoomItems[day.DayOfWeek].Select(x => new ScheduleEditorScheduleItemViewModel(x, cacheService)));
                        day.RelatedDate = selectedWeekBegining.AddDays(day.DayOfWeek - 1);
                    }
                }
                log.Info("Successfully loaded schedule editor");
            }
            catch (Exception ex)
            {
                log.Error("Failed to load schedule editor", ex);
                FailReason = string.Format("Не удалось загрузить расписание. Попробуйте выбрать другие даты. Если ошибка повторится, обратитесь в службу поддержки");
            }
        }

        #region IDialogViewModel

        public string Title { get { return "Редактор расписания"; } }

        public string ConfirmButtonText { get { return "Сохранить"; } }

        public string CancelButtonText { get { return "Отменить"; } }

        public RelayCommand<bool> CloseCommand { get; private set; }

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
            }
        }
    }
}
