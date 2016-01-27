using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class ExecutionPlaceValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public ExecutionPlaceValidator(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IEnumerable<ValidationResult> Validate()
        {
            using (var context = contextProvider.CreateLightweightContext())
            {
                return context.Set<ExecutionPlace>().Any(x => x.Options != null && x.Options.Contains(ExecutionPlace.PoliclynicKey))
                           ? null
                           : new[] { new ValidationResult(false, string.Format("В таблице ExecutionPlaces отсутствует информация об амбулаторном месте выполнения. В его поле Options должно содержаться значение '{0}'", ExecutionPlace.PoliclynicKey)) };
            }
        }
    }
}
