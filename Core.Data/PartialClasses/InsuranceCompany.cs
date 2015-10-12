namespace Core.Data
{
    public partial class InsuranceCompany
    {
        public override bool Equals(object obj)
        {
            var other = obj as InsuranceCompany;
            if (other == null)
            {
                return false;
            }
            if (Id < 1 || other.Id < 1)
            {
                return false;
            }
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
