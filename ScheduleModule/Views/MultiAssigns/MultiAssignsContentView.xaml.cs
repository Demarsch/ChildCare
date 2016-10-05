using Microsoft.Practices.Unity;
using ScheduleModule.ViewModels;

namespace ScheduleModule.Views
{
    /// <summary>
    /// Interaction logic for ScheduleContentView.xaml
    /// </summary>
    public partial class MultiAssignsContentView
    {
        public MultiAssignsContentView()
        {
            InitializeComponent();
        }

        [Dependency]
        public MultiAssignsContentViewModel ViewModel
        {
            get { return DataContext as MultiAssignsContentViewModel; }
            set { DataContext = value; }
        }
    }
}
