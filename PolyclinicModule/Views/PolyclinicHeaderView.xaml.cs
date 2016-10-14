using PolyclinicModule.ViewModels;
using Microsoft.Practices.Unity;

namespace PolyclinicModule.Views
{
    /// <summary>
    /// Interaction logic for PolyclinicHeaderView.xaml
    /// </summary>
    public partial class PolyclinicHeaderView
    {
        public PolyclinicHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PolyclinicHeaderViewModel ViewModel
        {
            get { return DataContext as PolyclinicHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
