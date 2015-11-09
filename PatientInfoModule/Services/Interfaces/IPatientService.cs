﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Misc;
using System;
using PatientInfoModule.Data;

namespace PatientInfoModule.Services
{
    public interface IPatientService
    {
        IDisposableQueryable<Person> GetPatientQuery(int patientId);

        IQueryable<Country> GetCountries();

        IQueryable<Education> GetEducations();

        IQueryable<MaritalStatus> GetMaritalStatuses();

        IQueryable<HealthGroup> GetHealthGroups();
            
        Task<SavePatientOutput> SavePatientAsync(SavePatientInput data, CancellationToken token);

        IDisposableQueryable<Person> GetPersonsByFullName(string fullname);

        IDisposableQueryable<PersonStaff> GetPersonStaff(int personId, int staffId, DateTime begin, DateTime end);

        IDisposableQueryable<PersonStaff> GetAllowedPersonStaffs(int recordTypeId, int memberRoleId);

        IDisposableQueryable<PersonOuterDocument> GetPersonOuterDocuments(int personId);

        void DeletePersonOuterDocument(int documentId);

        bool SavePersonDocument(PersonOuterDocument document);
    }
}
