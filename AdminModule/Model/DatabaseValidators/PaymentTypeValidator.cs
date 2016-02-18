using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class PaymentTypeValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public PaymentTypeValidator(IDbContextProvider contextProvider)
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
                if (!context.Set<PaymentType>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.Cash)))
                    result.Add(new ValidationResult(false, string.Format("В таблице PaymentTypes отсутствует информация о наличном способе оплаты. В его поле Options должно содержаться значение '{0}'", OptionValues.Cash)));
                if (!context.Set<PaymentType>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.Cashless)))
                    result.Add(new ValidationResult(false, string.Format("В таблице PaymentTypes отсутствует информация о безналичном способе оплаты. В его поле Options должно содержаться значение '{0}'", OptionValues.Cashless)));


                return result;
            }
        }
    }
}
