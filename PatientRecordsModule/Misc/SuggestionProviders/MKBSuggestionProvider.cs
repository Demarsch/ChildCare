using System;
using System.Collections;
using Shared.PatientRecords.Services;
using WpfControls.Editors;

namespace Shared.PatientRecords.Misc
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
