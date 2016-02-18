using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class RecordTypeRoleValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public RecordTypeRoleValidator(IDbContextProvider contextProvider)
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
                if (!context.Set<RecordTypeRole>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.ResponsibleForContract)))
                    result.Add(new ValidationResult(false, string.Format("В таблице RecordTypeRoles отсутствует информация об ответственном за составление договоров. В его поле Options должно содержаться значение '{0}'", OptionValues.ResponsibleForContract)));

                return result;
            }
        }
    }
}
