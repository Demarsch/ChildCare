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

        public Person GetPersonInfoes(int id)
        {
            return data.First<Person>(x => x.Id == id, x => x.PersonRelatives, x => x.InsuranceDocuments, x => x.PersonNames, x => x.Gender);
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
    }
}
