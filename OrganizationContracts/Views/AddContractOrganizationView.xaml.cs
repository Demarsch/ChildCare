using Microsoft.Practices.Unity;
using OrganizationContractsModule.ViewModels;

namespace OrganizationContractsModule.Views
{
    /// <summary>
    /// Interaction logic for AddContractOrganizationView.xaml
    /// </summary>
    public partial class AddContractOrganizationView
    {
        public AddContractOrganizationView()
        {
            InitializeComponent();
        }

        [Dependency]
        public AddContractOrganizationViewModel ViewModel
        {
            get { return DataContext as AddContractOrganizationViewModel; }
            set { DataContext = value; }
        }
    }
}
