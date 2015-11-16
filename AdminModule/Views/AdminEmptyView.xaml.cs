using Microsoft.Practices.Unity;
using AdminModule.ViewModels;

namespace AdminModule.Views
{
    /// <summary>
    /// Interaction logic for AdminEmptyView.xaml
    /// </summary>
    public partial class AdminEmptyView
    {
        public AdminEmptyView()
        {
            InitializeComponent();
        }

        [Dependency]
        public AdminEmptyViewModel ViewModel
        {
            get { return DataContext as AdminEmptyViewModel; }
            set { DataContext = value; }
        }
    }
}
