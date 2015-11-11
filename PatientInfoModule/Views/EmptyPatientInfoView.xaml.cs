using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for EmptyPatientInfoView.xaml
    /// </summary>
    public partial class EmptyPatientInfoView
    {
        public EmptyPatientInfoView()
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
