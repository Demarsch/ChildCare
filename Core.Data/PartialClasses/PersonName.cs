using System.Text;

namespace Core.Data
{
    public partial class PersonName
    {
        public static readonly string UnknownLastName = "?????";

        public static readonly string UnknownFirstName = "??";

        public string ShortName
        {
            get
            {
                var shortName = new StringBuilder();
                shortName.Append(string.IsNullOrEmpty(LastName) ? UnknownLastName : LastName)
                         .Append(' ')
                         .Append((string.IsNullOrEmpty(FirstName) ? UnknownFirstName : FirstName)[0])
                         .Append('.');
                if (!string.IsNullOrWhiteSpace(MiddleName))
                {
                    shortName.Append(' ')
                             .Append(MiddleName[0])
                             .Append('.');
                }
                return shortName.ToString();
            }
        }

        public string FullName
        {
            get
            {
                var fullName = new StringBuilder();
                fullName.Append(string.IsNullOrEmpty(LastName) ? UnknownLastName : LastName)
                        .Append(' ')
                        .Append(string.IsNullOrEmpty(FirstName) ? UnknownFirstName : FirstName);
                if (!string.IsNullOrWhiteSpace(MiddleName))
                {
                    fullName.Append(' ')
                            .Append(MiddleName);
                }
                return fullName.ToString();
            }
        }
    }
}
