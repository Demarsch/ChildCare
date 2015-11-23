using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Misc;
using ScheduleModule.DTO;
using Shared.Schedule.Services;

namespace ScheduleModule.Services
{
    public interface IScheduleService : IScheduleServiceBase
    {
        IEnumerable<Room> GetRooms();

        IEnumerable<RecordType> GetRecordTypes();

        ILookup<int, ScheduledAssignmentDTO> GetRoomsAssignments(DateTime date);
        
        IEnumerable<ITimeInterval> GetAvailableTimeIntervals(IEnumerable<ITimeInterval> workingTime, IEnumerable<ITimeInterval> occupiedTime, int nominalDurationInMinutes, int minimumDurationInMinutes);

        void SaveAssignment(Assignment assignment);

        void DeleteAssignment(int assignmentId);

        void CancelAssignment(int assignmentId);

        void UpdateAssignment(int assignmentId, int newFinancingSourceId, string newNote, int? newAssignLpuId);

        void MoveAssignment(int assignmentId, DateTime newTime, int newDuration, int newRoomId);

        IEnumerable<ScheduledAssignmentDTO> GetAssignments(int patientId);

        IEnumerable<ScheduledAssignmentDTO> GetActualAssignments(int patientId, DateTime date);

        ScheduledAssignmentDTO GetAssignment(int assignmentId, int patientId);

        IDisposableQueryable<Person> GetPatientQuery(int currentPatient);

        IEnumerable<Org> GetLpus();
    }
}
