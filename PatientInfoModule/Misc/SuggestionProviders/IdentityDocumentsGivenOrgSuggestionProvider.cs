using System;
using System.Collections;
using System.Linq;
using PatientInfoModule.Services;
using WpfControls.Editors;

namespace PatientInfoModule.Misc
{
    public class IdentityDocumentsGivenOrgSuggestionProvider : ISuggestionProvider
    {
        private readonly IPatientService patientService;

        public IdentityDocumentsGivenOrgSuggestionProvider(IPatientService patientService)
        {
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            this.patientService = patientService;
        }

        public IEnumerable GetSuggestions(string filter)
        {
            using (var query = patientService.GetDocumentGivenOrganizations(filter))
            {
                return query.ToArray();
            }
        }
    }
}
