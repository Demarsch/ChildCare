using Microsoft.Practices.Unity;
using ScheduleModule.ViewModels;

namespace ScheduleModule.Views
{
    /// <summary>
    /// Interaction logic for ScheduleHeaderView.xaml
    /// </summary>
    public partial class ScheduleHeaderView
    {
        public ScheduleHeaderView()
        {
            InitializeComponent();
        }

        [Dependency]
        public ScheduleHeaderViewModel ViewModel
        {
            get { return DataContext as ScheduleHeaderViewModel; }
            set { DataContext = value; }
        }
    }
}
