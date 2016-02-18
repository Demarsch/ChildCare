using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class RecordTypeValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public RecordTypeValidator(IDbContextProvider contextProvider)
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
                if (!context.Set<RecordType>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.Contract)))
                    result.Add(new ValidationResult(false, string.Format("В таблице RecordTypes отсутствует информация об услуге 'Договор'. В его поле Options должно содержаться значение '{0}' и поле IsRecord = false", OptionValues.Contract)));
                
                return result;
            }
        }
    }
}
