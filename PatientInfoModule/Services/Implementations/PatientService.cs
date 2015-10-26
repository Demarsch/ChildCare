using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using System.Data.Entity.Core.Objects;

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

        public IDisposableQueryable<Person> GetPersonsByFullName(string fullname)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(context.Set<Person>().Where(x => x.FullName.StartsWith(fullname)), context);
        } 

        public IDisposableQueryable<PersonStaff> GetPersonStaff(int personId, int staffId, DateTime begin, DateTime end)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonStaff>(context.Set<PersonStaff>()
                .Where(x => x.PersonId == personId && x.StaffId == staffId && 
                    EntityFunctions.TruncateTime(x.BeginDateTime) <= EntityFunctions.TruncateTime(end) && 
                    EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(begin)), context);
             
        }

        public IDisposableQueryable<PersonStaff> GetAllowedPersonStaffs(int recordTypeId, int memberRoleId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonStaff>(context.Set<RecordTypeRolePermission>()
                .Where(x => x.RecordTypeId == recordTypeId && x.RecordTypeMemberRoleId == memberRoleId)
                    .SelectMany(x => x.Permission.UserPermissions.SelectMany(a => a.User.Person.PersonStaffs)), context);
        }
    }
}
