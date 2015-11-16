using System;
using System.Collections;
using PatientInfoModule.Services;
using WpfControls.Editors;

namespace PatientInfoModule.Misc
{
    public class DisabilityDocumentGivenOrgSuggestionProvider : ISuggestionProvider
    {
        private readonly IPatientService patientService;

        public DisabilityDocumentGivenOrgSuggestionProvider(IPatientService patientService)
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
