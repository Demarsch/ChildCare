using System;
using System.Linq;
using Core.Data;
using Core.Data.Services;
using Core.Expressions;
using Core.Misc;
using PatientSearchModule.Model;

namespace PatientSearchModule.Services
{
    public class PatientSearchService : IPatientSearchService
    {
        private const int UserInputThresholdLength = 3;

        private const int TopPatientCount = 5;

        private readonly IDbContextProvider contextProvider;

        private readonly IUserInputNormalizer userInputNormalizer;

        private readonly ISimilarityExpressionProvider<Person> similarityExpressionProvider;

        public PatientSearchService(IDbContextProvider contextProvider, IUserInputNormalizer userInputNormalizer, ISimilarityExpressionProvider<Person> similarityExpressionProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (userInputNormalizer == null)
            {
                throw new ArgumentNullException("userInputNormalizer");
            }
            if (similarityExpressionProvider == null)
            {
                throw new ArgumentNullException("similarityExpressionProvider");
            }
            this.similarityExpressionProvider = similarityExpressionProvider;
            this.userInputNormalizer = userInputNormalizer;
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<Person> PatientSearchQuery(string searchPattern)
        {
            var normalizedUserInput = userInputNormalizer.NormalizeUserInput(searchPattern);
            if (normalizedUserInput.Length < UserInputThresholdLength)
            {
                return DisposableQueryable<Person>.EmptyInstance;
            }
            var similarityExpression = similarityExpressionProvider.CreateSimilarityExpression(normalizedUserInput);
            if (similarityExpression == null)
            {
                return DisposableQueryable<Person>.EmptyInstance;
            }
            var dataContext = contextProvider.CreateNewContext();
            return new DisposableQueryable<Person>(dataContext.Set<Person>()
                                                              .AsNoTracking()
                                                              .OrderByDescending(similarityExpression)
                                                              .Take(TopPatientCount),
                                                   dataContext);
        }
    }
}