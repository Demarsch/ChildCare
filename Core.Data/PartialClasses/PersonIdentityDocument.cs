using System;

namespace Core.Data
{
    public partial class PersonIdentityDocument
    {
        public static readonly string UnknownDocumentSeriesAndNumber = "?? ????";

        public string SeriesAndNumber
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Series) && !string.IsNullOrWhiteSpace(Number))
                {
                    return string.Format("{0} {1}", Series.Trim(), Number.Trim());
                }
                if (string.IsNullOrWhiteSpace(Series))
                {
                    return Number.Trim();
                }
                if (string.IsNullOrWhiteSpace(Number))
                {
                    return Series.Trim();
                }
                return UnknownDocumentSeriesAndNumber;
            }
        }

        protected bool Equals(PersonIdentityDocument other)
        {
            return Id == other.Id 
                && PersonId == other.PersonId 
                && IdentityDocumentTypeId == other.IdentityDocumentTypeId 
                && string.Compare(Series, other.Series, StringComparison.InvariantCultureIgnoreCase) == 0
                && string.Compare(Number, other.Number, StringComparison.InvariantCultureIgnoreCase) == 0
                && string.Compare(GivenOrg, other.GivenOrg, StringComparison.InvariantCultureIgnoreCase) == 0
                && BeginDate.Equals(other.BeginDate)
                && EndDate.Equals(other.EndDate);
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
            return Equals((PersonIdentityDocument)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ PersonId;
                hashCode = (hashCode * 397) ^ IdentityDocumentTypeId;
                hashCode = (hashCode * 397) ^ (Series != null ? Series.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Number != null ? Number.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (GivenOrg != null ? GivenOrg.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ BeginDate.GetHashCode();
                hashCode = (hashCode * 397) ^ EndDate.GetHashCode();
                return hashCode;
            }
        }
    }
}
