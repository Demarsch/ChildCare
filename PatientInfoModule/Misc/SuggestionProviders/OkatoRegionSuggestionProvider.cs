using System;
using System.Collections;
using System.Linq;
using Core.Data;
using Core.Misc;
using Core.Services;
using Core.Wpf.Misc;

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

        private readonly char[] separators = { ' ' };

        public IEnumerable GetSuggestions(string filter)
        {
            filter = (filter ?? string.Empty).Trim();
            if (filter.Length < AppConfiguration.UserInputSearchThreshold)
            {
                return new Okato[0];
            }
            var words = filter.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return regions.Where(x => words.All(y => x.FullName.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1))
                          .Take(AppConfiguration.SearchResultTakeTopCount);
        }
    }
}