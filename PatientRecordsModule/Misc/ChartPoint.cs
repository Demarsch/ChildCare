using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Shared.PatientRecords.Misc
{
    public class ChartPoint : BindableBase
    {
        public ChartPoint()
        {

        }

        private double result;
        public double Result
        {
            get { return result; }
            set { SetProperty(ref result, value); }
        }

        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set { SetProperty(ref date, value); }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }
    }
}
