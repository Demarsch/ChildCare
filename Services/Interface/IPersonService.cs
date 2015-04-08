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

        Person GetPersonInfoes(int id);

        ICollection<PersonRelative> GetPersonRelatives(int personId);
    }
}
