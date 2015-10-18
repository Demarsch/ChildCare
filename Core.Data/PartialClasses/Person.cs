namespace Core.Data
{
    public partial class Person
    {
        public static readonly string UnknownSnils = "отсутствует";

        public static readonly string UnknownMedNumber = "отсутствует";

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

        //ToDo: get photo from store
        public string PhotoUri
        {
            get
            {
                return string.Empty;
            }
        }

        public string BirthYear
        {
            get
            {
                return BirthDate.Year + " г.р.";
            }
        }

        public string DelimitizedSnils
        {
            get { return DelimitizeSnils(Snils); }
        }
    }
}
