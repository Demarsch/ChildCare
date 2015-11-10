using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for PatientContracts.xaml
    /// </summary>
    public partial class PatientContracts
    {
        public PatientContracts()
        {
            InitializeComponent();
        }

        [Dependency]
        public PatientContractsViewModel ViewModel
        {
            get { return DataContext as PatientContractsViewModel; }
            set { DataContext = value; }
        }
    }
}
