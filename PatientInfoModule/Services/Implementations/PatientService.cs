using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Misc;
using Core.Services;
using PatientInfoModule.Data;

namespace PatientInfoModule.Services
{
    public sealed class PatientService : IPatientService
    {
        private readonly IDbContextProvider contextProvider;

        private readonly ICacheService cacheService;

        public PatientService(IDbContextProvider contextProvider, ICacheService cacheService)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            this.contextProvider = contextProvider;
            this.cacheService = cacheService;
        }

        public IDisposableQueryable<Person> GetPatientQuery(int patientId)
        {
            var context = contextProvider.CreateNewContext();
            context.Configuration.ProxyCreationEnabled = false;
            return new DisposableQueryable<Person>(context.Set<Person>().AsNoTracking().Where(x => x.Id == patientId), context);
        }

        public IEnumerable<string> GetIdentityDocumentGivenOrganizations(string filter)
        {
            filter = (filter ?? string.Empty).Trim();
            if (filter.Length < AppConfiguration.UserInputSearchThreshold)
            {
                return new string[0];
            }
            using (var context = contextProvider.CreateNewContext())
            {
                return context.Set<PersonIdentityDocument>()
                              .Where(x => x.GivenOrg.Contains(filter))
                              .Select(x => x.GivenOrg)
                              .Distinct()
                              .ToArray();
            }
        }

        public IEnumerable<string> GetDisabilityDocumentGivenOrganizations(string filter)
        {
            filter = (filter ?? string.Empty).Trim();
            if (filter.Length < AppConfiguration.UserInputSearchThreshold)
            {
                return new string[0];
            }
            using (var context = contextProvider.CreateNewContext())
            {
                return context.Set<PersonDisability>()
                              .Where(x => x.GivenOrg.Contains(filter))
                              .Select(x => x.GivenOrg)
                              .Distinct()
                              .ToArray();
            }
        }

        public IEnumerable<InsuranceCompany> GetInsuranceCompanies(string filter)
        {
            filter = (filter ?? string.Empty).Trim();
            if (filter.Length < AppConfiguration.UserInputSearchThreshold)
            {
                return new InsuranceCompany[0];
            }
            var words = filter.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToArray();
            return cacheService.GetItems<InsuranceCompany>().Where(x => words.All(y => x.NameSMOK.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1
                                                                                       || x.AddressF.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1));
        }

        public IEnumerable<Org> GetOrganizations(string filter)
        {
            filter = (filter ?? string.Empty).Trim();
            if (filter.Length < AppConfiguration.UserInputSearchThreshold)
            {
                return new Org[0];
            }
            var words = filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            using (var contex = contextProvider.CreateNewContext())
            {
                return contex.Set<Org>().Where(x => words.All(y => x.Name.Contains(y) || x.Details.Contains(y)))
                                        .ToArray();
            }
        }

        public IEnumerable<Country> GetCountries()
        {
            return cacheService.GetItems<Country>();
        }

        public IEnumerable<Education> GetEducations()
        {
            return cacheService.GetItems<Education>();
        }

        public IEnumerable<MaritalStatus> GetMaritalStatuses()
        {
            return cacheService.GetItems<MaritalStatus>();
        }

        public IEnumerable<HealthGroup> GetHealthGroups()
        {
            return cacheService.GetItems<HealthGroup>();
        }

        public IEnumerable<RelativeRelationship> GetRelationships()
        {
            return cacheService.GetItems<RelativeRelationship>();
        }

