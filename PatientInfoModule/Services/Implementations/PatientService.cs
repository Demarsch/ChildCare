using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace PatientInfoModule.Services
{
    public class PatientService : IPatientService
    {
        private readonly IDbContextProvider contextProvider;

        public PatientService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<Person> GetPatientQuery(int patientId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(context.Set<Person>().AsNoTracking().Where(x => x.Id == patientId), context);
        }
    }
}
