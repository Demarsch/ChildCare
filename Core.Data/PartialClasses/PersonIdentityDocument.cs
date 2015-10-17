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
    }
}
