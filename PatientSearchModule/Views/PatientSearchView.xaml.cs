using System.Windows.Input;
using Microsoft.Practices.Unity;
using PatientSearchModule.ViewModels;

namespace PatientSearchModule.Views
{
    /// <summary>
    /// Interaction logic for PatientSearchView.xaml
    /// </summary>
    public partial class PatientSearchView
    {
        [Dependency]
        public PatientSearchViewModel ViewModel
        {
            set { DataContext = value; }
        }

        public PatientSearchView()
        {
            InitializeComponent();
        }
    }
}
