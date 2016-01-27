using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class UrgencyValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public UrgencyValidator(IDbContextProvider contextProvider)
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
                var result = new List<ValidationResult>();
                if (!context.Set<Urgently>().Any(x => x.IsDefault))
                {
                    result.Add(new ValidationResult(false, "В таблице Urgentlies отсутствует запись, чей флаг IsDefault равен единице"));
                }
                if (!context.Set<Urgently>().Any(x => x.IsUrgently))
                {
                    result.Add(new ValidationResult(false, "В таблице Urgentlies отсутствует запись, чей флаг IsUrgently равен единице"));
                }
                return result;
            }
        }
    }
}
