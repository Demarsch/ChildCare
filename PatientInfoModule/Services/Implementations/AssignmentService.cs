using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity.Core.Objects;

namespace PatientInfoModule.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IDbContextProvider contextProvider;

        public AssignmentService(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<Assignment> GetPersonAssignments(int patientId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().Where(x => x.PersonId == patientId && !x.CancelDateTime.HasValue && !x.RecordId.HasValue), context);
        }                    
    }
}
