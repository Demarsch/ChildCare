using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdminModule.Services
{
    public interface IDatabaseValidtionService
    {
        int ValidatorCount { get; }

        Task ValidateDatabaseAsync(CancellationToken token, IProgress<IEnumerable<ValidationResult>> progress);
    }
}
