using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for AddContractRecordsView.xaml
    /// </summary>
    public partial class AddContractRecordsView
    {
        public AddContractRecordsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public AddContractRecordsViewModel ViewModel
        {
            get { return DataContext as AddContractRecordsViewModel; }
            set { DataContext = value; }
        }
    }
}
