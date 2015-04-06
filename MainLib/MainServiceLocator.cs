using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Core
{
    public class MainServiceLocator : ISimpleLocator
    {
        private Dictionary<object, object> services = new Dictionary<object,object>();
        public TService Instance<TService>() where TService : class
        {
            return (TService)(services[typeof(TService)]);
        }

        public MainServiceLocator()
        {
            IDataContextProvider provider = new ModelContextProvider();

            services.Add(typeof(IDataAccessLayer), new ModelAccessLayer(provider));

            services.Add(typeof(IPersonService), new PersonService(this));
        }
    }
}
