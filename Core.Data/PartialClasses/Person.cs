using Core.Attributes;

namespace Core.Data
{
    [NonCachable]
    public partial class Person
    {
        public static readonly string UnknownSnils = "отсутствует";

        public static readonly string UnknownMedNumber = "отсутствует";

        public const int FullSnilsLength = 14;

        public const int FullMedNumberLength = 16;

        public static string DelimitizeSnils(string snils)
        {
            if (string.IsNullOrWhiteSpace(snils) || snils == UnknownSnils)
            {
                return UnknownSnils;
            }
            if (snils.Length == 11)
            {
                return string.Format("{0}-{1}-{2} {3}",
                                     snils.Substring(0, 3),
                                     snils.Substring(3, 3),
                                     snils.Substring(6, 3),
                                     snils.Substring(9, 2));
            }
            return snils;
        }

        public string BirthYear
        {
            get
            {
                return BirthDate.Year + " г.р.";
            }
        }

        public override string ToString()
        {
            return FullName + ", " + BirthYear;
        }
    }
}
