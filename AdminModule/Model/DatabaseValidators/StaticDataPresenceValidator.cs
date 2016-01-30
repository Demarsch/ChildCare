using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Core.Data;
using Core.Data.Misc;
using Core.Data.Services;

namespace AdminModule.Model
{
    public class StaticDataPresenceValidator : IDatabaseValidator
    {
        private readonly IDbContextProvider contextProvider;

        public StaticDataPresenceValidator(IDbContextProvider contextProvider)
        {
            if (contextProvider == null)
            {
                throw new ArgumentNullException("contextProvider");
            }
            this.contextProvider = contextProvider;
        }

        public IEnumerable<ValidationResult> Validate()
        {
            var result = new List<ValidationResult>();
            using (var context = contextProvider.CreateLightweightContext())
            {
                if (!context.Set<AddressType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице AddressTypes"));
                }
                if (!context.Set<CommissionMemberType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице ComissionMemberTypes"));
                }
                if (!context.Set<CommissionType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице ComissionTypes"));
                }
                if (!context.Set<Complication>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице Complications"));
                }
                if (!context.Set<Country>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице Countries"));
                }
                if (!context.Set<DiagnosType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице DiagnosTypes"));
                }
                if (!context.Set<DisabilityType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице DisabilityTypes"));
                }
                if (!context.Set<Education>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице Educations"));
                }
                if (!context.Set<EqiupmentType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице EquipmentTypes"));
                }
                if (!context.Set<ExecutionPlace>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице ExecutionPlaces"));
                }
                if (!context.Set<FinancingSource>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице FinancingSources"));
                }
                if (!context.Set<HealthGroup>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице HealthGroups"));
                }
                if (!context.Set<IdentityDocumentType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице IdentityDocumentTypes"));
                }
                if (!context.Set<InsuranceCompany>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице InsuranceCompanies"));
                }
                if (!context.Set<IdentityDocumentType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице InsuranceDocumentTypes"));
                }
                if (!context.Set<MaritalStatus>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице MaritalStatuses"));
                }
                if (!context.Set<MedicalHelpType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице MedicalHelpTypes"));
                }
                if (!context.Set<MKB>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице MKB"));
                }
                if (!context.Set<MKBGroup>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице MKBGroups"));
                }
                if (!context.Set<Okato>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице Okatos"));
                }
                if (!context.Set<OuterDocumentType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице OuterDocumentTypes"));
                }
                if (!context.Set<PaymentType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице PaymentTypes"));
                }
                if (!context.Set<PersonStaff>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице PersonStaffs"));
                }
                if (!context.Set<RecordType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице RecordTypes"));
                }
                if (!context.Set<RecordTypeCost>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице RecordTypeCosts"));
                } 
                if (!context.Set<RecordTypeRolePermission>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице RecordTypeRolePermissions"));
                }
                if (!context.Set<RecordTypeRole>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице RecordTypeRoles"));
                }
                if (!context.Set<RelativeRelationship>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице RelativeRelationships"));
                }
                if (!context.Set<RelativeRelationshipConnection>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице RelativeRelationshipConnections"));
                }
                if (!context.Set<Room>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице Rooms"));
                }
                if (!context.Set<SocialStatusType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице SocialStatusTypes"));
                }
                if (!context.Set<Staff>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице Staffs"));
                }
                if (!context.Set<Unit>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице Units"));
                } 
                if (!context.Set<Urgently>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице Urgentlies"));
                }
                if (!context.Set<UserMessageType>().Any())
                {
                    result.Add(new ValidationResult(false, "Отсутствуют записи в таблице UserMessageTypes"));
                }
            }
            return result;
        }
    }
}
