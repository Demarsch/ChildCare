using System;

namespace PatientInfoModule.Misc
{
    public class DuplicatePersonCheckParameters
    {
        public int Id { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public DateTime? BirthDate { get; set; }

        public string Snils { get; set; }

        protected bool Equals(DuplicatePersonCheckParameters other)
        {
            return Id == other.Id
                && string.Compare(LastName, other.LastName, StringComparison.CurrentCultureIgnoreCase) == 0
                && string.Compare(FirstName, other.FirstName, StringComparison.CurrentCultureIgnoreCase) == 0
                && string.Compare(MiddleName, other.MiddleName, StringComparison.CurrentCultureIgnoreCase) == 0  
                && BirthDate.Equals(other.BirthDate) 
                && string.Equals(Snils, other.Snils);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DuplicatePersonCheckParameters)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (LastName != null ? LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Id;
                hashCode = (hashCode * 397) ^ (FirstName != null ? FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MiddleName != null ? MiddleName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ BirthDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (Snils != null ? Snils.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
