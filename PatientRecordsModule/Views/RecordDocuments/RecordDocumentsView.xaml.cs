using Microsoft.Practices.Unity;
using Shared.PatientRecords.ViewModels;

namespace Shared.PatientRecords.Views
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
