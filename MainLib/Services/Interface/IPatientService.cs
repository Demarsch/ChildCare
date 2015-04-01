using System.Collections.Generic;
using DataLib;
using System;

namespace Core
{
    public interface IPatientService
    {
        ICollection<Person> GetPatients(string searchString, int topCount = 0);

        EntityContext<Person> GetPersonById(int id);

        string SavePersonName(int personId, string firstName, string lastName, string middleName, int changeNameReasonId, DateTime fromDateTime);

        ICollection<InsuranceDocument> GetPersonInsuranceDocuments(int personId);

        ICollection<PersonRelativeDTO> GetPersonRelatives(int personId);
    }
}