using System;
using System.Windows.Input;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace Registry
{
    public class FreeTimeSlotViewModel : ObservableObject, ITimeInterval
    {
        public FreeTimeSlotViewModel(DateTime startTime, DateTime endTime, int recordTypeId)
        {
            if (startTime >= endTime)
            {
                throw new ArgumentException("Start time must be less than end time");
            }
            StartTime = startTime;
            EndTime = endTime;
            RecordTypeId = recordTypeId;
            RequestAssignmentCreationCommand = new RelayCommand<MouseButtonEventArgs>(RequestAssignmentCreation);
        }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public int RecordTypeId { get; private set; }
        
        public ICommand RequestAssignmentCreationCommand { get; private set; }
        //TODO: make it the other way so that view-model is unaware of mouse buttons
        private void RequestAssignmentCreation(MouseButtonEventArgs args)
        {
            if (args.ChangedButton == MouseButton.Left)
            {
                OnAssignmentCreationRequested();
            }
            args.Handled = true;
        }

        public event EventHandler AssignmentCreationRequested;

        protected virtual void OnAssignmentCreationRequested()
        {
            var handler = AssignmentCreationRequested;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        TimeSpan ITimeInterval.StartTime
        {
            get { return StartTime.TimeOfDay; }
        }

        TimeSpan ITimeInterval.EndTime
        {
            get { return EndTime.TimeOfDay; }
        }
    }
}
