using System;
using System.Collections;
using System.Linq;
using Core.Data;
using Core.Misc;
using Core.Services;
using WpfControls.Editors;

namespace PatientInfoModule.Misc
{
    public class OkatoRegionSuggestionProvider : ISuggestionProvider
    {
        private readonly Okato[] regions;

        public OkatoRegionSuggestionProvider(ICacheService cacheService)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            regions = cacheService.GetItems<Okato>()
                .Where(x => x.CodeOKATO.EndsWith("000000000") || x.CodeOKATO.StartsWith("C"))
                .ToArray();
        }

        public IEnumerable GetSuggestions(string filter)
        {
            filter = (filter ?? string.Empty).Trim();
            if (filter.Length < AppConfiguration.UserInputSearchThreshold)
            {
                return new Okato[0];
            }
            return regions.Where(x => x.FullName.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) != -1);
        }
    }
}
