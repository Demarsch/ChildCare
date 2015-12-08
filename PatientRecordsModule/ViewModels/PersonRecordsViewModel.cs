using Core.Data;
using Core.Data.Misc;
using Core.Wpf.Events;
using Core.Wpf.Services;
using log4net;
using Shared.PatientRecords.Misc;
using Shared.PatientRecords.Services;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.PatientRecords.ViewModels
{
    public class PersonRecordsViewModel : BindableBase, IConfirmNavigationRequest
    {
        #region Fields

        private readonly IPatientRecordsService patientRecordsService;
        private readonly IEventAggregator eventAggregator;
        private readonly ILog logService;
        #endregion

        #region  Constructors
        public PersonRecordsViewModel(IPatientRecordsService patientRecordsService, IEventAggregator eventAggregator, ILog logService)
        {
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("patientRecordsService");
            }
            if (logService == null)
            {
                throw new ArgumentNullException("logService");
            }
            this.logService = logService;
            this.eventAggregator = eventAggregator;
            this.patientRecordsService = patientRecordsService;
            personId = SpecialValues.NonExistingId;
        }
        #endregion

        #region Properties

        private int personId;
        public int PersonId
        {
            get { return personId; }
            set { SetProperty(ref personId, value); }
        }

        #endregion

        #region Methods
        #endregion

        #region IConfirmNavigationRequest implimentation

        public void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback(true);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var targetPatientId = (int?)navigationContext.Parameters[ParameterNames.PersonId] ?? SpecialValues.NonExistingId;
            if (targetPatientId != personId)
                PersonId = targetPatientId;
        }

        #endregion
    }
}
