namespace Core.Data
{
    public partial class IdentityDocumentType
    {
        public const int RussianPassportSeriesDigitCount = 4;

        public const int RussianPassportNumberDigitCount = 6;

        public const int RussianBirthCertificateNumberDigitCount = 6;

        public const int RussianForeignPassportSeriesDigitCount = 2;

        public const int RussianForeignPassportNumberDigitCount = 7;

        #region Options

        public const string IsRussianPassportOption = "IsRussianPassport";

        public const string IsRussianBirthCertificateOption = "IsRussianBirthCertificate";

        public const string IsRussianForeignPassportOption = "IsRussianForeignPassport";

        #endregion
    }
}
