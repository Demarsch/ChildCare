using Microsoft.Practices.Unity;
using PatientRecordsModule.ViewModels;

namespace PatientRecordsModule.Views
{
    /// <summary>
    /// Interaction logic for DiagnosesView.xaml
    /// </summary>
    public partial class DiagnosesView 
    {
        public DiagnosesView()
        {
            InitializeComponent();
        }

        [Dependency]
        public DiagnosesCollectionViewModel ViewModel
        {
            get { return DataContext as DiagnosesCollectionViewModel; }
            set { DataContext = value; }
        }
    }
}
