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

        private double valueY;
        public double ValueY
        {
            get { return valueY; }
            set { SetProperty(ref valueY, value); }
        }

        private string valueX;
        public string ValueX
        {
            get { return valueX; }
            set { SetProperty(ref valueX, value); }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }
    }
}
