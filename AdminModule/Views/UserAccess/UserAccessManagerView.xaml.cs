using AdminModule.ViewModels;
using Microsoft.Practices.Unity;

namespace AdminModule.Views
{
    /// <summary>
    /// Interaction logic for UserAccessManagerView.xaml
    /// </summary>
    public partial class UserAccessManagerView
    {
        public UserAccessManagerView()
        {
            InitializeComponent();
        }

        [Dependency]
        public UserAccessManagerViewModel UserAccessManagerViewModel
        {
            get { return DataContext as UserAccessManagerViewModel; }
            set { DataContext = value; }
        }
    }
}
