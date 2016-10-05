using Microsoft.Practices.Unity;
using ScheduleModule.ViewModels;

namespace ScheduleModule.Views
{
    /// <summary>
    /// Interaction logic for ScheduleHeaderView.xaml
    /// </summary>
    public partial class MultiAssignsHeaderView
    {
        public MultiAssignsHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public MultiAssignsHeaderViewModel ViewModel
        {
            get { return DataContext as MultiAssignsHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
