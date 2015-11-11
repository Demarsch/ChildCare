using System;

namespace Core.Data
{
    public partial class InsuranceDocument
    {
        protected bool Equals(InsuranceDocument other)
        {
            return Id == other.Id 
                && PersonId == other.PersonId 
                && InsuranceCompanyId == other.InsuranceCompanyId 
                && InsuranceDocumentTypeId == other.InsuranceDocumentTypeId 
                && string.Compare(Series, other.Series, StringComparison.InvariantCultureIgnoreCase) == 0 
                && string.Compare(Number, other.Number, StringComparison.InvariantCultureIgnoreCase) == 0 
                && BeginDate.Equals(other.BeginDate) 
                && EndDate.Equals(other.EndDate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((InsuranceDocument)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ PersonId;
                hashCode = (hashCode * 397) ^ InsuranceCompanyId;
                hashCode = (hashCode * 397) ^ InsuranceDocumentTypeId;
                hashCode = (hashCode * 397) ^ (Series != null ? Series.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Number != null ? Number.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ BeginDate.GetHashCode();
                hashCode = (hashCode * 397) ^ EndDate.GetHashCode();
                return hashCode;
            }
        }
    }
}
