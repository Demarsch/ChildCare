using AdminModule.ViewModels;
using Microsoft.Practices.Unity;

namespace AdminModule.Views
{
    /// <summary>
    /// Interaction logic for DatabaseValidationView.xaml
    /// </summary>
    public partial class DatabaseValidationView
    {
        public DatabaseValidationView()
        {
            InitializeComponent();
        }

        [Dependency]
        public DatabaseValidationViewModel DatabaseValidationViewModel
        {
            get { return DataContext as DatabaseValidationViewModel; }
            set { DataContext = value; }
        }
    }
}
