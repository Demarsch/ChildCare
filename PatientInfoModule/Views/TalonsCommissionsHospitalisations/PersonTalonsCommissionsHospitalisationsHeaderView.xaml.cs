using Fluent;
using Microsoft.Practices.Unity;
using PatientInfoModule.Misc;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for PersonTalonsCommissionsHospitalisationsHeaderView.xaml
    /// </summary>
    public partial class PersonTalonsCommissionsHospitalisationsHeaderView
    {
        public PersonTalonsCommissionsHospitalisationsHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonTalonsCommissionsHospitalisationsHeaderViewModel ViewModel
        {
            get { return DataContext as PersonTalonsCommissionsHospitalisationsHeaderViewModel; }
            set { DataContext = value; }
        }

        [Dependency(Common.RibbonGroupName)]
        public RibbonContextualTabGroup ContextualTabGroup
        {
            set { Group = value; }
        }
    }
}
