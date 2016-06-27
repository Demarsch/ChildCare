using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using Core.Misc;
using Core.Notification;
using ScheduleModule.DTO;
using Shared.Schedule.Services;

namespace ScheduleModule.Services
{
    public interface IScheduleService : IScheduleServiceBase
    {
        ILookup<int, ScheduledAssignmentDTO> GetRoomsAssignments(DateTime date);
        
        IEnumerable<ITimeInterval> GenerateAvailableTimeIntervals(IEnumerable<ITimeInterval> workingTime, IEnumerable<ITimeInterval> occupiedTime, int nominalDurationInMinutes, int minimumDurationInMinutes);

        Task SaveAssignmentAsync(Assignment newAssignment, INotificationServiceSubscription<Assignment> assignmentChangeSubscription);

        Task DeleteAssignmentAsync(int assignmentId, INotificationServiceSubscription<Assignment> assignmentChangeSubscription);

        Task CancelAssignmentAsync(int assignmentId, INotificationServiceSubscription<Assignment> assignmentChangeSubscription);

        Task UpdateAssignmentAsync(int assignmentId, int newFinancingSourceId, string newNote, int? newAssignLpuId, INotificationServiceSubscription<Assignment> assignmentChangeSubscription);

        Task MoveAssignmentAsync(int assignmentId, DateTime newTime, int newDuration, Room newRoom, INotificationServiceSubscription<Assignment> assignmentChangeSubscription);

        IEnumerable<ScheduledAssignmentDTO> GetAssignments(int patientId);

        IEnumerable<ScheduledAssignmentDTO> GetActualAssignments(int patientId, DateTime date);

        Task<ScheduledAssignmentDTO> GetAssignmentAsync(int assignmentId, int patientId);

        IDisposableQueryable<Person> GetPatientQuery(int currentPatient);

        IEnumerable<Org> GetLpus();

        Task MarkAssignmentCompletedAsync(int id, DateTime selectedDateTime, INotificationServiceSubscription<Assignment> assignmentChangeSubscription);
    }
}
