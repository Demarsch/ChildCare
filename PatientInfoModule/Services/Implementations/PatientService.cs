using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using PatientInfoModule.Data;

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
            context.Configuration.ProxyCreationEnabled = false;
            return new DisposableQueryable<Person>(context.Set<Person>().AsNoTracking().Where(x => x.Id == patientId), context);
        }

        public IQueryable<Country> GetCountries()
        {
            return contextProvider.SharedContext.Set<Country>();
        }

        public IQueryable<Education> GetEducations()
        {
            return contextProvider.SharedContext.Set<Education>();
        }

        public IQueryable<MaritalStatus> GetMaritalStatuses()
        {
            return contextProvider.SharedContext.Set<MaritalStatus>();
        }

        public IQueryable<HealthGroup> GetHealthGroups()
        {
            return contextProvider.SharedContext.Set<HealthGroup>();
        }

        public async Task<SavePatientOutput> SavePatientAsync(SavePatientInput data, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                var result = new SavePatientOutput
                             {
                                 Person = data.CurrentPerson, 
                                 Name = data.IsNewName ? data.NewName : data.CurrentName
                             };
                data.CurrentPerson.FullName = data.NewName.FullName;
                data.CurrentPerson.ShortName = data.NewName.ShortName;
                context.Entry(data.CurrentPerson).State = data.CurrentPerson.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;
                //We already have current name, we should just copy new values to it
                if (data.IsIncorrectName)
                {
                    data.CurrentName.LastName = data.NewName.LastName;
                    data.CurrentName.FirstName = data.NewName.FirstName;
                    data.CurrentName.MiddleName = data.NewName.MiddleName;
                    context.Entry(data.CurrentName).State = EntityState.Modified;
                }
                //We should create new name whether because of real name change or because of new person
                else if (data.IsNewName)
                {
                    context.Entry(data.NewName).State = EntityState.Added;
                    //This is existing person - we should mark old name as no longer active
                    if (data.CurrentName != null)
                    {
                        data.CurrentName.EndDateTime = data.NewNameStartDate.AddDays(-1.0);
                        data.NewName.BeginDateTime = data.NewNameStartDate;
                        context.Entry(data.CurrentName).State = EntityState.Modified;
                    }
                    else
                    {
                        data.NewName.BeginDateTime = SpecialValues.MinDate;
                    }
                    data.NewName.EndDateTime = SpecialValues.MaxDate;
                    data.NewName.Person = data.CurrentPerson;
                }
                //Nationality
                if (data.CurrentNationality == null)
                {
                    if (data.NewNationality.CountryId != SpecialValues.NonExistingId)
                    {
                        data.NewNationality.BeginDateTime = SpecialValues.MinDate;
                        data.NewNationality.EndDateTime = SpecialValues.MaxDate;
                        data.NewNationality.Person = data.CurrentPerson;
                        context.Entry(data.NewNationality).State = EntityState.Added;
                        result.Nationality = data.NewNationality;
                    }
                }
                else if (data.CurrentNationality.CountryId != data.NewNationality.CountryId)
                {
                    data.CurrentNationality.EndDateTime = DateTime.Today.AddDays(-1.0);
                    data.NewNationality.BeginDateTime = DateTime.Today;
                    data.NewNationality.EndDateTime = SpecialValues.MaxDate;
                    data.NewNationality.Person = data.CurrentPerson;
                    context.Entry(data.NewNationality).State = EntityState.Added;
                    context.Entry(data.CurrentNationality).State = EntityState.Modified;
                    result.Nationality = data.NewNationality;
                }
                else
                {
                    result.Nationality = data.CurrentNationality;
                }
                //Education
                if (data.CurrentEducation == null)
                {
                    if (data.NewEducation.EducationId != SpecialValues.NonExistingId)
                    {
                        data.NewEducation.BeginDateTime = SpecialValues.MinDate;
                        data.NewEducation.EndDateTime = SpecialValues.MaxDate;
                        data.NewEducation.Person = data.CurrentPerson;
                        context.Entry(data.NewEducation).State = EntityState.Added;
                        result.Education = data.NewEducation;
                    }
                }
                else if (data.CurrentEducation.EducationId != data.NewEducation.EducationId)
                {
                    data.CurrentEducation.EndDateTime = DateTime.Today.AddDays(-1.0);
                    data.NewEducation.BeginDateTime = DateTime.Today;
                    data.NewEducation.EndDateTime = SpecialValues.MaxDate;
                    data.NewEducation.Person = data.CurrentPerson;
                    context.Entry(data.NewEducation).State = EntityState.Added;
                    context.Entry(data.CurrentEducation).State = EntityState.Modified;
                    result.Education = data.NewEducation;
                }
                else
                {
                    result.Education = data.CurrentEducation;
                }
                //HealthGroup
                if (data.CurrentHealthGroup == null)
                {
                    if (data.NewHealthGroup.HealthGroupId != SpecialValues.NonExistingId)
                    {
                        data.NewHealthGroup.BeginDateTime = SpecialValues.MinDate;
                        data.NewHealthGroup.EndDateTime = SpecialValues.MaxDate;
                        data.NewHealthGroup.Person = data.CurrentPerson;
                        context.Entry(data.NewHealthGroup).State = EntityState.Added;
                        result.HealthGroup = data.NewHealthGroup;
                    }
                }
                else if (data.CurrentHealthGroup.HealthGroupId != data.NewHealthGroup.HealthGroupId)
                {
                    //Health group have special treatment as unlike other properties is can become empty
                    data.CurrentHealthGroup.EndDateTime = DateTime.Today.AddDays(-1.0);
                    context.Entry(data.CurrentHealthGroup).State = EntityState.Modified;
                    if (data.NewHealthGroup.HealthGroupId == SpecialValues.NonExistingId)
                    {
                        result.HealthGroup = null;
                    }
                    else
                    {
                        data.NewHealthGroup.BeginDateTime = DateTime.Today;
                        data.NewHealthGroup.EndDateTime = SpecialValues.MaxDate;
                        data.NewHealthGroup.Person = data.CurrentPerson;
                        context.Entry(data.NewHealthGroup).State = EntityState.Added;
                        result.HealthGroup = data.NewHealthGroup;
                    }
                }
                else
                {
                    result.HealthGroup = data.CurrentHealthGroup;
                }
                //MaritalStatus
                if (data.CurrentMaritalStatus == null)
                {
                    if (data.NewMaritalStatus.MaritalStatusId != SpecialValues.NonExistingId)
                    {
                        data.NewMaritalStatus.BeginDateTime = SpecialValues.MinDate;
                        data.NewMaritalStatus.EndDateTime = SpecialValues.MaxDate;
                        data.NewMaritalStatus.EndDateTime = SpecialValues.MaxDate;
                        data.NewMaritalStatus.Person = data.CurrentPerson;
                        context.Entry(data.NewMaritalStatus).State = EntityState.Added;
                        result.MaritalStatus = data.NewMaritalStatus;
                    }
                }
                else if (data.CurrentMaritalStatus.MaritalStatusId != data.NewMaritalStatus.MaritalStatusId)
                {
                    data.CurrentMaritalStatus.EndDateTime = DateTime.Today.AddDays(-1.0);
                    data.NewMaritalStatus.BeginDateTime = DateTime.Today;
                    data.NewMaritalStatus.Person = data.CurrentPerson;
                    context.Entry(data.NewMaritalStatus).State = EntityState.Added;
                    context.Entry(data.CurrentMaritalStatus).State = EntityState.Modified;
                    result.MaritalStatus = data.NewMaritalStatus;
                }
                else
                {
                    result.MaritalStatus = data.CurrentMaritalStatus;
                }
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);
                return result;
            }
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
                                                               .Where(x => x.PersonId == personId && x.StaffId == staffId
                                                                           && DbFunctions.TruncateTime(x.BeginDateTime) <= DbFunctions.TruncateTime(end)
                                                                           && DbFunctions.TruncateTime(x.EndDateTime) >= DbFunctions.TruncateTime(begin)), context);
        }

        public IDisposableQueryable<PersonStaff> GetAllowedPersonStaffs(int recordTypeId, int memberRoleId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonStaff>(context.Set<RecordTypeRolePermission>()
                                                               .Where(x => x.RecordTypeId == recordTypeId && x.RecordTypeMemberRoleId == memberRoleId)
                                                               .SelectMany(x => x.Permission.UserPermissions.SelectMany(a => a.User.Person.PersonStaffs)), context);
        }

        public IDisposableQueryable<PersonOuterDocument> GetPersonOuterDocuments(int personId)
        {
            var context = contextProvider.CreateNewContext();
            return new DisposableQueryable<PersonOuterDocument>(context.Set<PersonOuterDocument>().Where(x => x.PersonId == personId), context);
        }

        public void DeletePersonOuterDocument(int documentId)
        {
            var context = contextProvider.CreateNewContext();
            //var personOuterDoc = db.GetData<PersonOuterDocument>().FirstOrDefault(x => x.DocumentId == documentId);
            //if (personOuterDoc == null) return;
            //db.Remove<PersonOuterDocument>(personOuterDoc);
            //db.Save();
        }
    }
}
