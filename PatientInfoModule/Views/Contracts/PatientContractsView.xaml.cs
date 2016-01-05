using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;
using System.Windows.Input;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for PatientContractsView.xaml
    /// </summary>
    public partial class PatientContractsView
    {
        public PatientContractsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PatientContractsViewModel ViewModel
        {
            get { return DataContext as PatientContractsViewModel; }
            set { DataContext = value; }
        }
    }
}
