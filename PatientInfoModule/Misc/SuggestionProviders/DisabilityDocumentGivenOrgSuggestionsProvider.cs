using System;
using System.Collections;
using Core.Wpf.Misc;
using PatientInfoModule.Services;

namespace PatientInfoModule.Misc
{
    public class DisabilityDocumentGivenOrgSuggestionsProvider : ISuggestionsProvider
    {
        private readonly IPatientService patientService;

        public DisabilityDocumentGivenOrgSuggestionsProvider(IPatientService patientService)
        {
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            this.patientService = patientService;
        }

        public IEnumerable GetSuggestions(string filter)
        {
            return patientService.GetDisabilityDocumentGivenOrganizations(filter);
        }
    }
}
