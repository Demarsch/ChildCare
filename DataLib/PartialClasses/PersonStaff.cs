namespace DataLib
{
    public partial class PersonStaff
    {        
        public string PersonName
        {
            get
            {
                return this.Person.ShortName;
            }
        }
    }
}
