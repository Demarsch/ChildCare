using Core.Data;
using Core.Data.Misc;
using System;

namespace PatientInfoModule.Services
{
    public interface IPatientService
    {
        IDisposableQueryable<Person> GetPatientQuery(int patientId);

        IDisposableQueryable<Person> GetPersonsByFullName(string fullname);

        IDisposableQueryable<PersonStaff> GetPersonStaff(int personId, int staffId, DateTime begin, DateTime end);

        IDisposableQueryable<PersonStaff> GetAllowedPersonStaffs(int recordTypeId, int memberRoleId);
    }
}
