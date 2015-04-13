using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;
using log4net;
using log4net.Core;

namespace Core
{
    public class MainServiceLocator : ISimpleLocator
    {
        private Dictionary<object, object> services = new Dictionary<object,object>();
        public TService Instance<TService>() where TService : class
        {
            return (TService)(services[typeof(TService)]);
        }

        public void Add<TService>(Type interfaceType, TService service) where TService : class
        {
            services[interfaceType] = service;
        }
        
        public MainServiceLocator()
        {
            Add(typeof(ILog), new LogImpl(LoggerManager.CreateRepository("ChildCare").GetLogger("ChildCare")));

            Add(typeof(IUserSystemInfoService), new ADUserSystemInfoService());

            IDataContextProvider provider = new ModelContextProvider();

            Add(typeof(IDataAccessLayer), new ModelAccessLayer(provider));

            Add(typeof(IPersonService), new PersonService(this));
        }
    }
}
