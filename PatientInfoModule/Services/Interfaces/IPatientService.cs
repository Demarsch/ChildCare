using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using System;
using PatientInfoModule.Data;
using System.Collections;
using PatientInfoModule.Misc;

namespace PatientInfoModule.Services
{
    public interface IPatientService
    {
        IDisposableQueryable<Person> GetPatientQuery(int patientId);

        IEnumerable<string> GetIdentityDocumentGivenOrganizations(string filter);

        IEnumerable<string> GetDisabilityDocumentGivenOrganizations(string filter);
        
        IEnumerable<Country> GetCountries();

        IEnumerable<Education> GetEducations();

        IEnumerable<MaritalStatus> GetMaritalStatuses();

        IEnumerable<HealthGroup> GetHealthGroups();
            
        Task<SavePatientOutput> SavePatientAsync(SavePatientInput data);

        Task<string> CreateAmbCard(int personId);

        Task<bool> DeleteAmbCard(int personId);

        IEnumerable GetPersonsByFullName(string filter);

        IDisposableQueryable<PersonStaff> GetPersonStaff(int personId, int staffId, DateTime begin, DateTime end);

        IDisposableQueryable<PersonStaff> GetAllowedPersonStaffs(int recordTypeId, int memberRoleId, DateTime onDate);

        IDisposableQueryable<PersonOuterDocument> GetPersonOuterDocuments(int personId);

        void DeletePersonOuterDocument(int documentId);

        IDisposableQueryable<Person> GetPersonById(int id);

        bool SavePersonDocument(PersonOuterDocument document);

        IEnumerable<InsuranceCompany> GetInsuranceCompanies(string filter);

        IEnumerable<Org> GetOrganizations(string filter);

        Org GetOrganization(int orgId);

        IEnumerable<RelativeRelationship> GetRelationships();

        RelativeRelationship GetSymmetricalRelationship(RelativeRelationship relationship, bool symmetricalRelationshipIsMale);

        Task<IEnumerable<PersonRelative>> GetRelativesAsync(int patientId);

        Task<Person> CheckIfSimilarPatientExistsAsync(DuplicatePersonCheckParameters param);

        string GetDBSettingValue(string parameter);
    }
}
