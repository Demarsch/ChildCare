using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;
using StuffLib;
using DataLib;

namespace Core
{
    public class MainContainerProvider
    {
        static Container container = null;

        public static Container GetContainer()
        {
            if (container == null)
                container = new Container();
            return container;
        }
    }
}
