using System;
using System.Collections;
using System.Linq;
using Core.Data;
using Core.Extensions;
using Core.Services;
using Core.Wpf.Misc;
using Core.Wpf.Services;
using Shared.Patient.Services;
using Shared.Patient.ViewModels;

namespace Shared.Patient.Misc
{
    public class PersonSearchSuggestionProvider : ISuggestionsProvider
    {
        private readonly IPersonSearchService personSearchService;

        private readonly ICacheService cacheService;

        private readonly IFileService fileService;

        public PersonSearchSuggestionProvider(IPersonSearchService personSearchService, ICacheService cacheService, IFileService fileService)
        {
            if (personSearchService == null)
            {
                throw new ArgumentNullException("personSearchService");
            }
            if (cacheService == null)
            {
                throw new ArgumentNullException("cacheService");
            }
            if (fileService == null)
            {
                throw new ArgumentNullException("fileService");
            }
            this.personSearchService = personSearchService;
            this.cacheService = cacheService;
            this.fileService = fileService;
        }

        public IEnumerable GetSuggestions(string filter)
        {
            using (var query = personSearchService.GetPatientSearchQuery(filter))
            {
                var result = query.PersonsQuery
                                  .Select(x => new
                                               {
                                                   x.Id,
                                                   x.BirthDate,
                                                   x.IsMale,
                                                   x.Snils,
                                                   x.MedNumber,
                                                   CurrentName = x.PersonNames.FirstOrDefault(y => y.EndDateTime == null || y.EndDateTime == DateTime.MaxValue),
                                                   PreviousName = x.PersonNames.Where(y => y.EndDateTime != null && y.EndDateTime != DateTime.MaxValue)
                                                                   .OrderByDescending(y => y.BeginDateTime)
                                                                   .FirstOrDefault(),
                                                   IdentityDocument = x.PersonIdentityDocuments
                                                                       .OrderByDescending(y => y.BeginDate)
                                                                       .FirstOrDefault(),
                                                   Photo = x.Document.FileData
                                               })
                                  .ToArray();
                return result.Select(x => new FoundPersonViewModel
                                          {
                                              Id = x.Id,
                                              BirthDate = x.BirthDate,
                                              CurrentName = x.CurrentName,
                                              PreviousName = x.PreviousName,
                                              IsMale = x.IsMale,
                                              IdentityDocument = cacheService.AutoWire(x.IdentityDocument, y => y.IdentityDocumentType),
                                              MedNumber = x.MedNumber,
                                              Snils = Person.DelimitizeSnils(x.Snils),
                                              Photo = fileService.GetImageSourceFromBinaryData(x.Photo)
                                          })
                             .ToArray();
            }
        }
    }
}