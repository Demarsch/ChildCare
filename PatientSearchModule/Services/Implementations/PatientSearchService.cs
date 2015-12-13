using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Expressions;
using Core.Misc;
using PatientSearchModule.Misc;

namespace PatientSearchModule.Services
{
    public class PatientSearchService : IPatientSearchService
    {
        private readonly IDbContextProvider contextProvider;

        private readonly ISearchExpressionProvider<Person> searchExpressionProvider;

        public PatientSearchService(IDbContextProvider contextProvider, ISearchExpressionProvider<Person> searchExpressionProvider)
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

        public PatientSearchQuery GetPatientSearchQuery(string searchPattern)
        {
            var searchExpression = searchExpressionProvider.CreateSearchExpression(searchPattern);
            if (searchExpression == null)
            {
                return PatientSearchQuery.Empty;
            }
            var dataContext = contextProvider.CreateNewContext();
            dataContext.Configuration.ProxyCreationEnabled = false;
            return new PatientSearchQuery(searchExpression.ParsedTokens,
                                          new DisposableQueryable<Person>(dataContext.Set<Person>()
                                                                                     .AsNoTracking()
                                                                                     .Where(searchExpression.FilterExpression)
                                                                                     .OrderByDescending(searchExpression.SimilarityExpression)
                                                                                     .Take(AppConfiguration.SearchResultTakeTopCount),
                                                                          dataContext));
        }
    }
}