        public async Task<IEnumerable<PersonRelative>> GetRelativesAsync(int patientId)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                return await context.Set<PersonRelative>().Where(x => x.PersonId == patientId).ToArrayAsync();
            }
        }

        #region SavePatientAsync

        public async Task<SavePatientOutput> SavePatientAsync(SavePatientInput data, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(token);
            }
            using (var context = contextProvider.CreateNewContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                var result = new SavePatientOutput
                             {
                                 Person = data.CurrentPerson,
                                 Name = data.IsNewName ? data.NewName : data.CurrentName
                             };
                PreparePerson(data, context);
                PrepareNationality(data, context, result);
                PrepareEducation(data, context, result);
                PrepareHealthGroup(data, context, result);
                PrepareMaritalStatus(data, context, result);
                PrepareIdentityDocuments(data, context, result);
                PrepareInsuranceDocuments(data, context, result);
                PrepareAddresses(data, context, result);
                PrepareDisabilityDocuments(data, context, result);
                PrepareSocialStatuses(data, context, result);
                PrepareRelativeRelationship(data, context, result);
                if (token.IsCancellationRequested)
                {
                    throw new OperationCanceledException(token);
                }
                await context.SaveChangesAsync(token);
                return result;
            }
        }

        private void PrepareRelativeRelationship(SavePatientInput data, DbContext context, SavePatientOutput result)
        {
            if (data.Relative == null)
            {
                result.Relative = null;
                return;
            }
            result.Relative = data.Relative;
            if (data.Relative.Id.IsNewOrNonExisting())
            {
                data.Relative.Person1 = data.CurrentPerson;
                context.Entry(data.Relative).State = EntityState.Added;
            }
            else
            {
                context.Entry(data.Relative).State = EntityState.Modified;
            }
        }

        private void PrepareSocialStatuses(SavePatientInput data, DbContext context, SavePatientOutput result)
        {
            var old = data.CurrentSocialStatuses.ToDictionary(x => x.Id);
            var @new = data.NewSocialStatuses.Where(x => x.Id != SpecialValues.NewId).ToDictionary(x => x.Id);
            var added = data.NewSocialStatuses.Where(x => x.Id == SpecialValues.NewId).ToArray();
            var removed = old.Where(x => !@new.ContainsKey(x.Key))
                             .Select(removedDocument => removedDocument.Value)
                             .ToArray();
            var existed = @new.Where(x => old.ContainsKey(x.Key))
                              .Select(x => new { Old = old[x.Key], New = x.Value, IsChanged = !x.Value.Equals(old[x.Key]) })
                              .ToArray();
            foreach (var document in added)
            {
                document.Person = data.CurrentPerson;
                context.Entry(document).State = EntityState.Added;
                if (document.OrgId == SpecialValues.NewId && document.Org != null)
                {
                    context.Entry(document.Org).State = EntityState.Added;
                }
            }
            foreach (var document in removed)
            {
                context.Entry(document).State = EntityState.Deleted;
            }
            foreach (var document in existed.Where(x => x.IsChanged))
            {
                context.Entry(document.New).State = EntityState.Modified;
                if (document.New.OrgId == SpecialValues.NewId && document.New.Org != null)
                {
                    context.Entry(document.New.Org).State = EntityState.Added;
                }
            }
            result.SocialStatuses = added.Concat(existed.Select(x => x.New)).ToArray();
        }

        private void PrepareDisabilityDocuments(SavePatientInput data, DbContext context, SavePatientOutput result)
        {
            var old = data.CurrentDisabilities.ToDictionary(x => x.Id);
            var @new = data.NewDisabilities.Where(x => x.Id != SpecialValues.NewId).ToDictionary(x => x.Id);
            var added = data.NewDisabilities.Where(x => x.Id == SpecialValues.NewId).ToArray();
            var removed = old.Where(x => !@new.ContainsKey(x.Key))
                             .Select(removedDocument => removedDocument.Value)
                             .ToArray();
            var existed = @new.Where(x => old.ContainsKey(x.Key))
                              .Select(x => new { Old = old[x.Key], New = x.Value, IsChanged = !x.Value.Equals(old[x.Key]) })
                              .ToArray();
            foreach (var document in added)
            {
                document.Person = data.CurrentPerson;
                context.Entry(document).State = EntityState.Added;
            }
            foreach (var document in removed)
            {
                context.Entry(document).State = EntityState.Deleted;
            }
            foreach (var document in existed.Where(x => x.IsChanged))
            {
                context.Entry(document.New).State = EntityState.Modified;
            }
            result.DisabilityDocuments = added.Concat(existed.Select(x => x.New)).ToArray();
        }

        private static void PrepareAddresses(SavePatientInput data, DbContext context, SavePatientOutput result)
        {
            var old = data.CurrentAddresses.ToDictionary(x => x.Id);
            var @new = data.NewAddresses.Where(x => x.Id != SpecialValues.NewId).ToDictionary(x => x.Id);
            var added = data.NewAddresses.Where(x => x.Id == SpecialValues.NewId).ToArray();
            var removed = old.Where(x => !@new.ContainsKey(x.Key))
                             .Select(removedDocument => removedDocument.Value)
                             .ToArray();
            var existed = @new.Where(x => old.ContainsKey(x.Key))
                              .Select(x => new { Old = old[x.Key], New = x.Value, IsChanged = !x.Value.Equals(old[x.Key]) })
                              .ToArray();
            foreach (var address in added)
            {
                address.Person = data.CurrentPerson;
                context.Entry(address).State = EntityState.Added;
            }
            foreach (var address in removed)
            {
                context.Entry(address).State = EntityState.Deleted;
            }
            foreach (var address in existed.Where(x => x.IsChanged))
            {
                context.Entry(address.New).State = EntityState.Modified;
            }
            result.Addresses = added.Concat(existed.Select(x => x.New)).ToArray();
        }

        private static void PrepareInsuranceDocuments(SavePatientInput data, DbContext context, SavePatientOutput result)
        {
            var old = data.CurrentInsuranceDocuments.ToDictionary(x => x.Id);
            var @new = data.NewInsuranceDocuments.Where(x => x.Id != SpecialValues.NewId).ToDictionary(x => x.Id);
            var added = data.NewInsuranceDocuments.Where(x => x.Id == SpecialValues.NewId).ToArray();
            var removed = old.Where(x => !@new.ContainsKey(x.Key))
                             .Select(removedDocument => removedDocument.Value)
                             .ToArray();
            var existed = @new.Where(x => old.ContainsKey(x.Key))
                              .Select(x => new { Old = old[x.Key], New = x.Value, IsChanged = !x.Value.Equals(old[x.Key]) })
                              .ToArray();
            foreach (var document in added)
            {
                document.Person = data.CurrentPerson;
                context.Entry(document).State = EntityState.Added;
            }
            foreach (var document in removed)
            {
                context.Entry(document).State = EntityState.Deleted;
            }
            foreach (var document in existed.Where(x => x.IsChanged))
            {
                context.Entry(document.New).State = EntityState.Modified;
            }
            result.InsuranceDocuments = added.Concat(existed.Select(x => x.New)).ToArray();
        }

        private static void PrepareIdentityDocuments(SavePatientInput data, DbContext context, SavePatientOutput result)
        {
            var old = data.CurrentIdentityDocuments.ToDictionary(x => x.Id);
            var @new = data.NewIdentityDocuments.Where(x => x.Id != SpecialValues.NewId).ToDictionary(x => x.Id);
            var added = data.NewIdentityDocuments.Where(x => x.Id == SpecialValues.NewId).ToArray();
            var removed = old.Where(x => !@new.ContainsKey(x.Key))
                             .Select(removedDocument => removedDocument.Value)
                             .ToArray();
            var existed = @new.Where(x => old.ContainsKey(x.Key))
                              .Select(x => new { Old = old[x.Key], New = x.Value, IsChanged = !x.Value.Equals(old[x.Key]) })
                              .ToArray();
            foreach (var document in added)
            {
                document.Person = data.CurrentPerson;
                context.Entry(document).State = EntityState.Added;
            }
            foreach (var document in removed)
            {
                context.Entry(document).State = EntityState.Deleted;
            }
            foreach (var document in existed.Where(x => x.IsChanged))
            {
                context.Entry(document.New).State = EntityState.Modified;
            }
            result.IdentityDocuments = added.Concat(existed.Select(x => x.New)).ToArray();
        }

        private static void PrepareMaritalStatus(SavePatientInput data, DbContext context, SavePatientOutput result)
        {
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
        }

        private static void PrepareHealthGroup(SavePatientInput data, DbContext context, SavePatientOutput result)
        {
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
        }

        private static void PrepareEducation(SavePatientInput data, DbContext context, SavePatientOutput result)
        {
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
        }

        private static void PrepareNationality(SavePatientInput data, DbContext context, SavePatientOutput result)
        {
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
        }

        private static void PreparePerson(SavePatientInput data, DbContext context)
        {
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
        }

        #endregion

        public IEnumerable GetPersonsByFullName(string filter)
        {
            filter = (filter ?? string.Empty).Trim();
            if (filter.Length < AppConfiguration.UserInputSearchThreshold)
            {
                return new Person[0];
            }
            var words = (filter.Contains(',') ? filter.Split(',')[0] : filter).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            return cacheService.GetItems<Person>().Where(x => words.All(y => x.FullName.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1));
        }

        public Person GetPersonById(int id)
        {
            var context = contextProvider.CreateNewContext();
            return cacheService.GetItems<Person>().FirstOrDefault(x => x.Id == id);
        }

        public Org GetOrganization(int orgId)
        {
            using (var context = contextProvider.CreateNewContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
                return context.Set<Org>().FirstOrDefault(x => x.Id == orgId);
            }
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
            var personOuterDocument = context.Set<PersonOuterDocument>().FirstOrDefault(x => x.DocumentId == documentId);
            if (personOuterDocument != null)
            {
                context.Entry(personOuterDocument).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        public bool SavePersonDocument(PersonOuterDocument document)
        {
            using (var db = contextProvider.CreateNewContext())
            {
                var saveDocument = document.Id == SpecialValues.NewId ? new PersonOuterDocument() : db.Set<PersonOuterDocument>().First(x => x.Id == document.Id);
                saveDocument.PersonId = document.PersonId;
                saveDocument.OuterDocumentTypeId = document.OuterDocumentTypeId;
                saveDocument.DocumentId = document.DocumentId;
                db.Entry(saveDocument).State = saveDocument.Id == SpecialValues.NewId ? EntityState.Added : EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}