namespace Core.Data
{
    public partial class Permission
    {
        public override string ToString()
        {
            return string.Format("{0} - {1}", Id, Name);
        }

        #region Module access

        public const string PatientInfoModuleAccess = "Доступ к модулю \"Информация о пациенте\"";

        public const string ScheduleModuleAccess = "Доступ к модулю \"Расписание\"";

        public const string ScheduleEditorModuleAccess = "Доступ к модулю \"Редактор расписания\"";

        public const string AdminModuleAccess = "Доступ к модулю \"Администрирование\"";

        public const string OrganizationContractsModuleAccess = "Доступ к модулю \"Договора с юридическими лицами\"";

        public const string CommissionsModuleAccess = "Доступ к модулю \"Комиссии\"";

        #endregion

        #region Assignments

        public static readonly string EditAssignments = "Редактирование назначений";

        public static readonly string EditSchedule = "Редактирование расписания";

        public static readonly string DeleteTemporaryAssignments = "Удаление временных назначений";

        #endregion

        #region Commissions

        public readonly static string ShowAllCommissionDecisions = "Отображать все решения комиссии";

        public readonly static string DeleteCommissionProtocol = "Удалить протокол комиссии";

        public readonly static string EditCommissionMembers = "Редактировать состав комиссии";

        #endregion

        #region PersonTalons

        public readonly static string EditPersonTalon = "Редактировать талон пациента";
        
        public readonly static string DeletePersonTalon = "Удалить талон пациента";

        #endregion

        public readonly static string ChangeRecordParentVisit = "Изменить родительский случай";

        public readonly static string UseCompletedVisit = "Использовать закрытые случаи";

        public readonly static string ChangeRecordRoom = "Изменить кабинет услуги";

        public readonly static string CreateAmbCard = "Создать а/к";

        public readonly static string DeleteAmbCard = "Удалить а/к";
    }
}
