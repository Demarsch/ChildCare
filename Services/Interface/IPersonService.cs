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

        PersonName GetActualPersonName(int personId);

        string GetActualInsuranceDocumentsString(int personId);

        string GetActualPersonAddressesString(int personId);

        string GetActualPersonIdentityDocumentsString(int personId);

        string GetActualPersonDisabilitiesString(int personId);

        ICollection<PersonRelativeDTO> GetPersonRelativesDTO(int personId);

        ICollection<InsuranceDocument> GetInsuranceDocuments(int personId);

        ICollection<PersonIdentityDocument> GetPersonIdentityDocuments(int personId);

        ICollection<PersonDisability> GetPersonDisabilities(int personId);

        ICollection<PersonAddress> GetPersonAddresses(int personId);

        ICollection<InsuranceDocumentType> GetInsuranceDocumentTypes();

        InsuranceDocumentType GetInsuranceDocumentTypeById(int id);

        ICollection<InsuranceCompany> GetInsuranceCompanies(string filter);

        InsuranceCompany GetInsuranceCompany(int id);

        ICollection<ChangeNameReason> GetActualChangeNameReasons();

        ICollection<Gender> GetGenders();

        ICollection<Person> GetPersonsByFullName(string fullName);

        ICollection<AddressType> GetAddressTypes();

        ICollection<DisabilityType> GetDisabilityTypes();

        DisabilityType GetDisabilityType(int id);

        AddressType GetAddressType(int id);

        ICollection<IdentityDocumentType> GetIdentityDocumentTypes();

        IdentityDocumentType GetIdentityDocumentType(int id);

        ICollection<string> GetDisabilitiesGivenOrgByName(string name);

        ICollection<string> GetIdentityDocumentsGivenOrgByName(string name);

        //ToDo: Move to other service
        Okato GetOKATOByCode(string codeOKATO);

        ICollection<Okato> GetOKATOByName(string okatoName, string okatoRegion);

        ICollection<Okato> GetOKATORegion(string regionName);
    }
}
