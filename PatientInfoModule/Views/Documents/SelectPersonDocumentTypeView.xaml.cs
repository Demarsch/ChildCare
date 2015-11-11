using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for SelectPersonDocumentTypeView.xaml
    /// </summary>
    public partial class SelectPersonDocumentTypeView 
    {
        public SelectPersonDocumentTypeView()
        {
            InitializeComponent();
        }

        [Dependency]
        public SelectPersonDocumentTypeViewModel ViewModel
        {
            get { return DataContext as SelectPersonDocumentTypeViewModel; }
            set { DataContext = value; }
        }
    }
}
