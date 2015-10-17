using System.Collections.Generic;
using DataLib;
using System;
using Core;

namespace Core
{
    public interface IVisitService
    {
        ICollection<PersonVisitItemsListViewModels.RecordDTO> GetChildRecords(int visitId);
        ICollection<AssignmentDTO> GetChildAssignments(int visitId);
    }
}
