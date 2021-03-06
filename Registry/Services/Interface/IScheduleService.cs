﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;

namespace Registry
{
    public interface IScheduleService
    {
        ICollection<Room> GetRooms();

        ICollection<RecordType> GetRecordTypes();

        ILookup<int, ScheduledAssignmentDTO> GetRoomsAssignments(DateTime date);

        ICollection<ScheduleItem> GetRoomsWorkingTime(DateTime date);

        ICollection<ScheduleItem> GetRoomsWeeklyWorkingTime(DateTime date);
        
        IEnumerable<ITimeInterval> GetAvailableTimeIntervals(IEnumerable<ITimeInterval> workingTime, IEnumerable<ITimeInterval> occupiedTime, int nominalDurationInMinutes, int minimumDurationInMinutes);

        void SaveAssignment(Assignment assignment);

        void DeleteAssignment(int assignmentId);

        void CancelAssignment(int assignmentId);

        void UpdateAssignment(int assignmentId, int newFinancingSourceId, string newNote, int? newAssignLpuId);

        void MoveAssignment(int assignmentId, DateTime newTime, int newDuration, int newRoomId);

        void SaveSchedule(ICollection<ScheduleItem> newScheduleItems);
    }
}
