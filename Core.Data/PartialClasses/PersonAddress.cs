using System;

namespace Core.Data
{
    public partial class PersonAddress
    {
        protected bool Equals(PersonAddress other)
        {
            return Id == other.Id 
                && PersonId == other.PersonId 
                && AddressTypeId == other.AddressTypeId 
                && OkatoId == other.OkatoId
                && string.Compare(UserText, other.UserText, StringComparison.CurrentCultureIgnoreCase) == 0
                && string.Compare(House, other.House, StringComparison.CurrentCultureIgnoreCase) == 0
                && BeginDateTime.Equals(other.BeginDateTime) 
                && EndDateTime.Equals(other.EndDateTime) 
                && string.Compare(Building, other.Building, StringComparison.CurrentCultureIgnoreCase) == 0
                && string.Compare(Apartment, other.Apartment, StringComparison.CurrentCultureIgnoreCase) == 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PersonAddress)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ PersonId;
                hashCode = (hashCode * 397) ^ AddressTypeId;
                hashCode = (hashCode * 397) ^ OkatoId;
                hashCode = (hashCode * 397) ^ (UserText != null ? UserText.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (House != null ? House.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ BeginDateTime.GetHashCode();
                hashCode = (hashCode * 397) ^ EndDateTime.GetHashCode();
                hashCode = (hashCode * 397) ^ (Building != null ? Building.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Apartment != null ? Apartment.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
