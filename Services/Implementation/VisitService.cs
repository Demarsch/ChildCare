using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;
using System.Data.Entity.Core.Objects;

namespace Core
{
    public class VisitService : IVisitService
    {
        private readonly IDataContextProvider provider;

        public VisitService(IDataContextProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            this.provider = provider;
        }

        public ICollection<Core.PersonVisitItemsListViewModels.RecordDTO> GetChildRecords(int visitId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Record>().Where(x => x.VisitId == visitId).Select(x => new Core.PersonVisitItemsListViewModels.RecordDTO()
                {
                    Id = x.Id,
                    BeginDateTime = x.BeginDateTime,
                    EndDateTime = x.EndDateTime,
                    RecordTypeName = x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                    IsCompleted = x.IsCompleted
                }).ToList();
            }
        }

        public ICollection<AssignmentDTO> GetChildAssignments(int visitId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Assignment>().Where(x => x.VisitId == visitId && !x.RecordId.HasValue).Select(x => new AssignmentDTO()
                {
                    Id = x.Id,
                    AssignDateTime = x.AssignDateTime,
                    RecordTypeName = x.RecordType.Name,
                    RoomName = (x.Room.Number != string.Empty ? x.Room.Number + " - " : string.Empty) + x.Room.Name,
                }).ToList();
            }
        }
    }
}
