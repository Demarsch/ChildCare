using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class DiagnosValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public DiagnosValidator(IDbContextProvider contextProvider)
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
                if (!context.Set<DiagnosType>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.DiagnosDischarge)))
                    result.Add(new ValidationResult(false, string.Format("В таблице DiagnosTypes отсутствует информация о типе 'Выписной диагноз'. В его поле Options должно содержаться значение '{0}' и поле MainDiagnosHeader = 'Пролечено'", OptionValues.DiagnosDischarge)));
                if (!context.Set<DiagnosType>().Any(x => !string.IsNullOrEmpty(x.Options) && x.Options.Contains(OptionValues.DiagnosSpecialistExamination)))
                    result.Add(new ValidationResult(false, string.Format("В таблице DiagnosTypes отсутствует информация о типе 'Осмотр специалиста'. В его поле Options должно содержаться значение '{0}' и поле MainDiagnosHeader = 'Диаг. обращения'", OptionValues.DiagnosSpecialistExamination)));

                return result;
            }
        }
    }
}
