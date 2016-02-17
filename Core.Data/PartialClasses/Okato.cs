namespace Core.Data
{
    public partial class Okato
    {
        public static readonly string ForeignCountryStartsWith = "C";

        public static readonly string RegionEndsWith = "00000000";

        public bool IsRegion { get { return CodeOKATO.EndsWith(RegionEndsWith); } }

        public bool IsForeignCountry { get { return CodeOKATO.StartsWith(ForeignCountryStartsWith); } }
    }
}
