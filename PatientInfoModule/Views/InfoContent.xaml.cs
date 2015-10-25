using Microsoft.Practices.Unity;
using PatientInfoModule.ViewModels;

namespace PatientInfoModule.Views
{
    /// <summary>
    /// Interaction logic for InfoContent.xaml
    /// </summary>
    public partial class InfoContent
    {
        public InfoContent()
        {
            InitializeComponent();
        }

        [Dependency]
        public InfoContentViewModel ContentViewModel
        {
            get { return DataContext as InfoContentViewModel; }
            set { DataContext = value; }
        }
    }
}
