using GalaSoft.MvvmLight;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib
{
    class PatientInfoViewModel : ObservableObject
    {
        private readonly ILog log;

        //private readonly MainService service;

        public PatientInfoViewModel(ILog log/*, MainService service*/)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            //if (service == null)
            //    throw new ArgumentNullException("service");
            //this.service = service;
            this.log = log;
        }
    }
}
