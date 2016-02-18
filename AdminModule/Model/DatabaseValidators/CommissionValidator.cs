using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class CommissionValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public CommissionValidator(IDbContextProvider contextProvider)
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
                if (!context.Set<CommissionFilter>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.ProtocolsInProcess)))
                    result.Add(new ValidationResult(false, string.Format("В таблице CommissionFilters отсутствует фильтр 'Протоколы в работе'. В его поле Options должно содержаться значение '{0}'", OptionValues.ProtocolsInProcess)));
                if (!context.Set<CommissionFilter>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.ProtocolsPreliminary)))
                    result.Add(new ValidationResult(false, string.Format("В таблице CommissionFilters отсутствует фильтр 'Протоколы предварительные'. В его поле Options должно содержаться значение '{0}'", OptionValues.ProtocolsPreliminary)));
                if (!context.Set<CommissionFilter>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.ProtocolsOnCommission)))
                    result.Add(new ValidationResult(false, string.Format("В таблице CommissionFilters отсутствует фильтр 'Протоколы на комиссии'. В его поле Options должно содержаться значение '{0}'", OptionValues.ProtocolsOnCommission)));
                if (!context.Set<CommissionFilter>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.ProtocolsOnDate)))
                    result.Add(new ValidationResult(false, string.Format("В таблице CommissionFilters отсутствует фильтр 'Протоколы на дату'. В его поле Options должно содержаться значение '{0}' и '{1}'", OptionValues.ProtocolsOnDate, OptionValues.CommissionFilterHasDate)));
                if (!context.Set<CommissionFilter>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.ProtocolsAdded)))
                    result.Add(new ValidationResult(false, string.Format("В таблице CommissionFilters отсутствует фильтр 'Протоколы добавленные'. В его поле Options должно содержаться значение '{0}' и '{1}'", OptionValues.ProtocolsAdded, OptionValues.CommissionFilterHasDate)));
                if (!context.Set<CommissionFilter>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.ProtocolsAwaiting)))
                    result.Add(new ValidationResult(false, string.Format("В таблице CommissionFilters отсутствует фильтр 'Протоколы в ожидании'. В его поле Options должно содержаться значение '{0}'", OptionValues.ProtocolsAwaiting)));
               

                return result;
            }
        }
    }
}
