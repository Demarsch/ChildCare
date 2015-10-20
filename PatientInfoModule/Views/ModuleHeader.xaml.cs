using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for ModuleHeader.xaml
    /// </summary>
    public partial class ModuleHeader
    {
        public ModuleHeader()
        {
            InitializeComponent();
        }

        [Dependency]
        public ModuleHeaderViewModel ModuleHeaderViewModel
        {
            get { return DataContext as ModuleHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
