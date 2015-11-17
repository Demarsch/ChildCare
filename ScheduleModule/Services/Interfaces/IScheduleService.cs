﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.Misc;
using ScheduleModule.DTO;

namespace ScheduleModule.Services
{
    public interface IScheduleService
    {
        IEnumerable<Room> GetRooms();

        IEnumerable<RecordType> GetRecordTypes();

        ILookup<int, ScheduledAssignmentDTO> GetRoomsAssignments(DateTime date);

        IEnumerable<ScheduleItem> GetRoomsWorkingTime(DateTime date);

        IEnumerable<ScheduleItem> GetRoomsWeeklyWorkingTime(DateTime date);
        
        IEnumerable<ITimeInterval> GetAvailableTimeIntervals(IEnumerable<ITimeInterval> workingTime, IEnumerable<ITimeInterval> occupiedTime, int nominalDurationInMinutes, int minimumDurationInMinutes);

        void SaveAssignment(Assignment assignment);

        void DeleteAssignment(int assignmentId);

        void CancelAssignment(int assignmentId);

        void UpdateAssignment(int assignmentId, int newFinancingSourceId, string newNote, int? newAssignLpuId);

        void MoveAssignment(int assignmentId, DateTime newTime, int newDuration, int newRoomId);

        void SaveSchedule(ICollection<ScheduleItem> newScheduleItems);

        IEnumerable<ScheduledAssignmentDTO> GetAssignments(int patientId);

        IEnumerable<ScheduledAssignmentDTO> GetActualAssignments(int patientId, DateTime date);

        ScheduledAssignmentDTO GetAssignment(int assignmentId, int patientId);
    }
}
