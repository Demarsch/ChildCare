using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for InfoContentView.xaml
    /// </summary>
    public partial class InfoContentView
    {
        public InfoContentView()
        {
            InitializeComponent();
        }

        [Dependency]
        public InfoContentViewModel ViewModel
        {
            get { return DataContext as InfoContentViewModel; }
            set { DataContext = value; }
        }
    }
}
