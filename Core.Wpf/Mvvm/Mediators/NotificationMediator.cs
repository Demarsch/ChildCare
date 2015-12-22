using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;

namespace Core.Wpf.Mvvm
{
    public class NotificationMediator : BindableBase, IMediator
    {
        public static readonly TimeSpan DefaultHideTime = TimeSpan.FromSeconds(2.0);

        public NotificationMediator()
        {
            DeactivateCommand = new DelegateCommand(Deactivate);
            StopTimerCommand = new DelegateCommand(StopTimer);
            ResumeTimerCommand = new DelegateCommand(ResumeTimer);
        }

        private DispatcherTimer timer;

        private bool isActive;

        public bool IsActive
        {
            get { return isActive; }
            private set
            {
                DisposeTimer();
                SetProperty(ref isActive, value);
                //if (!isActive)
                //{
                //    NotificationContent = null;
                //}
            }
        }

        private object notificationContent;

        public object NotificationContent
        {
            get { return notificationContent; }
            private set { SetProperty(ref notificationContent, value); }
        }

        public void Activate(object notificationContent, TimeSpan hideAfter = default(TimeSpan))
        {
            if (notificationContent == null)
            {
                throw new ArgumentNullException("notificationContent");
            }
            NotificationContent = notificationContent;
            IsActive = true;
            if (hideAfter > TimeSpan.Zero)
            {
                timer = new DispatcherTimer(hideAfter, DispatcherPriority.Normal, TimerOnTick, Application.Current.Dispatcher);
            }
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            Deactivate();
        }

        public ICommand DeactivateCommand { get; private set; }

        public void Deactivate()
        {
            IsActive = false;
        }

        public ICommand StopTimerCommand { get; private set; }

        private void StopTimer()
        {
            if (timer != null)
            {
                timer.Stop();
            }
        }

        public ICommand ResumeTimerCommand { get; private set; }

        private void ResumeTimer()
        {
            if (timer != null)
            {
                timer.Start();
            }
        }

        private void DisposeTimer()
        {
            if (timer != null)
            {
                timer.Tick -= TimerOnTick;
                timer.Stop();
                timer = null;
            }
        }
    }
}
