using Microsoft.Practices.Unity;
using CommissionsModule.ViewModels;

namespace CommissionsModule.Views
{
    /// <summary>
    /// Interaction logic for CommissionJournalView.xaml
    /// </summary>
    public partial class CommissionJournalView
    {
        public CommissionJournalView()
        {
            InitializeComponent();
        }

        [Dependency]
        public CommissionJournalViewModel ViewModel
        {
            get { return DataContext as CommissionJournalViewModel; }
            set { DataContext = value; }
        }
    }
}
