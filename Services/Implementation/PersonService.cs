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
    }
}
