using System;

namespace Core.Data
{
    public partial class PersonSocialStatus
    {
        protected bool Equals(PersonSocialStatus other)
        {
            return Id == other.Id
                && PersonId == other.PersonId 
                && SocialStatusTypeId == other.SocialStatusTypeId 
                && string.Compare(Office, other.Office, StringComparison.CurrentCultureIgnoreCase) == 0
                && OrgId == other.OrgId 
                && BeginDateTime.Equals(other.BeginDateTime) 
                && EndDateTime.Equals(other.EndDateTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((PersonSocialStatus)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ PersonId;
                hashCode = (hashCode * 397) ^ SocialStatusTypeId;
                hashCode = (hashCode * 397) ^ (Office != null ? Office.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ OrgId.GetHashCode();
                hashCode = (hashCode * 397) ^ BeginDateTime.GetHashCode();
                hashCode = (hashCode * 397) ^ EndDateTime.GetHashCode();
                return hashCode;
            }
        }
    }
}
