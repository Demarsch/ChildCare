using System.Collections.Generic;
using DataLib;

namespace Core
{
    public interface IPatientService
    {
        IList<Person> GetPatients(string searchString, int topCount = 0);

        EntityContext<Person> GetPersonById(int id);
    }
}