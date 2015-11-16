using Microsoft.Practices.Unity;
using AdminModule.ViewModels;

namespace AdminModule.Views
{
    /// <summary>
    /// Interaction logic for ReportTemplatesManagerView.xaml
    /// </summary>
    public partial class ReportTemplatesManagerView
    {
        public ReportTemplatesManagerView()
        {
            InitializeComponent();
        }

        [Dependency]
        public ReportTemplatesManagerViewModel ViewModel
        {
            get { return DataContext as ReportTemplatesManagerViewModel; }
            set { DataContext = value; }
        }
    }
}
