using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace PatientRecordsModule.Services
{
    public class PatientRecordsService : IPatientRecordsService
    {
        private readonly IDbContextProvider contextProvider;

        public PatientRecordsService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<Person> GetPersonQuery(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(context.Set<Person>().AsNoTracking().Where(x => x.Id == personId), context);
        }

        public IDisposableQueryable<Record> GetPersonRecordsQuery(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Record>(context.Set<Record>().AsNoTracking().Where(x => x.PersonId == personId), context);
        }

        public IDisposableQueryable<Assignment> GetPersonRootAssignmentsQuery(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().AsNoTracking().Where(x => x.PersonId == personId && !x.VisitId.HasValue && !x.RecordId.HasValue), context);
        }

        public IDisposableQueryable<Visit> GetPersonVisitsQuery(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Visit>(context.Set<Visit>().AsNoTracking().Where(x => x.PersonId == personId), context);
        }
    }
}
