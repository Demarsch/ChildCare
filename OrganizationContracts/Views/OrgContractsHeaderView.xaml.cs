using Microsoft.Practices.Unity;
using OrganizationContractsModule.ViewModels;

namespace OrganizationContractsModule.Views
{
    /// <summary>
    /// Interaction logic for OrgContractsHeaderView.xaml
    /// </summary>
    public partial class OrgContractsHeaderView
    {
        public OrgContractsHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public OrgContractsHeaderViewModel ViewModel
        {
            get { return DataContext as OrgContractsHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
