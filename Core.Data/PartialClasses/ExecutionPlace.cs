using System;

namespace Core.Data
{
    public partial class ExecutionPlace
    {
        public static readonly string PoliclynicKey = "|Ambulatory|";

        public bool IsPolyclynic
        {
            get { return !string.IsNullOrEmpty(Options) && Options.IndexOf(PoliclynicKey, StringComparison.CurrentCultureIgnoreCase) != -1; }
        }
    }
}
