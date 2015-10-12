namespace Core.Data
{
    public partial class PersonTalon
    {
        public string NumberWithDate
        {
            get
            {
                return "(" + TalonNumber + (!string.IsNullOrWhiteSpace(MKB) ? " - " + MKB : string.Empty) + ") от " + TalonDateTime.ToShortDateString();
            }
        }
    }
}
