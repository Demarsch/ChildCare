using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace Shared.Patient.Services
{
    public class PatientAssignmentService : IPatientAssignmentService
    {
        private readonly IDbContextProvider contextProvider;

        private readonly IEnvironment environment;

        public PatientAssignmentService(IDbContextProvider contextProvider, IEnvironment environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
            this.environment = environment;
        }

        public IDisposableQueryable<Assignment> GetAssignmentsQuery(int patientId)
        {
            var context = contextProvider.CreateLightweightContext();
            return new DisposableQueryable<Assignment>(context.Set<Assignment>().Where(x => x.PersonId == patientId), context);
        }

        public async Task CancelAssignmentAsync(int assignmentId)
        {
            using (var context = contextProvider.CreateLightweightContext())
            {
                var assignment = await context.Set<Assignment>().FirstAsync(x => x.Id == assignmentId);
                assignment.CancelDateTime = environment.CurrentDate;
                assignment.CancelUserId = environment.CurrentUser.Id;
                context.ChangeTracker.DetectChanges();
                await context.SaveChangesAsync();
            }
        }     

        public IDisposableQueryable<Person> GetPersonById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(context.Set<Person>().Where(x => x.Id == id), context);
        }

        public IDisposableQueryable<RecordContract> GetContractById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<RecordContract>(context.Set<RecordContract>().Where(x => x.Id == id), context);
        }

        public string GetDBSettingValue(string parameter, bool useDisplayName = false)
        {
            var setting = contextProvider.CreateNewContext().Set<DBSetting>().FirstOrDefault(x => x.Name == parameter);
            if (setting != null)
                return (useDisplayName ? setting.DisplayName : setting.Value);
            return string.Empty;
        }
    }
}
