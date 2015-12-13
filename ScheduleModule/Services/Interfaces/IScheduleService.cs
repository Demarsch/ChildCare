using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        Task SaveAssignmentAsync(Assignment assignment);

        Task DeleteAssignmentAsync(int assignmentId);

        Task CancelAssignmentAsync(int assignmentId);

        Task UpdateAssignmentAsync(int assignmentId, int newFinancingSourceId, string newNote, int? newAssignLpuId);

        Task MoveAssignmentAsync(int assignmentId, DateTime newTime, int newDuration, Room newRoom);

        IEnumerable<ScheduledAssignmentDTO> GetAssignments(int patientId);

        IEnumerable<ScheduledAssignmentDTO> GetActualAssignments(int patientId, DateTime date);

        ScheduledAssignmentDTO GetAssignment(int assignmentId, int patientId);

        IDisposableQueryable<Person> GetPatientQuery(int currentPatient);

        IEnumerable<Org> GetLpus();
    }
}
