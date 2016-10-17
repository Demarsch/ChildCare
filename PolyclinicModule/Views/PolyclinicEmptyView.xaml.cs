using Microsoft.Practices.Unity;
using PolyclinicModule.ViewModels;

namespace PolyclinicModule.Views
{
    /// <summary>
    /// Interaction logic for PolyclinicEmptyView.xaml
    /// </summary>
    public partial class PolyclinicEmptyView
    {
        public PolyclinicEmptyView()
        {
            InitializeComponent();
        }

        [Dependency]
        public PolyclinicEmptyViewModel ViewModel
        {
            get { return DataContext as PolyclinicEmptyViewModel; }
            set { DataContext = value; }
        }
    }
}
