using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommissionsModule.ViewModels
{
    public class CommissionProtocolViewModel: BindableBase
    {
        public CommissionProtocolViewModel()
        {
        }

        private int id;
        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private SolidColorBrush statusColor;
        public SolidColorBrush StatusColor
        {
            get { return statusColor; }
            set { SetProperty(ref statusColor, value); }
        }

        private ImageSource statusImage;
        public ImageSource StatusImage
        {
            get { return statusImage; }
            set { SetProperty(ref statusImage, value); }
        }

        private bool? isCompleted;
        public bool? IsCompleted
        {
            get { return isCompleted; }
            set { SetProperty(ref isCompleted, value); }
        }

        private string patientFIO;
        public string PatientFIO
        {
            get { return patientFIO; }
            set { SetProperty(ref patientFIO, value); }
        }

        private string birthDate;
        public string BirthDate
        {
            get { return birthDate; }
            set { SetProperty(ref birthDate, value); }
        }

        private string talon;
        public string Talon
        {
            get { return talon; }
            set { SetProperty(ref talon, value); }
        }

        private string mkb;
        public string MKB
        {
            get { return mkb; }
            set { SetProperty(ref mkb, value); }
        }

        private string incomeDateTime;
        public string IncomeDateTime
        {
            get { return incomeDateTime; }
            set { SetProperty(ref incomeDateTime, value); }
        }
    }
}
