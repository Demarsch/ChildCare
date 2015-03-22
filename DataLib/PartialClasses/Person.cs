using System.Linq;

namespace DataLib
{
    public partial class Person
    {
        //TODO: shouldn't we sort them?
        public string CurrentLastName
        {
            get
            {
                var lastPersonName = PersonNames.LastOrDefault();
                return lastPersonName == null ? string.Empty : lastPersonName.LastName;
            }
        }
        //TODO: shouldn't we sort them?
        public string CurrentFirstName
        {
            get
            {
                var lastPersonName = PersonNames.LastOrDefault();
                return lastPersonName == null ? string.Empty : lastPersonName.FirstName;
            }
        }
        //TODO: shouldn't we sort them?
        public string CurrentMiddleName
        {
            get
            {
                var lastPersonName = PersonNames.LastOrDefault();
                return lastPersonName == null ? string.Empty : lastPersonName.MiddleName;
            }
        }
    }
}
