using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class RecordContractValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public RecordContractValidator(IDbContextProvider contextProvider)
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
                if (!context.Set<RecordContract>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.HMH)))
                    result.Add(new ValidationResult(false, string.Format("В таблице RecordContracts отсутствует информация о договоре на оказание ВМП. В его поле Options должно содержаться значение '{0}'", OptionValues.HMH)));

                return result;
            }
        }
    }
}
