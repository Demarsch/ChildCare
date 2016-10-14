using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Extensions;

namespace PolyclinicModule.Services
{
    public class PolyclinicService : IPolyclinicService
    {
        private readonly IDbContextProvider contextProvider;

        public PolyclinicService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<Room> GetPolyclinicRooms()
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Room>(context.Set<Room>().AsNoTracking().OrderBy(x => x.Number), context);
        }

        public IDisposableQueryable<Assignment> GetAssignments(DateTime onDate, int roomId)
        {
            var context = contextProvider.CreateNewContext();
            var query = context.Set<Assignment>().Where(x => DbFunctions.TruncateTime(onDate) == DbFunctions.TruncateTime(x.AssignDateTime) && x.RoomId == roomId && !x.RecordId.HasValue).OrderBy(x => x.AssignDateTime);
            return new DisposableQueryable<Assignment>(query, context);
        }

        public IDisposableQueryable<Record> GetRecords(DateTime onDate, int roomId)
        {
            var context = contextProvider.CreateNewContext();
            var query = context.Set<Record>().Where(x => DbFunctions.TruncateTime(onDate) == DbFunctions.TruncateTime(x.BeginDateTime) && x.RoomId == roomId).OrderBy(x => x.BeginDateTime);
            return new DisposableQueryable<Record>(query, context);
        }

        public IDisposableQueryable<Assignment> GetAssignmentById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<Record> GetRecordById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Record>(context.Set<Record>().Where(x => x.Id == id), context);
        }
    }
}
