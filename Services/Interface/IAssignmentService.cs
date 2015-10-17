using System;
using System.Collections.Generic;
using DataLib;

namespace Core
{
    public interface IAssignmentService
    {
        ICollection<Assignment> GetAssignments(int personId = 0, DateTime? fromDate = null, DateTime? toDate = null,
            bool includeCanceled = true);
        ICollection<AssignmentDTO> GetChildAssignments(int parentAssignmentId);
    }
}
