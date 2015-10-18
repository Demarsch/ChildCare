using System;
using System.Linq;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;
using Core.Expressions;
using PatientSearchModule.Misc;
using PatientSearchModule.Model;

namespace PatientSearchModule.Services
{
    public class PatientSearchService : IPatientSearchService
    {
        private const int UserInputThresholdLength = 3;

        private const int TopPatientCount = 5;

        private readonly IDbContextProvider contextProvider;

        private readonly IUserInputNormalizer userInputNormalizer;

        private readonly ISearchExpressionProvider<Person> searchExpressionProvider;

        public PatientSearchService(IDbContextProvider contextProvider, IUserInputNormalizer userInputNormalizer, ISearchExpressionProvider<Person> searchExpressionProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (userInputNormalizer == null)
            {
                throw new ArgumentNullException("userInputNormalizer");
            }
            if (searchExpressionProvider == null)
            {
                throw new ArgumentNullException("searchExpressionProvider");
            }
            this.searchExpressionProvider = searchExpressionProvider;
            this.userInputNormalizer = userInputNormalizer;
            this.contextProvider = contextProvider;
        }

        public PatientSearchQuery GetPatientSearchQuery(string searchPattern)
        {
            var normalizedUserInput = userInputNormalizer.NormalizeUserInput(searchPattern);
            if (normalizedUserInput.Length < UserInputThresholdLength)
            {
                return PatientSearchQuery.Empty;
            }
            var searchExpression = searchExpressionProvider.CreateSearchExpression(normalizedUserInput);
            if (searchExpression == null)
            {
                return PatientSearchQuery.Empty;
            }
            var dataContext = contextProvider.CreateNewContext();
            dataContext.Configuration.ProxyCreationEnabled = false;
            return new PatientSearchQuery(searchExpression.ParsedTokens,
                                          new DisposableQueryable<Person>(dataContext.Set<Person>()
                                                                                     .AsNoTracking()
                                                                                     .OrderByDescending(searchExpression.SimilarityExpression)
                                                                                     .Take(TopPatientCount)
                                                                                     .Where(searchExpression.FilterExpression),
                                                                          dataContext));
        }
    }
}