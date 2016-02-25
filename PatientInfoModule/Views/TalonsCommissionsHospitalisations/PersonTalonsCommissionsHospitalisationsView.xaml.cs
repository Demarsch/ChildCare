using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for PersonTalonsCommissionsHospitalisationsView.xaml
    /// </summary>
    public partial class PersonTalonsCommissionsHospitalisationsView
    {
        public PersonTalonsCommissionsHospitalisationsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonTalonsCommissionsHospitalisationsViewModel ViewModel
        {
            get { return DataContext as PersonTalonsCommissionsHospitalisationsViewModel; }
            set { DataContext = value; }
        }
    }
}
