using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for PatientInfo.xaml
    /// </summary>
    public partial class PatientInfo
    {
        public PatientInfo()
        {
            InitializeComponent();
        }

        [Dependency]
        public PatientInfoViewModel ViewModel
        {
            get { return DataContext as PatientInfoViewModel; }
            set { DataContext = value; }
        }
    }
}
