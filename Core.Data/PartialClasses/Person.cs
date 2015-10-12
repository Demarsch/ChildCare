namespace Core.Data
{
    public partial class Person
    {
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
