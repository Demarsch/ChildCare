using System;
using System.Collections;
using PatientRecordsModule.Services;
using WpfControls.Editors;

namespace PatientRecordsModule.Misc
{
    public class MKBSuggestionProvider : ISuggestionProvider
    {
        private readonly IPatientRecordsService patientRecordsService;

        public MKBSuggestionProvider(IPatientRecordsService patientRecordsService)
        {
            if (patientRecordsService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            this.patientRecordsService = patientRecordsService;
        }

        public IEnumerable GetSuggestions(string filter)
        {
            return patientRecordsService.GetMKBs(filter);
        }
    }
}
