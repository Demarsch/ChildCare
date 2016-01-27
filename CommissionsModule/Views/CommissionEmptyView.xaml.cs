using Microsoft.Practices.Unity;
using CommissionsModule.ViewModels;

namespace CommissionsModule.Views
{
    /// <summary>
    /// Interaction logic for CommissionEmptyView.xaml
    /// </summary>
    public partial class CommissionEmptyView
    {
        public CommissionEmptyView()
        {
            InitializeComponent();
        }

        [Dependency]
        public CommissionEmptyViewModel ViewModel
        {
            get { return DataContext as CommissionEmptyViewModel; }
            set { DataContext = value; }
        }
    }
}
