using Microsoft.Practices.Unity;
using OrganizationContractsModule.ViewModels;

namespace OrganizationContractsModule.Views
{
    /// <summary>
    /// Interaction logic for OrgContracts.xaml
    /// </summary>
    public partial class OrgContracts
    {
        public OrgContracts()
        {
            InitializeComponent();
        }

        [Dependency]
        public OrgContractsViewModel ViewModel
        {
            get { return DataContext as OrgContractsViewModel; }
            set { DataContext = value; }
        }
    }
}
