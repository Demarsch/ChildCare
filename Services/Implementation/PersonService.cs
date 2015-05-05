using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Core
{
    public class PersonService : IPersonService
    {
        private IDataAccessLayer data;

        public PersonService(ISimpleLocator serviceLocator)
        {
            data = serviceLocator.Instance<IDataAccessLayer>();
        }

        public Person PersonById(int Id)
        {
            return data.Cache<Person>(Id);
        }

        public Person GetPersonInfoes(int id, DateTime toDate)
        {
            return data.First<Person>(x => x.Id == id, x => x.PersonRelatives, x => x.InsuranceDocuments, x => x.InsuranceDocuments.Select(y => y.InsuranceCompany), x => x.InsuranceDocuments.Select(y => y.InsuranceDocumentType),
                x => x.PersonNames, x => x.Gender);
        }

        public string SetPersonInfoes(Person person, List<PersonName> personNames, List<InsuranceDocument> insuranceDocuments)
        {
            try
            {
                data.Save<Person>(person);
                foreach (var personName in personNames)
                    personName.PersonId = person.Id;
                data.Save<PersonName>(personNames.ToArray());
                foreach (var insuranceDocument in insuranceDocuments)
                    insuranceDocument.PersonId = person.Id;
                data.Save<InsuranceDocument>(insuranceDocuments.ToArray());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }

        public ICollection<PersonRelative> GetPersonRelatives(int personId)
        {
            return data.Get<PersonRelative>(x => x.PersonId == personId, x => x.RelativeRelationship, x => x.Person1);
            //.Select(x => new PersonRelativeDTO()
            //{
            //    RelativePersonId = x.RelativeId,
            //    ShortName = x.Person1.ShortName,
            //    RelativeRelationName = x.RelativeRelationship.Name,
            //    IsRepresentative = x.IsRepresentative
            //}).ToList());
        }

        public ICollection<InsuranceDocument> GetInsuranceDocuments(int personId)
        {
            return data.Get<InsuranceDocument>(x => x.PersonId == personId, x => x.InsuranceCompany, x => x.InsuranceDocumentType);
        }

        public ICollection<InsuranceDocumentType> GetInsuranceDocumentTypes()
        {
            return data.Get<InsuranceDocumentType>();
        }


        public ICollection<ChangeNameReason> GetChangeNameReasons()
        {
            var DateTimeNow = DateTime.Now;
            return data.Get<ChangeNameReason>(x => DateTimeNow >= x.BeginDateTime && DateTimeNow < x.EndDateTime).ToList();
        }


        public ICollection<Gender> GetGenders()
        {
            return data.Get<Gender>();
        }

        public ICollection<Person> GetPersonsByFullName(string fullName)
        {
            return data.Get<Person>(x => x.FullName.ToLower().Trim().Contains(fullName.ToLower().Trim())).ToList();
        }


        public ICollection<InsuranceCompany> GetInsuranceCompanies(string filter)
        {
            var words = filter.Split(' ').ToList();
            words.Remove("");
            var list = data.Get<InsuranceCompany>(x => filter != string.Empty ? words.All(y => x.NameSMOK.Contains(y) || x.AddressF.Contains(y)) : true).ToList();
            return list;
        }

        public InsuranceDocumentType GetInsuranceDocumentTypeById(int id)
        {
            return data.Get<InsuranceDocumentType>(x => x.Id == id).FirstOrDefault();
        }


        public ICollection<PersonAddress> GetPersonAddresses(int personId)
        {
            return data.Get<PersonAddress>(x => x.PersonId == personId, x => x.AddressType);
        }


        public Okato GetOKATOByCode(string codeOKATO)
        {
            return data.Get<Okato>(x => x.CodeOKATO == codeOKATO).FirstOrDefault();
        }

        public ICollection<Okato> GetOKATOByName(string okatoName, string okatoRegion = "")
        {
            var okatoRegionTwoDigits = string.Empty;
            if (okatoRegion != string.Empty)
                okatoRegionTwoDigits = okatoRegion.Substring(0, 2);
            return data.Get<Okato>(x => x.FullName.Contains(okatoName) &&
                (okatoRegionTwoDigits != string.Empty ? (x.CodeOKATO.StartsWith(okatoRegionTwoDigits)) : true));
        }

        public ICollection<Okato> GetOKATORegion(string regionName)
        {
            return data.Get<Okato>(x => x.FullName.Contains(regionName) && (x.CodeOKATO.Substring(2, 9) == "000000000" || x.CodeOKATO.Substring(0, 1) == "c"));
        }

        public ICollection<AddressType> GetAddressTypes()
        {
            return data.Get<AddressType>();
        }


        public AddressType GetAddressType(int id)
        {
            return data.Get<AddressType>(x => x.Id == id).FirstOrDefault();
        }
    }
}
