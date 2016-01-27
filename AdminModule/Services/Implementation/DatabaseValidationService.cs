using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using AdminModule.Model;

namespace AdminModule.Services
{
    public class DatabaseValidationService : IDatabaseValidtionService
    {
        private readonly IDatabaseValidator[] validators;

        public DatabaseValidationService(IEnumerable<IDatabaseValidator> validators)
        {
            if (validators == null)
            {
                throw new ArgumentNullException("validators");
            }
            this.validators = validators.ToArray();
            ValidatorCount = this.validators.Length;
        }

        public int ValidatorCount { get; private set; }

        public async Task ValidateDatabaseAsync(CancellationToken token, IProgress<IEnumerable<ValidationResult>> progress)
        {
            foreach (var validator in validators)
            {
                token.ThrowIfCancellationRequested();
                var results = await Task.Factory.StartNew(() => validator.Validate());
                progress.Report(results);
            }
        }
    }
}
