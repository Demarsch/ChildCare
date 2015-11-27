namespace Core.Data
{
    public partial class ExecutionPlace
    {
        public static readonly string PoliclynicKey = "Polyclinic";

        public bool IsPolyclynic
        {
            get { return !string.IsNullOrEmpty(Options) && Options.Contains(PoliclynicKey); }
        }
    }
}
