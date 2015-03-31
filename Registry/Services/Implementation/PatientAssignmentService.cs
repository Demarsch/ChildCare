using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using DataLib;

namespace Registry
{
    public class PatientAssignmentService : IPatientAssignmentService
    {
        private readonly IDataContextProvider dataContextProvider;

        public PatientAssignmentService(IDataContextProvider dataContextProvider)
        {
            if (dataContextProvider == null)
                throw new ArgumentNullException("dataContextProvider");
            this.dataContextProvider = dataContextProvider;
        }

        public ICollection<AssignmentDTO> GetAssignments(int patientId)
        {
            using (var dataContext = dataContextProvider.GetNewDataContext())
            {
                return dataContext.GetData<Assignment>().Where(x => x.PersonId == patientId)
                    .Select(x => new AssignmentDTO
                    {
                        AssignDateTime = x.AssignDateTime,
                        IsCanceled = x.CancelUserId.HasValue,
                        IsCompleted = x.RecordId.HasValue && x.Record.IsCompleted,
                        RecordTypeId = x.RecordTypeId,
                        RoomId = x.RoomId
                    }).ToArray();
            }
        }
    }
}