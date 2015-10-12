namespace DataLib
{
    public partial class InsuranceCompany
    {
        public override bool Equals(object obj)
        {
            var insuranceCompany2 = obj as InsuranceCompany;
            if (insuranceCompany2 == null)
                return false;
            if (this.Id < 1 || insuranceCompany2.Id < 1)
                return false;
            return this.Id == insuranceCompany2.Id;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }
    }
}
