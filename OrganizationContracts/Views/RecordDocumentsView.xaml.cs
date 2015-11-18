using Microsoft.Practices.Unity;
using OrganizationContractsModule.ViewModels;

namespace OrganizationContractsModule.Views
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
