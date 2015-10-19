using System;
using System.Collections.Generic;
using DataLib;

namespace Core
{
    public interface IAssignmentService
    {	
	Assignment GetAssignmentById(int assignmentId);
        ICollection<Assignment> GetAssignments(int personId = 0, DateTime? fromDate = null, DateTime? toDate = null, bool includeCanceled = false);
        ICollection<AssignmentDTO> GetChildAssignments(int parentAssignmentId);
    }
}
