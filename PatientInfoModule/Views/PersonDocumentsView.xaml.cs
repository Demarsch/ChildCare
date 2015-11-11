using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for PersonDocumentsView.xaml
    /// </summary>
    public partial class PersonDocumentsView
    {
        public PersonDocumentsView()
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
