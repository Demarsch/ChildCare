using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for PersonDocuments.xaml
    /// </summary>
    public partial class PersonDocuments
    {
        public PersonDocuments()
        {
            InitializeComponent();
        }

        [Dependency]
        public PersonDocumentsViewModel ViewModel
        {
            get { return DataContext as PersonDocumentsViewModel; }
            set { DataContext = value; }
        }
    }
}
