using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using log4net;

namespace Registry
{
    public class ScheduleEditorViewModel : ObservableObject, IDialogViewModel
    {
        private readonly ILog log;

        private IScheduleService scheduleService;

        public ScheduleEditorViewModel(IScheduleService scheduleService, ILog log)
        {
            if (scheduleService == null)
            {
                throw new ArgumentNullException("scheduleService");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            this.log = log;
            this.scheduleService = scheduleService;
            CloseCommand = new RelayCommand<bool>(x => OnCloseRequested(new ReturnEventArgs<bool>(x)));
            Rooms = scheduleService.GetRooms().Select(x => new ScheduleEditorRoomViewModel(x)).ToArray();
            SelectedDate = DateTime.Today;
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
                var differentWeek = selectedDate.GetWeekBegininng() != newWeekBegining;
                Set("SelectedDate", ref selectedDate, value);
                if (differentWeek)
                {
                    WeekDays = Enumerable.Range(0, 7).Select(x => new DayOfWeekViewModel(newWeekBegining.AddDays(x))).ToArray();
                }
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
    }
}
