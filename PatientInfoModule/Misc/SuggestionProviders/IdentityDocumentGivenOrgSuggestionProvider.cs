using System;
using System.Collections;
using PatientInfoModule.Services;
using WpfControls.Editors;

namespace PatientInfoModule.Misc
{
    public class IdentityDocumentGivenOrgSuggestionProvider : ISuggestionProvider
    {
        private readonly IPatientService patientService;

        public IdentityDocumentGivenOrgSuggestionProvider(IPatientService patientService)
        {
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            this.patientService = patientService;
        }

        public IEnumerable GetSuggestions(string filter)
        {
            return patientService.GetIdentityDocumentGivenOrganizations(filter);
        }
    }
}
