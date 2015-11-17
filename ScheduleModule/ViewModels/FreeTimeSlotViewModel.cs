﻿using System;
using System.Windows.Input;
using Core.Misc;
using Prism.Commands;
using Prism.Mvvm;

namespace ScheduleModule.ViewModels
{
    public class FreeTimeSlotViewModel : BindableBase, ITimeInterval
    {
        public FreeTimeSlotViewModel(DateTime startTime, DateTime endTime, int recordTypeId, int roomId)
        {
            if (startTime >= endTime)
            {
                throw new ArgumentException("Start time must be less than end time");
            }
            StartTime = startTime;
            EndTime = endTime;
            RecordTypeId = recordTypeId;
            RoomId = roomId;
            RequestAssignmentCreationCommand = new DelegateCommand(RequestAssignmentCreation);
        }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public int RecordTypeId { get; private set; }

        public int RoomId { get; private set; }

        public ICommand RequestAssignmentCreationCommand { get; private set; }

        private void RequestAssignmentCreation()
        {
            OnAssignmentCreationRequested();
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