using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core;
using DataLib;
using GalaSoft.MvvmLight;

namespace Registry
{
    public class RoomViewModel : ObservableObject
    {
        private readonly Room room;

        private readonly ICacheService cacheService;

        public RoomViewModel(Room room, ICacheService cacheService)
        {
            if (room == null)
                throw new ArgumentNullException("room");
            if (cacheService == null)
                throw new ArgumentNullException("cacheService");
            this.cacheService = cacheService;
            this.room = room;
        }

        public int Id { get { return room.Id; } }

        public string Number { get { return room.Number; } }

        public string Name { get { return room.Name; } }

        private bool isInDetailedMode;

        public bool IsInDetailedMode
        {
            get { return isInDetailedMode; }
            set { Set("IsInDetailedMode", ref isInDetailedMode, value); }
        }

        internal void SetAssignments(IEnumerable<ScheduledAssignmentDTO> assignments)
        {
            var recordTypesList = assignments
                .GroupBy(x => x.RecordTypeId)
                .Select(x => new RecordTypeViewModel(cacheService.GetItemById<RecordType>(x.Key).Name, x.Select(y => new ScheduledAssignmentViewModel(y, cacheService)).ToArray()))
                .ToList();
            var startTime = TimeSpan.MinValue;
            var endTime = TimeSpan.MinValue;
            var patientSummary = new List<string>();
            var assignmentGroupsList = new List<ScheduledAssignmentGroupViewModel>();
            foreach (var assignment in recordTypesList.SelectMany(x => x.Assignments).OrderBy(x => x.StartTime))
            {
                if (startTime == TimeSpan.MinValue)
                {
                    startTime = assignment.StartTime;
                    endTime = assignment.EndTime;
                    patientSummary.Add(assignment.PersonShortName);
                }
                else if (assignment.StartTime < endTime)
                {
                    endTime = assignment.EndTime > endTime ? assignment.EndTime : endTime;
                    patientSummary.Add(assignment.PersonShortName);
                }
                else
                {
                    assignmentGroupsList.Add(new ScheduledAssignmentGroupViewModel(startTime, endTime, string.Join(Environment.NewLine, patientSummary.Distinct().OrderBy(x => x))));
                    startTime = TimeSpan.MinValue;
                    endTime = TimeSpan.MinValue;
                    patientSummary.Clear();
                }
            }
            AssignmentGroups = new ObservableCollection<ScheduledAssignmentGroupViewModel>(assignmentGroupsList);
            RecordTypes = new ObservableCollection<RecordTypeViewModel>(recordTypesList);
        }

        private ObservableCollection<RecordTypeViewModel> recordTypes;

        public ObservableCollection<RecordTypeViewModel> RecordTypes
        {
            get { return recordTypes; }
            set { Set("RecordTypes", ref recordTypes, value); }
        }

        private ObservableCollection<ScheduledAssignmentGroupViewModel> assignmentGroups;

        public ObservableCollection<ScheduledAssignmentGroupViewModel> AssignmentGroups
        {
            get { return assignmentGroups; }
            set { Set("AssignmentGroups", ref assignmentGroups, value); }
        }
    }
}