using Microsoft.Practices.Unity;
using PatientRecordsModule.ViewModels;

namespace PatientRecordsModule.Views
{
    /// <summary>
    /// Interaction logic for RecordDocsView.xaml
    /// </summary>
    public partial class RecordDocumentsView
    {
        public RecordDocumentsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public RecordDocumentsCollectionViewModel ViewModel
        {
            get { return DataContext as RecordDocumentsCollectionViewModel; }
            set { DataContext = value; }
        }
    }
}
