
using System;
using System.Collections;
using PatientInfoModule.Services;
using WpfControls.Editors;

namespace PatientInfoModule.Misc
{
    public class OrganizationSuggestionProvider : ISuggestionProvider
    {
        private readonly IPatientService patientService;

        public OrganizationSuggestionProvider(IPatientService patientService)
        {
            if (patientService == null)
            {
                throw new ArgumentNullException("patientService");
            }
            this.patientService = patientService;
        }

        public IEnumerable GetSuggestions(string filter)
        {
            return patientService.GetOrganizations(filter);
        }
    }
}
