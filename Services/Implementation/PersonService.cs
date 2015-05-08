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

        public ICollection<PersonRelative> GetPersonRelatives(int personId)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonRelative>().Where(x => x.PersonId == personId).ToArray();
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
                return db.GetData<Person>().Where(x => x.FullName.ToLower().Trim() == fullName.ToLower().Trim()).ToArray();
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
        
        public ICollection<string> GetGivenOrgByName(string name)
        {
            using (var db = provider.GetNewDataContext())
            {
                return db.GetData<PersonIdentityDocument>().Where(x => x.GivenOrg.Contains(name)).Select(x => x.GivenOrg).Distinct().ToArray();
            }
        }
    }
}
