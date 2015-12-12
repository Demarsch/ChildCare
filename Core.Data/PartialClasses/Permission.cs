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

        #endregion

        public readonly static string ChangeRecordParentVisit = "Изменить родительский случай";

        public readonly static string ChangeRecordRoom = "Изменить кабинет услуги";
    }
}
