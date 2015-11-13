using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using System;
using PatientInfoModule.Data;
using System.Collections;

namespace PatientInfoModule.Services
{
    public interface IPatientService
    {
        IDisposableQueryable<Person> GetPatientQuery(int patientId);

        IDisposableQueryable<string> GetDocumentGivenOrganizations(string filter);
        
        IEnumerable<Country> GetCountries();

        IEnumerable<Education> GetEducations();

        IEnumerable<MaritalStatus> GetMaritalStatuses();

        IEnumerable<HealthGroup> GetHealthGroups();
            
        Task<SavePatientOutput> SavePatientAsync(SavePatientInput data, CancellationToken token);

        IEnumerable GetPersonsByFullName(string filter);

        IDisposableQueryable<PersonStaff> GetPersonStaff(int personId, int staffId, DateTime begin, DateTime end);

        IDisposableQueryable<PersonStaff> GetAllowedPersonStaffs(int recordTypeId, int memberRoleId);

        IDisposableQueryable<PersonOuterDocument> GetPersonOuterDocuments(int personId);

        void DeletePersonOuterDocument(int documentId);

        bool SavePersonDocument(PersonOuterDocument document);

        IEnumerable<InsuranceCompany> GetInsuranceCompanies(string filter);

        Person GetPersonById(int id);
    }
}
