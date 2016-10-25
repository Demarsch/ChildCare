using StatisticsModule.ViewModels;
using Microsoft.Practices.Unity;

namespace StatisticsModule.Views
{
    /// <summary>
    /// Interaction logic for ScheduleStatisticsView.xaml
    /// </summary>
    public partial class ScheduleStatisticsView
    {
        public ScheduleStatisticsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public ScheduleStatisticsViewModel ScheduleStatisticsViewModel
        {
            get { return DataContext as ScheduleStatisticsViewModel; }
            set { DataContext = value; }
        }
    }
}