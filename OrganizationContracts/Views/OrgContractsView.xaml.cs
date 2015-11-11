using Microsoft.Practices.Unity;
using OrganizationContractsModule.ViewModels;

namespace OrganizationContractsModule.Views
{
    /// <summary>
    /// Interaction logic for OrgContractsView.xaml
    /// </summary>
    public partial class OrgContractsView
    {
        public OrgContractsView()
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
