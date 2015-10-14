using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;

namespace Core
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IDataContextProvider dataContextProvider;

        public AssignmentService(IDataContextProvider dataContextProvider)
        {
            if (dataContextProvider == null)
                throw new ArgumentNullException("dataContextProvider");
            this.dataContextProvider = dataContextProvider;
        }

        public ICollection<Assignment> GetAssignments(int personId = 0, DateTime? fromDate = null, DateTime? toDate = null, bool includeCanceled = true)
        {
            using (var context = dataContextProvider.GetNewDataContext())
            {
                var result = context.GetData<Assignment>();
                if (personId != 0)
                    result = result.Where(x => x.PersonId == personId);
                if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date == toDate.Value.Date)
                    result = result.Where(x => x.AssignDateTime.Date == fromDate.Value.Date);
                else
                {
                    if (fromDate.HasValue)
                        result = result.Where(x => x.AssignDateTime >= fromDate.Value.Date);
                    if (toDate.HasValue)
                        result = result.Where(x => x.AssignDateTime < toDate.Value.Date.AddDays(1.0));
                }
                if (!includeCanceled)
                    result = result.Where(x => !x.CancelUserId.HasValue);
                return result.ToArray();
            }
        }



        public ICollection<AssignmentDTO> GetChildAssignments(int parentAssignmentId)
        {
            using (var dataContext = dataContextProvider.GetNewDataContext())
            {
                return dataContext.GetData<Assignment>().Where(x => x.ParentId == parentAssignmentId)
                       .Select(x => new AssignmentDTO
                       {
                           Id = x.Id,
                           AssignDateTime = x.AssignDateTime,
                           RecordTypeName = x.RecordType.Name,
                           RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name

                       }).ToList();
            }
        }
    }
}