using Microsoft.Practices.Unity;
using OrganizationContractsModule.ViewModels;

namespace OrganizationContractsModule.Views
{
    /// <summary>
    /// Interaction logic for AddContractOrganization.xaml
    /// </summary>
    public partial class AddContractOrganization
    {
        public AddContractOrganization()
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
