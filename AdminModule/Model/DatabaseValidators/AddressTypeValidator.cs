using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class AddressTypeValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public AddressTypeValidator(IDbContextProvider contextProvider)
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
                if (!context.Set<AddressType>().Any(x => !string.IsNullOrEmpty(x.Category) && x.Category.Contains(AddressTypeCategory.Registry.ToString())))
                    result.Add(new ValidationResult(false, string.Format("В таблице AddressTypes отсутствует информация об адресе 'По паспорту', 'По временной регистрации', 'Проживание'. В их поле Category должно содержаться значение '{0}'", "|" + AddressTypeCategory.Registry.ToString() + "|")));
                if (!context.Set<AddressType>().Any(x => !string.IsNullOrEmpty(x.Category) && x.Category.Contains(AddressTypeCategory.Talon.ToString())))
                    result.Add(new ValidationResult(false, string.Format("В таблице AddressTypes отсутствует информация об адресе 'По талону'. В его поле Options должно содержаться значение '{0}'", "|" + AddressTypeCategory.Talon.ToString() + "|")));

                return result;
            }
        }
    }
}
