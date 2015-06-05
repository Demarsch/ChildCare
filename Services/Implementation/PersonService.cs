using System;
using System.Collections.Generic;
using System.Linq;
using DataLib;
using System.Data.Entity.Core.Objects;

namespace Core
{
    public class PersonService : IPersonService
    {
        private readonly IDataContextProvider provider;

        public PersonService(IDataContextProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            this.provider = provider;
        }

        public Person GetPersonById(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<Person>(id);
            }
        }

        public ICollection<PersonRelativeDTO> GetPersonRelativesDTO(int personId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonRelative>().Where(x => x.PersonId == personId).Select(x => new PersonRelativeDTO
                    {
                        RelativePersonId = x.RelativeId,
                        ShortName = x.Person1.ShortName,
                        RelativeRelationId = x.RelativeRelationshipId,
                        IsRepresentative = x.IsRepresentative,
                        PhotoUri = string.Empty
                    }).ToList();
            }
        }

        public ICollection<InsuranceDocument> GetInsuranceDocuments(int personId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<InsuranceDocument>().Where(x => x.PersonId == personId).ToArray();
            }
        }

        public ICollection<InsuranceDocumentType> GetInsuranceDocumentTypes()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<InsuranceDocumentType>().ToArray();
            }
        }

        public ICollection<ChangeNameReason> GetActualChangeNameReasons()
        {
            using (var db = provider.GetNewDataContext())
            {
                var DateTimeNow = DateTime.Now;
                return db.GetData<ChangeNameReason>().Where(x => DateTimeNow >= x.BeginDateTime && DateTimeNow < x.EndDateTime).ToArray();
            }
        }

        public ICollection<Gender> GetGenders()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Gender>().ToArray();
            }
        }

        public ICollection<Person> GetPersonsByFullName(string fullName)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Person>().Where(x => x.FullName.ToLower().Trim().Contains(fullName.ToLower().Trim())).ToArray();
            }
        }

        public ICollection<InsuranceCompany> GetInsuranceCompanies(string filter)
        {
            using (var db = provider.GetNewDataContext())
            {
                var words = filter.Split(' ').ToList();
                words.Remove("");
                var list = db.GetData<InsuranceCompany>().Where(x => filter == string.Empty || words.All(y => x.NameSMOK.Contains(y) || x.AddressF.Contains(y))).ToArray();
                return list;
            }
        }

        public InsuranceDocumentType GetInsuranceDocumentTypeById(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<InsuranceDocumentType>(id);
            }
        }


        public ICollection<PersonAddress> GetPersonAddresses(int personId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonAddress>().Where(x => x.PersonId == personId).ToArray();
            }
        }


        public Okato GetOKATOByCode(string codeOKATO)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Okato>().FirstOrDefault(x => x.CodeOKATO == codeOKATO);
            }
        }

        public ICollection<Okato> GetOKATOByName(string okatoName, string okatoRegion = "")
        {
            using (var db = provider.GetNewDataContext())
            {
                var okatoRegionTwoDigits = string.Empty;
                if (okatoRegion != string.Empty)
                    okatoRegionTwoDigits = okatoRegion.Substring(0, 2);
                return db.GetData<Okato>().Where(x => x.FullName.Contains(okatoName) &&
                    (okatoRegionTwoDigits != string.Empty ? (x.CodeOKATO.StartsWith(okatoRegionTwoDigits)) : true)).ToArray();
            }
        }

        public ICollection<Okato> GetOKATORegion(string regionName)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Okato>().Where(x => x.FullName.Contains(regionName) && (x.CodeOKATO.Substring(2, 9) == "000000000" || x.CodeOKATO.Substring(0, 1) == "c")).ToArray();
            }
        }

        public ICollection<AddressType> GetAddressTypes()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<AddressType>().ToArray();
            }
        }

        public AddressType GetAddressType(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<AddressType>(id);
            }
        }

        public ICollection<PersonIdentityDocument> GetPersonIdentityDocuments(int personId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonIdentityDocument>().Where(x => x.PersonId == personId).ToArray();
            }
        }

        public ICollection<IdentityDocumentType> GetIdentityDocumentTypes()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<IdentityDocumentType>().ToArray();
            }
        }

        public IdentityDocumentType GetIdentityDocumentType(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<IdentityDocumentType>(id);
            }
        }

        public ICollection<string> GetIdentityDocumentsGivenOrgByName(string name)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonIdentityDocument>().Where(x => x.GivenOrg.Contains(name)).Select(x => x.GivenOrg).Distinct().ToArray();
            }
        }

        public ICollection<string> GetDisabilitiesGivenOrgByName(string name)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonDisability>().Where(x => x.GivenOrg.Contains(name)).Select(x => x.GivenOrg).Distinct().ToArray();
            }
        }

        public PersonName GetActualPersonName(int personId)
        {
            using (var db = provider.GetNewDataContext())
            {
                var dateTimeNow = DateTime.Now;
                return db.GetData<PersonName>().FirstOrDefault(y => dateTimeNow >= y.BeginDateTime && dateTimeNow < y.EndDateTime && !y.ChangeNameReasonId.HasValue);
            }
        }

        public string GetActualInsuranceDocumentsString(int personId)
        {
            var resStr = string.Empty;
            using (var db = provider.GetNewDataContext())
            {
                var dateTimeNow = DateTime.Now;
                var actualInsuranceDocuments = db.GetData<InsuranceDocument>().Where(x => personId == x.PersonId && dateTimeNow >= x.BeginDate && dateTimeNow < x.EndDate);

                foreach (var insuranceDocument in actualInsuranceDocuments)
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    resStr += String.Format("тип док-та: {0}\r\nстрах. орг.: {1}\r\nсерия {2} номер {3}\r\nдействует с {4} {5}",
                        insuranceDocument.InsuranceDocumentType.Name, insuranceDocument.InsuranceCompany.NameSMOK, insuranceDocument.Series, insuranceDocument.Number, insuranceDocument.BeginDate.ToString("dd.MM.yyyy"),
                        (insuranceDocument.EndDate != DateTime.MaxValue ? " по " + insuranceDocument.EndDate.ToString("dd.MM.yyyy") : string.Empty));
                }
            }
            return resStr;
        }


        public InsuranceCompany GetInsuranceCompany(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<InsuranceCompany>().FirstOrDefault(x => x.Id == id);
            }
        }


        public ICollection<PersonDisability> GetPersonDisabilities(int personId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonDisability>().Where(x => personId == x.PersonId).ToArray();
            }
        }

        public ICollection<DisabilityType> GetDisabilityTypes()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<DisabilityType>().ToArray();
            }
        }

        public DisabilityType GetDisabilityType(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<DisabilityType>().Where(x => x.Id == id).FirstOrDefault();
            }
        }

        public string GetActualPersonAddressesString(int personId)
        {
            var resStr = string.Empty;
            using (var db = provider.GetNewDataContext())
            {
                var dateTimeNow = DateTime.Now;
                var actualPersonAddresses = db.GetData<PersonAddress>().Where(x => personId == x.PersonId && dateTimeNow >= x.BeginDateTime && dateTimeNow < x.EndDateTime);

                foreach (var personAddress in actualPersonAddresses)
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    resStr += personAddress.AddressType.Name + ": " + personAddress.UserText + " " + personAddress.House + (personAddress.Building != string.Empty ? "\"" + personAddress.Building + "\"" : string.Empty) +
                        (personAddress.Apartment != string.Empty ? " " + personAddress.Apartment : string.Empty) + "\r\nДействует с " + personAddress.BeginDateTime.ToString("dd.MM.yyyy") + (personAddress.EndDateTime != DateTime.MaxValue ? " по" + personAddress.EndDateTime.ToString("dd.MM.yyyy") : string.Empty);
                }
            }
            return resStr;
        }


        public string GetActualPersonIdentityDocumentsString(int personId)
        {
            var resStr = string.Empty;
            using (var db = provider.GetNewDataContext())
            {
                var dateTimeNow = DateTime.Now;
                var actualPersonIdentityDocuments = db.GetData<PersonIdentityDocument>().Where(x => personId == x.PersonId && dateTimeNow >= x.BeginDate && dateTimeNow < x.EndDate);

                foreach (var personIdentityDocument in actualPersonIdentityDocuments)
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    resStr += personIdentityDocument.IdentityDocumentType.Name + ": Серия " + personIdentityDocument.Series + " Номер " + personIdentityDocument.Number + "\r\nВыдан " + personIdentityDocument.GivenOrg + " " + personIdentityDocument.BeginDate.ToString("dd.MM.yyyy") + (personIdentityDocument.EndDate != DateTime.MaxValue ? " по " + personIdentityDocument.EndDate.ToString("dd.MM.yyyy") : string.Empty);
                }
            }
            return resStr;
        }


        public string GetActualPersonDisabilitiesString(int personId)
        {
            var resStr = string.Empty;
            using (var db = provider.GetNewDataContext())
            {
                var dateTimeNow = DateTime.Now;
                var actualPersonPersonDisabilities = db.GetData<PersonDisability>().Where(x => personId == x.PersonId && dateTimeNow >= x.BeginDate && dateTimeNow < x.EndDate);

                foreach (var personDisabilities in actualPersonPersonDisabilities)
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    resStr += personDisabilities.DisabilityType.Name + ": Серия " + personDisabilities.Series + " Номер " + personDisabilities.Number + "\r\nВыдан " + personDisabilities.GivenOrg + " " + personDisabilities.BeginDate.ToString("dd.MM.yyyy") + (personDisabilities.EndDate != DateTime.MaxValue ? " по " + personDisabilities.EndDate.ToString("dd.MM.yyyy") : string.Empty);
                }
            }
            return resStr;
        }

        public PersonTalon GetPersonTalonById(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonTalon>().FirstOrDefault(x => x.Id == id);
            }
        }


        public ICollection<RelativeRelationship> GetRelativeRelationships()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<RelativeRelationship>().ToArray();
            }
        }

        public string GetActualPersonSocialStatusesString(int personId)
        {
            var resStr = string.Empty;
            using (var db = provider.GetNewDataContext())
            {
                var dateTimeNow = DateTime.Now;
                var actualPersonSocialStatuses = db.GetData<PersonSocialStatus>().Where(x => personId == x.PersonId && dateTimeNow >= x.BeginDateTime && dateTimeNow < x.EndDateTime);

                foreach (var personSocialStatus in actualPersonSocialStatuses)
                {
                    if (resStr != string.Empty)
                        resStr += "\r\n";
                    resStr += personSocialStatus.SocialStatusType.Name + (personSocialStatus.Org != null ? ": " + personSocialStatus.Org.Name + ", " + personSocialStatus.Office : string.Empty);
                }
            }
            return resStr;
        }

        public SocialStatusType GetSocialStatusType(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<SocialStatusType>().FirstOrDefault(x => x.Id == id);
            }
        }


        public ICollection<SocialStatusType> GetSocialStatusTypes()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<SocialStatusType>().ToArray();
            }
        }

        public ICollection<Org> GetSocialStatusOrgByName(string name)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Org>().Where(x => x.Name.Contains(name)).ToArray();
            }
        }

        public Org GetOrg(int id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Org>().FirstOrDefault(x => x.Id == id);
            }
        }


        public ICollection<PersonSocialStatus> GetPersonSocialStatuses(int personId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonSocialStatus>().Where(x => x.PersonId == personId).ToArray();
            }
        }


        public ICollection<HealthGroup> GetHealthGroups()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<HealthGroup>().ToArray();
            }
        }

        public ICollection<Country> GetCountries()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Country>().ToArray();
            }
        }


        public int GetHealthGroupId(int personId, DateTime date)
        {
            using (var db = provider.GetNewDataContext())
            {
                var personHealthGroup = db.GetData<PersonHealthGroup>().FirstOrDefault(x => x.PersonId == personId && date > x.BeginDateTime && date < x.EndDateTime);
                if (personHealthGroup == null) return 0;
                return personHealthGroup.HealthGroupId;
            }
        }

        public int GetNationalityCountryId(int personId, DateTime date)
        {
            using (var db = provider.GetNewDataContext())
            {
                var personNationality = db.GetData<PersonNationality>().FirstOrDefault(x => x.PersonId == personId && date > x.BeginDateTime && date < x.EndDateTime);
                if (personNationality == null) return 0;
                return personNationality.CountryId;
            }
        }

        public ICollection<Staff> GetAllStaffs()
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<Staff>().OrderBy(x => x.Name).ToArray();
            }
        }

        public ICollection<Person> GetPersonsByStaffId(int staffId, DateTime begin, DateTime end)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonStaff>().Where(x => x.StaffId == staffId && EntityFunctions.TruncateTime(x.BeginDateTime) <= EntityFunctions.TruncateTime(end) && EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(begin)).Select(x => x.Person).OrderBy(x => x.ShortName).ToArray();
            }
        }

        public PersonStaff GetPersonStaff(int personId, int staffId, DateTime begin, DateTime end)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonStaff>().FirstOrDefault(x => x.PersonId == personId && x.StaffId == staffId && EntityFunctions.TruncateTime(x.BeginDateTime) <= EntityFunctions.TruncateTime(end) && EntityFunctions.TruncateTime(x.EndDateTime) >= EntityFunctions.TruncateTime(begin));
            }
        }


        public int GetDefaultNationalityCountryId()
        {
            using (var db = provider.GetNewDataContext())
            {
                var defaultNationality = db.GetData<Country>().FirstOrDefault(x => x.IsDefaultNationality);
                if (defaultNationality == null) return 0;
                return defaultNationality.Id;
            }
        }


        public bool SavePersonData(Person person, IList<PersonName> personNames, IList<InsuranceDocument> personInsuranceDocuments, IList<PersonAddress> personAddresses, IList<PersonIdentityDocument> personIdentityDocuments,
            IList<PersonDisability> personDisabilities, IList<PersonSocialStatus> personSocialStatuses, int healthGroupId, int nationalityId)
        {
            using (var db = provider.GetNewDataContext())
            {
                if (person.Id == 0)
                    db.Add(person);
                else
                    db.Update(person);
                SetPersonNames(person, personNames, db);
                if (personInsuranceDocuments != null)
                    SetPersonInsuranceDocuments(person, personInsuranceDocuments, db);
                if (personAddresses != null)
                    SetPersonAddresses(person, personAddresses, db);
                if (personIdentityDocuments != null)
                    SetPersonIdentityDocuments(person, personIdentityDocuments, db);
                if (personDisabilities != null)
                    SetPersonDisabilities(person, personDisabilities, db);
                if (personSocialStatuses != null)
                    SetPersonSocialStatuses(person, personSocialStatuses, db);
                SetPersonHealthGroup(person, healthGroupId, db);
                SetPersonNationality(person, nationalityId, db);
                try
                {
                    db.Save();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        private void SetPersonNationality(Person person, int nationalityId, IDataContext db)
        {
            if (nationalityId < 1)
                return;
            PersonNationality personNationality = person.PersonNationalities.FirstOrDefault(x => x.EndDateTime == DateTime.MaxValue);
            DateTime dateTimeNow = DateTime.Now;
            if (personNationality != null)
                personNationality.EndDateTime = dateTimeNow;
            var newPersonNationality = new PersonNationality()
            {
                Person = person,
                CountryId = nationalityId,
                BeginDateTime = dateTimeNow,
                EndDateTime = DateTime.MaxValue
            };
        }

        private void SetPersonHealthGroup(Person person, int healthGroupId, IDataContext db)
        {
            if (healthGroupId < 1)
                return;
            PersonHealthGroup personHealthGroup = person.PersonHealthGroups.FirstOrDefault(x => x.EndDateTime == DateTime.MaxValue);
            DateTime dateTimeNow = DateTime.Now;
            if (personHealthGroup != null)
                personHealthGroup.EndDateTime = dateTimeNow;
            var newPersonHealthGroup = new PersonHealthGroup()
            {
                Person = person,
                HealthGroupId = healthGroupId,
                BeginDateTime = dateTimeNow,
                EndDateTime = DateTime.MaxValue
            };
        }

        private void SetPersonSocialStatuses(Person person, IList<PersonSocialStatus> changedPersonSocialStatuses, IDataContext db)
        {
            PersonSocialStatus changedPersonSocialStatus;
            PersonSocialStatus personSocialStatus;
            var personSocialStatusesList = person.PersonSocialStatuses.ToList();
            int personSocialStatusesCount = personSocialStatusesList.Count;
            for (int i = 0; i < personSocialStatusesCount; i++)
            {
                personSocialStatus = personSocialStatusesList[i];
                if (changedPersonSocialStatuses.Count > i)
                {
                    changedPersonSocialStatus = changedPersonSocialStatuses[i];
                    personSocialStatus.SocialStatusTypeId = changedPersonSocialStatus.SocialStatusTypeId;
                    personSocialStatus.Office = changedPersonSocialStatus.Office;
                    personSocialStatus.OrgId = changedPersonSocialStatus.OrgId;
                    personSocialStatus.BeginDateTime = changedPersonSocialStatus.BeginDateTime;
                    personSocialStatus.EndDateTime = changedPersonSocialStatus.EndDateTime;
                }
                else
                {
                    db.Remove(personSocialStatus);
                }
            }
            if (changedPersonSocialStatuses.Count > personSocialStatusesCount)
            {
                for (int j = personSocialStatusesCount; j < changedPersonSocialStatuses.Count; j++)
                {
                    changedPersonSocialStatuses[j].Person = person;
                    db.Add(changedPersonSocialStatuses[j]);
                }
            }
        }

        private void SetPersonDisabilities(Person person, IList<PersonDisability> changedPersonDisabilities, IDataContext db)
        {
            PersonDisability changedPersonDisability;
            PersonDisability personDisability;
            var personDisabilitiesList = person.PersonDisabilities.ToList();
            int personDisabilitiesCount = personDisabilitiesList.Count;
            for (int i = 0; i < personDisabilitiesCount; i++)
            {
                personDisability = personDisabilitiesList[i];
                if (changedPersonDisabilities.Count > i)
                {
                    changedPersonDisability = changedPersonDisabilities[i];
                    personDisability.DisabilityTypeId = changedPersonDisability.DisabilityTypeId;
                    personDisability.Series = changedPersonDisability.Series;
                    personDisability.Number = changedPersonDisability.Number;
                    personDisability.GivenOrg = changedPersonDisability.GivenOrg;
                    personDisability.BeginDate = changedPersonDisability.BeginDate;
                    personDisability.EndDate = changedPersonDisability.EndDate;
                }
                else
                {
                    db.Remove(personDisability);
                }
            }
            if (changedPersonDisabilities.Count > personDisabilitiesCount)
            {
                for (int j = personDisabilitiesCount; j < changedPersonDisabilities.Count; j++)
                {
                    changedPersonDisabilities[j].Person = person;
                    db.Add(changedPersonDisabilities[j]);
                }
            }
        }

        private void SetPersonIdentityDocuments(Person person, IList<PersonIdentityDocument> changedPersonIdentityDocuments, IDataContext db)
        {
            PersonIdentityDocument changedPersonIdentityDocument;
            PersonIdentityDocument personIdentityDocument;
            var personIdentityDocumentsList = person.PersonIdentityDocuments.ToList();
            int personIdentityDocumentsCount = personIdentityDocumentsList.Count;
            for (int i = 0; i < personIdentityDocumentsCount; i++)
            {
                personIdentityDocument = personIdentityDocumentsList[i];
                if (changedPersonIdentityDocuments.Count > i)
                {
                    changedPersonIdentityDocument = changedPersonIdentityDocuments[i];
                    personIdentityDocument.IdentityDocumentTypeId = changedPersonIdentityDocument.IdentityDocumentTypeId;
                    personIdentityDocument.Series = changedPersonIdentityDocument.Series;
                    personIdentityDocument.Number = changedPersonIdentityDocument.Number;
                    personIdentityDocument.GivenOrg = changedPersonIdentityDocument.GivenOrg;
                    personIdentityDocument.BeginDate = changedPersonIdentityDocument.BeginDate;
                    personIdentityDocument.EndDate = changedPersonIdentityDocument.EndDate;
                }
                else
                {
                    db.Remove(personIdentityDocument);
                }
            }
            if (changedPersonIdentityDocuments.Count > personIdentityDocumentsCount)
            {
                for (int j = personIdentityDocumentsCount; j < changedPersonIdentityDocuments.Count; j++)
                {
                    changedPersonIdentityDocuments[j].Person = person;
                    db.Add(changedPersonIdentityDocuments[j]);
                }
            }
        }

        private void SetPersonAddresses(Person person, IList<PersonAddress> changedPersonAddresses, IDataContext db)
        {
            PersonAddress changedPersonAddress;
            PersonAddress personAddress;
            var personAddressesList = person.PersonAddresses.ToList();
            int personAddressesCount = personAddressesList.Count;
            for (int i = 0; i < personAddressesCount; i++)
            {
                personAddress = personAddressesList[i];
                if (changedPersonAddresses.Count > i)
                {
                    changedPersonAddress = changedPersonAddresses[i];
                    personAddress.AddressTypeId = changedPersonAddress.AddressTypeId;
                    personAddress.OkatoText = changedPersonAddress.OkatoText;
                    personAddress.UserText = changedPersonAddress.UserText;
                    personAddress.House = changedPersonAddress.House;
                    personAddress.Building = changedPersonAddress.Building;
                    personAddress.Apartment = changedPersonAddress.Apartment;
                    personAddress.BeginDateTime = changedPersonAddress.BeginDateTime;
                    personAddress.EndDateTime = changedPersonAddress.EndDateTime;
                }
                else
                {
                    db.Remove(personAddress);
                }
            }
            if (changedPersonAddresses.Count > personAddressesCount)
            {
                for (int j = personAddressesCount; j < changedPersonAddresses.Count; j++)
                {
                    changedPersonAddresses[j].Person = person;
                    db.Add(changedPersonAddresses[j]);
                }
            }
        }

        private void SetPersonNames(Person person, IList<PersonName> changedPersonNames, IDataContext db)
        {
            PersonName changedPersonName;
            PersonName personName;
            var personNamesList = person.PersonNames.ToList();
            int personInsuranceDocumentsCount = personNamesList.Count;
            for (int i = 0; i < personInsuranceDocumentsCount; i++)
            {
                personName = personNamesList[i];
                if (changedPersonNames.Count > i)
                {
                    changedPersonName = changedPersonNames[i];
                    personName.LastName = changedPersonName.LastName;
                    personName.FirstName = changedPersonName.FirstName;
                    personName.MiddleName = changedPersonName.MiddleName;
                    personName.ChangeNameReasonId = changedPersonName.ChangeNameReasonId;
                    personName.BeginDateTime = changedPersonName.BeginDateTime;
                    personName.EndDateTime = changedPersonName.EndDateTime;
                }
                else
                {
                    db.Remove(personName);
                }
            }
            if (changedPersonNames.Count > personInsuranceDocumentsCount)
            {
                for (int j = personInsuranceDocumentsCount; j < changedPersonNames.Count; j++)
                {
                    changedPersonNames[j].Person = person;
                    db.Add(changedPersonNames[j]);
                }
            }
        }

        private void SetPersonInsuranceDocuments(Person person, IList<InsuranceDocument> changedPersonInsuranceDocuments, IDataContext db)
        {
            InsuranceDocument changedPersonInsuranceDocument;
            InsuranceDocument personInsuranceDocument;
            var personInsuranceDocumentsList = person.InsuranceDocuments.ToList();
            int personInsuranceDocumentsCount = personInsuranceDocumentsList.Count;
            for (int i = 0; i < personInsuranceDocumentsCount; i++)
            {
                personInsuranceDocument = personInsuranceDocumentsList[i];
                if (changedPersonInsuranceDocuments.Count > i)
                {
                    changedPersonInsuranceDocument = changedPersonInsuranceDocuments[i];
                    personInsuranceDocument.InsuranceCompanyId = changedPersonInsuranceDocument.InsuranceCompanyId;
                    personInsuranceDocument.InsuranceDocumentTypeId = changedPersonInsuranceDocument.InsuranceDocumentTypeId;
                    personInsuranceDocument.Series = changedPersonInsuranceDocument.Series;
                    personInsuranceDocument.Number = changedPersonInsuranceDocument.Number;
                    personInsuranceDocument.BeginDate = changedPersonInsuranceDocument.BeginDate;
                    personInsuranceDocument.EndDate = changedPersonInsuranceDocument.EndDate;
                }
                else
                {
                    db.Remove(personInsuranceDocument);
                }
            }
            if (changedPersonInsuranceDocuments.Count > personInsuranceDocumentsCount)
            {
                for (int j = personInsuranceDocumentsCount; j < changedPersonInsuranceDocuments.Count; j++)
                {
                    changedPersonInsuranceDocuments[j].Person = person;
                    db.Add(changedPersonInsuranceDocuments[j]);
                }
            }
        }
    }
}
