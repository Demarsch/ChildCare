using Fluent;
using Microsoft.Practices.Unity;
using PatientInfoModule.Misc;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for CommonInfoHeader.xaml
    /// </summary>
    public partial class InfoHeader
    {
        public InfoHeader()
        {
            InitializeComponent();
        }

        [Dependency]
        public InfoHeaderViewModel ViewModel
        {
            get { return DataContext as InfoHeaderViewModel; }
            set { DataContext = value; }
        }

        [Dependency(Common.RibbonGroupName)]
        public RibbonContextualTabGroup ContextualTabGroup
        {
            set { Group = value; }
        }
    }
}
