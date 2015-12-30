using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Expressions;
using Core.Misc;
using Shared.Patient.Misc;

namespace Shared.Patient.Services
{
    public class PersonSearchService : IPersonSearchService
    {
        private readonly IDbContextProvider contextProvider;

        private readonly ISearchExpressionProvider<Person> searchExpressionProvider;

        public PersonSearchService(IDbContextProvider contextProvider, ISearchExpressionProvider<Person> searchExpressionProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (searchExpressionProvider == null)
            {
                throw new ArgumentNullException("searchExpressionProvider");
            }
            this.searchExpressionProvider = searchExpressionProvider;
            this.contextProvider = contextProvider;
        }

        public PersonSearchQuery GetPatientSearchQuery(string searchPattern)
        {
            var searchExpression = searchExpressionProvider.CreateSearchExpression(searchPattern);
            if (searchExpression == null)
            {
                return PersonSearchQuery.Empty;
            }
            var dataContext = contextProvider.CreateNewContext();
            dataContext.Configuration.ProxyCreationEnabled = false;
            return new PersonSearchQuery(searchExpression.ParsedTokens,
                                          new DisposableQueryable<Person>(dataContext.Set<Person>()
                                                                                     .AsNoTracking()
                                                                                     .Where(searchExpression.FilterExpression)
                                                                                     .OrderByDescending(searchExpression.SimilarityExpression)
                                                                                     .Take(AppConfiguration.SearchResultTakeTopCount),
                                                                          dataContext));
        }
    }
}