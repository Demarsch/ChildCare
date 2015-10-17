using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;

namespace Core
{
    public class PatientAssignmentService : IPatientAssignmentService
    {
        private readonly IDataContextProvider dataContextProvider;

        public PatientAssignmentService(IDataContextProvider dataContextProvider)
        {
            if (dataContextProvider == null)
            {
                throw new ArgumentNullException("dataContextProvider");
            }
            this.dataContextProvider = dataContextProvider;
        }

        public ICollection<AssignmentScheduleDTO> GetAssignments(int patientId)
        {
            using (var dataContext = dataContextProvider.GetNewDataContext())
            {
                return dataContext.GetData<Assignment>().Where(x => x.PersonId == patientId)
                    .Select(x => new AssignmentScheduleDTO
                    {
                        Id = x.Id,
                        AssignDateTime = x.AssignDateTime,
                        IsCanceled = x.CancelUserId.HasValue,
                        IsCompleted = x.RecordId.HasValue && x.Record.IsCompleted,
                        IsTemporary = x.IsTemporary,
                        RecordTypeId = x.RecordTypeId,
                        RoomId = x.RoomId,
                        Duration = x.Duration
                    }).ToArray();
            }
        }

        public ICollection<AssignmentScheduleDTO> GetActualAssignments(int patientId, DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1.0);
            using (var dataContext = dataContextProvider.GetNewDataContext())
            {
                return dataContext.GetData<Assignment>().Where(x => x.PersonId == patientId && x.CancelUserId == null && x.AssignDateTime >= startDate && x.AssignDateTime < endDate)
                    .Select(x => new AssignmentScheduleDTO
                    {
                        Id = x.Id,
                        AssignDateTime = x.AssignDateTime,
                        IsCanceled = false,
                        IsCompleted = x.RecordId.HasValue && x.Record.IsCompleted,
                        IsTemporary = x.IsTemporary,
                        RecordTypeId = x.RecordTypeId,
                        RoomId = x.RoomId,
                        Duration = x.Duration
                    }).ToArray();
            }
        }

        public AssignmentScheduleDTO GetAssignment(int assignmentId, int patientId)
        {
            using (var dataContext = dataContextProvider.GetNewDataContext())
            {
                return dataContext.GetData<Assignment>().Where(x => x.Id == assignmentId && x.PersonId == patientId)
                    .Select(x => new AssignmentScheduleDTO
                    {
                        Id = x.Id,
                        AssignDateTime = x.AssignDateTime,
                        IsCanceled = x.CancelUserId != null,
                        IsCompleted = x.RecordId.HasValue && x.Record.IsCompleted,
                        IsTemporary = x.IsTemporary,
                        RecordTypeId = x.RecordTypeId,
                        RoomId = x.RoomId,
                        Duration = x.Duration
                    })
                    .FirstOrDefault();
            }
        }
    }
}