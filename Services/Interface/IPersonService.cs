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

        string GetActualPersonSocialStatusesString(int personId);

        //bool SavePersonData(Person person, IList<PersonName> personNames, IList<InsuranceDocument> personInsuranceDocuments, IList<PersonAddress> PersonAddresses, IList<PersonIdentityDocument> personIdentityDocuments,
        //    IList<PersonDisability> personDisabilities, IList<PersonSocialStatus> personSocialStatuses, int healthGroupId, int nationalityId);

        bool SavePersonData(PersonDataSaveDTO person, ICollection<PersonDataSaveDTO> personRelatives);

        string GetOrCreateAmbCard(int personId);

        string GetAmbCardFirstList(int personId);

        string GetPersonHospList(int personId);

        string GetRadiationList(int personId);

        ICollection<PersonRelativeDTO> GetPersonRelativesDTO(int personId);

        PersonRelative GetPersonRelative(int personId, int relativePersonId);

        ICollection<RelativeRelationship> GetRelativeRelationships();

        ICollection<InsuranceDocument> GetInsuranceDocuments(int personId);

        ICollection<PersonIdentityDocument> GetPersonIdentityDocuments(int personId);

        ICollection<PersonOuterDocument> GetPersonOuterDocuments(int personId);

        ICollection<PersonSocialStatus> GetPersonSocialStatuses(int personId);

        ICollection<PersonDisability> GetPersonDisabilities(int personId);

        ICollection<PersonAddress> GetPersonAddresses(int personId);

        ICollection<InsuranceDocumentType> GetInsuranceDocumentTypes();        

        InsuranceDocumentType GetInsuranceDocumentTypeById(int id);

        ICollection<InsuranceCompany> GetInsuranceCompanies(string filter);

        InsuranceCompany GetInsuranceCompany(int id);

        ICollection<ChangeNameReason> GetActualChangeNameReasons();

        ICollection<Gender> GetGenders();

        ICollection<HealthGroup> GetHealthGroups();

        ICollection<Country> GetCountries();

        ICollection<MaritalStatus> GetMaritalStatuses();

        ICollection<Education> GetEducations();

        ICollection<Person> GetPersonsByFullName(string fullName);

        ICollection<AddressType> GetAddressTypes();

        ICollection<DisabilityType> GetDisabilityTypes();

        DisabilityType GetDisabilityType(int id);

        AddressType GetAddressType(int id);

        ICollection<IdentityDocumentType> GetIdentityDocumentTypes();

        IdentityDocumentType GetIdentityDocumentType(int id);

        ICollection<string> GetDisabilitiesGivenOrgByName(string name);

        ICollection<string> GetIdentityDocumentsGivenOrgByName(string name);

        SocialStatusType GetSocialStatusType(int id);

        ICollection<SocialStatusType> GetSocialStatusTypes();

        ICollection<Org> GetSocialStatusOrgByName(string name);

        int GetHealthGroupId(int personId, DateTime date);

        int GetNationalityCountryId(int personId, DateTime date);

        int GetMaritalStatusId(int personId, DateTime date);

        int GetEducationId(int personId, DateTime date);

        int GetDefaultNationalityCountryId();

        //ToDo: Move to other service
        Okato GetOKATOByCode(string codeOKATO);

        ICollection<Okato> GetOKATOByName(string okatoName, string okatoRegion);

        ICollection<Okato> GetOKATORegion(string regionName);

        Org GetOrg(int id);

        ////////

        PersonTalon GetPersonTalonById(int id);

        ICollection<Staff> GetAllStaffs();

        ICollection<Person> GetPersonsByStaffId(int staffId, DateTime begin, DateTime end);

        PersonStaff GetPersonStaff(int personId, int staffId, DateTime begin, DateTime end);

        bool SavePersonDocument(PersonOuterDocument personOuterDocument, out string exception);

        void DeletePersonOuterDocument(int documentId);
    }
}
