using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class IdentityDocumentTypesValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public IdentityDocumentTypesValidator(IDbContextProvider contextProvider)
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
                var options = new HashSet<string>(context.Set<IdentityDocumentType>().Where(x => x.Options != null).Select(x => x.Options));
                if (!options.Any(x => x.Contains(IdentityDocumentType.IsRussianPassportOption)))
                {
                    result.Add(new ValidationResult(false, string.Format("В таблице IdentityDocumentTypes отсутствует информация о российском паспорте. В его поле Options должно содержаться значение '{0}'", IdentityDocumentType.IsRussianPassportOption)));
                }
                if (!options.Any(x => x.Contains(IdentityDocumentType.IsRussianBirthCertificateOption)))
                {
                    result.Add(new ValidationResult(false, string.Format("В таблице IdentityDocumentTypes отсутствует информация о российском свидетельстве о рождении. В его поле Options должно содержаться значение '{0}'", IdentityDocumentType.IsRussianBirthCertificateOption)));
                }
                if (!options.Any(x => x.Contains(IdentityDocumentType.IsRussianForeignPassportOption)))
                {
                    result.Add(new ValidationResult(false, string.Format("В таблице IdentityDocumentTypes отсутствует информация о российском заграничном паспорте. В его поле Options должно содержаться значение '{0}'", IdentityDocumentType.IsRussianForeignPassportOption)));
                }
                return result;
            }
        }
    }
}
