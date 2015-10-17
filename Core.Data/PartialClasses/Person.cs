namespace Core.Data
{
    public partial class Person
    {
        public static readonly string UnknownSnils = "отсутствует";

        public static readonly string UnknownMedNumber = "отсутствует";

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
    }
}
