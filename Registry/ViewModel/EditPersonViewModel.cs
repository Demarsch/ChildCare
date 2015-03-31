using Core;
using DataLib;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registry
{
    class EditPersonViewModel
    {
        private readonly ILog log;

        private readonly IPatientService service;

        private EditPersonDataViewModel editPersonDataViewModel;
        public EditPersonDataViewModel EditPersonDataViewModel
        {
            get { return editPersonDataViewModel; }
        }

         /// <summary>
        /// Use this for creating new person
        /// </summary>
        public EditPersonViewModel(ILog log, IPatientService service)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (service == null)
                throw new ArgumentNullException("service");
            this.service = service;
            this.log = log;

        }

          public EditPersonViewModel(ILog log, IPatientService service, int personId)
            : this(log, service)
        {
            Id = personId;
            this.log = log;
        }

        /// <summary>
        /// TODO: Use this for creating new person with default data from search
        /// </summary>
        public EditPersonViewModel(ILog log, IPatientService service, string personData)
            : this(log, service)
        {

        }

        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                if (id == value)
                    return;
                id = value;
                editPersonDataViewModel = new EditPersonDataViewModel(log, service, id);
            }
        }
    }
}
