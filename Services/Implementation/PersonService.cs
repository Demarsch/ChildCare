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

        public string SetPersonInfoes(Person person, List<PersonName> personNames)
        {
            try
            {
                data.Save<Person>(person);
                foreach (var personName in personNames)
                    personName.PersonId = person.Id;
                data.Save<PersonName>(personNames.ToArray());
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
            return data.Get<ChangeNameReason>().Where(x => DateTimeNow >= x.BeginDateTime && DateTimeNow < x.EndDateTime).ToList();
        }


        public ICollection<Gender> GetGenders()
        {
            return data.Get<Gender>();
        }

        public ICollection<Person> GetPersonsByFullName(string fullName)
        {
            return data.Get<Person>().Where(x => x.FullName.ToLower().Trim() == fullName.ToLower().Trim()).ToList();
        }


        public ICollection<InsuranceCompany> GetInsuranceCompanies(string filter)
        {
            //ToDo: More complex search with splitting filter on words
            return data.Get<InsuranceCompany>().Where(x => filter != string.Empty ? x.NameSMOK.Contains(filter) : true).ToList();
        }
    }
}
