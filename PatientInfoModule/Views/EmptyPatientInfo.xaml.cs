using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for EmptyPatientInfo.xaml
    /// </summary>
    public partial class EmptyPatientInfo
    {
        public EmptyPatientInfo()
        {
            InitializeComponent();
        }

        [Dependency]
        public EmptyPatientInfoViewModel ViewModel
        {
            get { return DataContext as EmptyPatientInfoViewModel; }
            set { DataContext = value; }
        }
    }
}
