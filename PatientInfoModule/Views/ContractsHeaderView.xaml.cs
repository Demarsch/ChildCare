using Fluent;
using Microsoft.Practices.Unity;
using PatientInfoModule.Misc;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for CommonInfoHeader.xaml
    /// </summary>
    public partial class ContractsHeader
    {
        public ContractsHeader()
        {
            InitializeComponent();
        }

        [Dependency]
        public ContractsHeaderViewModel ViewModel
        {
            get { return DataContext as ContractsHeaderViewModel; }
            set { DataContext = value; }
        }

        [Dependency(Common.RibbonGroupName)]
        public RibbonContextualTabGroup ContextualTabGroup
        {
            set { Group = value; }
        }
    }
}
