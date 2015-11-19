using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Navigation;
using System.Windows.Threading;
using Core.Data;
using Core.Services;
using Core.Wpf.Mvvm;
using Prism.Commands;
using Prism.Mvvm;
using ScheduleModule.Services;
using Shell.Shared;

namespace ScheduleModule.ViewModels
{
    public class ScheduleAssignmentUpdateViewModel : BindableBase, IDialogViewModel, IDataErrorInfo
    {
        private DispatcherTimer timer;

        private static readonly Org SelfAssigned = new Org { Name = "Самообращение" };

        public ScheduleAssignmentUpdateViewModel(IScheduleService scheduleService, ICacheService cacheService, bool runCountdown)
        {
            FinancingSources = cacheService.GetItems<FinancingSource>().OrderBy(x => x.Name).ToArray();
            AssignLpuList = new[] { SelfAssigned }.Concat(scheduleService.GetLpus()).ToArray();
            CloseCommand = new DelegateCommand<bool?>(Close);
            if (runCountdown)
            {
                timer = new DispatcherTimer(TimeSpan.FromMinutes(10.0), DispatcherPriority.ApplicationIdle, (sender, args) =>
                                                                                                            {
                                                                                                                timer.Stop();
                                                                                                                timer = null;
                                                                                                                OnCloseRequested(new ReturnEventArgs<bool>(false));
                                                                                                            },
                                            Dispatcher.CurrentDispatcher);
            }
            SelectedAssignLpu = AssignLpuList.FirstOrDefault(x => x.Name == ConfigurationManager.AppSettings[ApplicationSettings.CurrentLpuName]);
        }

        private FinancingSource selectedFinancingSource;

        public FinancingSource SelectedFinancingSource
        {
            get { return selectedFinancingSource; }
            set { SetProperty(ref selectedFinancingSource, value); }
        }

        private Org selectedAssignLpu;

        public Org SelectedAssignLpu
        {
            get { return selectedAssignLpu; }
            set
            {
                if (value == null)
                {
                    value = SelfAssigned;
                }
                SetProperty(ref selectedAssignLpu, value);
                IsSelfAssigned = value == SelfAssigned;
            }
        }

        private bool isSelfAssigned;

        public bool IsSelfAssigned
        {
            get { return isSelfAssigned; }
            private set { SetProperty(ref isSelfAssigned, value); }
        }

        private string note;

        public string Note
        {
            get { return note; }
            set { SetProperty(ref note, value); }
        }

        public IEnumerable<FinancingSource> FinancingSources { get; private set; }

        public IEnumerable<Org> AssignLpuList { get; private set; }

        public string Title
        {
            get { return "Дополнительные детали назначения"; }
        }

        public string ConfirmButtonText
        {
            get { return "Сохранить"; }
        }

        public string CancelButtonText
        {
            get { return "Отменить"; }
        }

        public DelegateCommand<bool?> CloseCommand { get; private set; }

        private void Close(bool? validate)
        {
            saveWasRequested = true;
            if (validate == true)
            {
                OnPropertyChanged(string.Empty);
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