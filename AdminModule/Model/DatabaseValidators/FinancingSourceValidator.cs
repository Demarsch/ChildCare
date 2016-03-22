using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class FinancingSourceValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public FinancingSourceValidator(IDbContextProvider contextProvider)
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
                if (!context.Set<FinancingSource>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.HMH)))
                    result.Add(new ValidationResult(false, string.Format("В таблице FinancingSource отсутствует информация об источнике финансирования 'Средства из фонда ВМП'. В его поле Options должно содержаться значение '{0}'", OptionValues.HMH)));
                if (!context.Set<FinancingSource>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.OMS)))
                    result.Add(new ValidationResult(false, string.Format("В таблице FinancingSource отсутствует информация об источнике финансирования 'Средства из фонда ОМС'. В его поле Options должно содержаться значение '{0}'", OptionValues.OMS)));
                if (!context.Set<FinancingSource>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.Pay)))
                    result.Add(new ValidationResult(false, string.Format("В таблице FinancingSource отсутствует информация об источнике финансирования 'Средства, получаемые за счет оказания платных мед. услуг'. В их поле Options должно содержаться значение '{0}'", OptionValues.Pay)));
                if (!context.Set<FinancingSource>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.Donations)))
                    result.Add(new ValidationResult(false, string.Format("В таблице FinancingSource отсутствует информация об источнике финансирования 'Гуманитарные пожертвования'. В его поле Options должно содержаться значение '{0}'", OptionValues.Donations)));
                if (!context.Set<FinancingSource>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.DMS)))
                    result.Add(new ValidationResult(false, string.Format("В таблице FinancingSource отсутствует информация об источнике финансирования 'Платные услуги (ДМС)'. В его поле Options должно содержаться значение '{0}'", OptionValues.DMS)));
                if (!context.Set<FinancingSource>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.Organization)))
                    result.Add(new ValidationResult(false, string.Format("В таблице FinancingSource отсутствует информация об источнике финансирования 'Платные договорные'. В их поле Options должно содержаться значение '{0}'", OptionValues.Organization)));
                if (!context.Set<FinancingSource>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.IndividualContaract)))
                    result.Add(new ValidationResult(false, string.Format("В таблице FinancingSource отсутствует информация об источнике финансирования 'Платные физ. лица'. В его поле Options должно содержаться значение '{0}'", OptionValues.IndividualContaract)));
                return result;
            }
        }
    }
}
