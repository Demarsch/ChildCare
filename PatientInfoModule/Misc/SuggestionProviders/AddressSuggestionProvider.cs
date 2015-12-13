using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Misc;
using Core.Services;
using log4net;
using Microsoft.Practices.Unity;
using Prism.Mvvm;
using WpfControls.Editors;

namespace PatientInfoModule.Misc
{
    public class AddressSuggestionProvider : BindableBase, IAddressSuggestionProvider
    {
        private readonly LocationSuggestionProviderInternal locationSuggestionProvider;

        public AddressSuggestionProvider(ICacheService cacheService, ILog log)
        {
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            locationSuggestionProvider = new LocationSuggestionProviderInternal(cacheService, () => SelectedRegion, log);
        }
        [Dependency(SuggestionProviderNames.OkatoRegion)]
        public ISuggestionProvider RegionSuggestionProvider { get; set; }

        private Okato selectedRegion;

        public Okato SelectedRegion
        {
            get { return selectedRegion; }
            set { SetProperty(ref selectedRegion, value); }
        }

        public ISuggestionProvider LocationSuggestionProvider
        {
            get { return locationSuggestionProvider; }
        }

        public async Task EnsureDataSourceLoadedAsync()
        {
            await locationSuggestionProvider.LoadDataSourcesAsync();
        }

        private class LocationSuggestionProviderInternal : ISuggestionProvider 
        {
            private readonly ICacheService cacheService;

            private readonly Func<Okato> regionFactory;

            private readonly ILog log;

            private Dictionary<Okato, List<Okato>> regionLocations;  

            public LocationSuggestionProviderInternal(ICacheService cacheService, Func<Okato> regionFactory, ILog log)
            {
                if (cacheService == null)
                {
                    throw new ArgumentNullException("cacheService");
                }
                if (regionFactory == null)
                {
                    throw new ArgumentNullException("regionFactory");
                }
                if (log == null)
                {
                    throw new ArgumentNullException("log");
                }
                this.cacheService = cacheService;
                this.regionFactory = regionFactory;
                this.log = log;
            }

            private TaskCompletionSource<object> source; 

            public async Task LoadDataSourcesAsync()
            {
                if (source != null)
                {
                    return;
                }
                source = new TaskCompletionSource<object>();
                log.Info("Building hiearchy for OKATO regions...");
                await Task.Factory.StartNew(LoadDataSources);
                log.Info("Building hieararchy for OKATO regions completed");
                source.SetResult(null);
            }

            private void LoadDataSources()
            {
                regionLocations = cacheService.GetItems<Okato>()
                                              .Where(x => x.IsRegion || x.IsForeignCountry)
                                              .ToDictionary(x => x, x => new List<Okato>());
                foreach (var okato in cacheService.GetItems<Okato>())
                {
                    List<Okato> location;
                    if (regionLocations.TryGetValue(okato, out location))
                    {
                        continue;
                    }
                    var regionCode = okato.CodeOKATO.Substring(0, 2);
                    location = regionLocations.Where(x => x.Key.CodeOKATO.StartsWith(regionCode) && okato.FullName.StartsWith(x.Key.FullName))
                                              .Select(x => x.Value)
                                              .FirstOrDefault();
                    if (location == null)
                    {
                        log.WarnFormat("Can't locate parent OKATO region for location with code '{0}'", okato.CodeOKATO);
                    }
                    else
                    {
                        location.Add(okato);
                    }
                }
            }

            public IEnumerable GetSuggestions(string filter)
            {
                source.Task.Wait();
                var selectedRegion = regionFactory();
                filter = (filter ?? string.Empty).Trim();
                if (selectedRegion == null || filter.Length < AppConfiguration.UserInputSearchThreshold)
                {
                    return new Okato[0];
                }
                var words = filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return regionLocations[selectedRegion].Where(x => words.All(y => x.FullName.IndexOf(y, StringComparison.CurrentCultureIgnoreCase) != -1))
                                                      .Take(AppConfiguration.SearchResultTakeTopCount);
            }
        }
    }
}