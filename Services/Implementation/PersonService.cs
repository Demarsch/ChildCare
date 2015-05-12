﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Core
{
    public class PersonService : IPersonService
    {
        private IDataContextProvider provider;

        public PersonService(IDataContextProvider Provider)
        {
            provider = Provider;
        }

        public Person GetPersonById(int Id)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetById<Person>(Id);
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
                        RelativeRelationName = x.RelativeRelationship.Name,
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
                return db.GetAll<InsuranceDocumentType>();
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
                return db.GetAll<Gender>();
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
                var list = db.GetData<InsuranceCompany>().Where(x => filter != string.Empty ? words.All(y => x.NameSMOK.Contains(y) || x.AddressF.Contains(y)) : true).ToArray();
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
                return db.GetAll<AddressType>();
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
                return db.GetAll<IdentityDocumentType>();
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
                    resStr += String.Format("тип док-та: {0}\r\nстрах. орг.: {1}\r\nсерия {2} номер {3}\r\nпериод действия {4}-{5}",
                        insuranceDocument.InsuranceDocumentType.Name, insuranceDocument.InsuranceCompany.NameSMOK, insuranceDocument.Series, insuranceDocument.Number, insuranceDocument.BeginDate.ToString("dd.MM.yyyy"),
                        insuranceDocument.EndDate.ToString("dd.MM.yyyy"));
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
    }
}
