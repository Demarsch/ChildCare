using Microsoft.Practices.Unity;
using PatientSearchModule.ViewModels;

namespace PatientSearchModule.Views
{
    /// <summary>
    /// Interaction logic for PatientSearch.xaml
    /// </summary>
    public partial class PatientSearch
    {
        [Dependency]
        public PatientSearchViewModel ViewModel
        {
            set { DataContext = value; }
        }

        public PatientSearch()
        {
            InitializeComponent();
        }
    }
}
