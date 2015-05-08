using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Core
{
    public interface IPersonService
    {
        Person GetPersonById(int Id);

        ICollection<PersonRelative> GetPersonRelatives(int personId);

        ICollection<InsuranceDocument> GetInsuranceDocuments(int personId);

        ICollection<PersonIdentityDocument> GetPersonIdentityDocuments(int personId);

        ICollection<PersonAddress> GetPersonAddresses(int personId);

        ICollection<InsuranceDocumentType> GetInsuranceDocumentTypes();

        InsuranceDocumentType GetInsuranceDocumentTypeById(int id);

        ICollection<InsuranceCompany> GetInsuranceCompanies(string filter);

        ICollection<ChangeNameReason> GetActualChangeNameReasons();

        ICollection<Gender> GetGenders();

        ICollection<Person> GetPersonsByFullName(string fullName);

        ICollection<AddressType> GetAddressTypes();

        AddressType GetAddressType(int id);

        ICollection<IdentityDocumentType> GetIdentityDocumentTypes();

        IdentityDocumentType GetIdentityDocumentType(int id);

        ICollection<string> GetGivenOrgByName(string name);

        //ToDo: Move to other service
        Okato GetOKATOByCode(string codeOKATO);

        ICollection<Okato> GetOKATOByName(string okatoName, string okatoRegion);

        ICollection<Okato> GetOKATORegion(string regionName);
    }
}
