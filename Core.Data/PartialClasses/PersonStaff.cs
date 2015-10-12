namespace Core.Data
{
    public partial class PersonStaff
    {        
        public string PersonName
        {
            get
            {
                return Person.ShortName;
            }
        }
    }
}
