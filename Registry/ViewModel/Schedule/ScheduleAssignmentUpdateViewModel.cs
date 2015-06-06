using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Navigation;
using System.Windows.Threading;
using Core;
using DataLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Registry
{
    public class ScheduleAssignmentUpdateViewModel : ObservableObject, IDialogViewModel, IDataErrorInfo
    {
        private readonly DispatcherTimer timer;

        private static readonly Lpu EmptyLpu = new Lpu();

        public ScheduleAssignmentUpdateViewModel(ICacheService cacheService, bool runCountdown)
        {
            FinacingSources = cacheService.GetItems<FinacingSource>().OrderBy(x => x.Name).ToArray();
            AssignLpuList = new[] { EmptyLpu }.Concat(cacheService.GetItems<Lpu>().OrderBy(x => x.Name)).ToArray();
            CloseCommand = new RelayCommand<object>(x => Close((bool?)x));
            if (runCountdown)
            {
                timer = new DispatcherTimer(TimeSpan.FromMinutes(10.0), DispatcherPriority.ApplicationIdle, (sender, args) => OnCloseRequested(new ReturnEventArgs<bool>(false)),
                    Dispatcher.CurrentDispatcher);
            }
        }

        private FinacingSource selectedFinancingSource;

        public FinacingSource SelectedFinancingSource
        {
            get { return selectedFinancingSource; }
            set { Set("SelectedFinancingSource", ref selectedFinancingSource, value); }
        }

        private Lpu selectedAssignLpu;

        public Lpu SelectedAssignLpu
        {
            get { return selectedAssignLpu; }
            set
            {
                if (value == EmptyLpu)
                {
                    value = null;
                }
                Set("SelectedAssignLpu", ref selectedAssignLpu, value);
            }
        }

        private string note;

        public string Note
        {
            get { return note; }
            set { Set("Note", ref note, value); }
        }

        public IEnumerable<FinacingSource> FinacingSources { get; private set; }

        public IEnumerable<Lpu> AssignLpuList { get; private set; }

        public string Title { get { return "Дополнительные детали назначения"; } }

        public string ConfirmButtonText { get { return "Сохранить"; } }

        public string CancelButtonText { get { return "Отменить"; } }

        public RelayCommand<object> CloseCommand { get; private set; }

        private void Close(bool? validate)
        {
            saveWasRequested = true;
            if (validate == true)
            {
                RaisePropertyChanged(string.Empty);
                if (invalidProperties.Count == 0)
                {
                    OnCloseRequested(new ReturnEventArgs<bool>(true));
                }
            }
            else
            {
                OnCloseRequested(new ReturnEventArgs<bool>(false));
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

        private bool saveWasRequested;

        private readonly HashSet<string> invalidProperties = new HashSet<string>();

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!saveWasRequested)
                {
                    invalidProperties.Remove(columnName);
                    return string.Empty;
                }
                var result = string.Empty;
                if (columnName == "SelectedFinancingSource")
                {
                    result = selectedFinancingSource == null || !selectedFinancingSource.IsActive ? "Укажите источник финансирования" : string.Empty;
                }
                if (string.IsNullOrEmpty(result))
                {
                    invalidProperties.Remove(columnName);
                }
                else
                {
                    invalidProperties.Add(columnName);
                }
                return result;
            }
        }

        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
    }
}
