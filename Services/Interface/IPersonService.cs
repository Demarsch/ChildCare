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
        Person PersonById(int Id);

        Person GetPersonInfoes(int id, DateTime toDate);

        string SetPersonInfoes(Person person, List<PersonName> personNames);

        ICollection<PersonRelative> GetPersonRelatives(int personId);

        ICollection<InsuranceDocument> GetInsuranceDocuments(int personId);

        ICollection<InsuranceDocumentType> GetInsuranceDocumentTypes();

        ICollection<ChangeNameReason> GetChangeNameReasons();

        ICollection<Gender> GetGenders();
    }
}
