using System;
using Core.Data;
using Core.Data.Interfaces;
using Core.Misc;
using PatientSearchModule.Model;

namespace PatientSearchModule.Services
{
    public class PatientSearchService : IPatientSearchService
    {
        private const int UserInputThresholdLength = 3;

        private readonly IDbContextProvider contextProvider;

        private readonly IUserInputNormalizer userInputNormalizer;

        public PatientSearchService(IDbContextProvider contextProvider, IUserInputNormalizer userInputNormalizer)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            if (userInputNormalizer == null)
            {
                throw new ArgumentNullException("userInputNormalizer");
            }
            this.userInputNormalizer = userInputNormalizer;
            this.contextProvider = contextProvider;
        }

        public IDisposableQueryable<Person> SearchPatients(string searchPattern)
        {
            var normalizedUserInput = userInputNormalizer.NormalizeUserInput(searchPattern);
            if (normalizedUserInput.Length < UserInputThresholdLength)
            {
                return DisposableQueryable<Person>.EmptyInstance;
            }
            throw new NotImplementedException();
        }
    }
}