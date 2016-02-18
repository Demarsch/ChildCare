using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class RoomValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public RoomValidator(IDbContextProvider contextProvider)
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
                if (!context.Set<Room>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.LaboratoryRoom)))
                    result.Add(new ValidationResult(false, string.Format("В таблице Rooms отсутствует информация о кабинете 'Лаборатория'. В его поле Options должно содержаться значение '{0}'", OptionValues.LaboratoryRoom)));

                return result;
            }
        }
    }
}
