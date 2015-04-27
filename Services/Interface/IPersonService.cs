﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Core
{
    public interface IPersonService
    {
        Person PersonById(int Id);

        Person GetPersonInfoes(int id, DateTime toDate);

        string SetPersonInfoes(Person person, List<PersonName> personNames, List<InsuranceDocument> insuranceDocuments);

        ICollection<PersonRelative> GetPersonRelatives(int personId);

        ICollection<InsuranceDocument> GetInsuranceDocuments(int personId);

        ICollection<InsuranceDocumentType> GetInsuranceDocumentTypes();

        InsuranceDocumentType GetInsuranceDocumentTypeById(int id);

        ICollection<InsuranceCompany> GetInsuranceCompanies(string filter);

        ICollection<ChangeNameReason> GetChangeNameReasons();

        ICollection<Gender> GetGenders();

        ICollection<Person> GetPersonsByFullName(string fullName);
    }
}
