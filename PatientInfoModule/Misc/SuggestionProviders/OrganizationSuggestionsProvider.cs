
using System;
using System.Collections;
using Core.Wpf.Misc;
using PatientInfoModule.Services;

namespace PatientInfoModule.Misc
{
    public class OrganizationSuggestionsProvider : ISuggestionsProvider
    {
        private readonly IPatientService patientService;

        public OrganizationSuggestionsProvider(IPatientService patientService)
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
