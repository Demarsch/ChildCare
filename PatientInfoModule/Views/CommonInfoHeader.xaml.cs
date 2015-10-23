using Fluent;
using Microsoft.Practices.Unity;
using PatientInfoModule.Misc;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for CommonInfoHeader.xaml
    /// </summary>
    public partial class CommonInfoHeader
    {
        public CommonInfoHeader()
        {
            InitializeComponent();
        }

        [Dependency]
        public CommonInfoHeaderViewModel ViewModel
        {
            get { return DataContext as CommonInfoHeaderViewModel; }
            set { DataContext = value; }
        }

        [Dependency(Common.RibbonGroupName)]
        public RibbonContextualTabGroup ContextualTabGroup
        {
            set { Group = value; }
        }
    }
}
