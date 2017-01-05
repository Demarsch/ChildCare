using StatisticsModule.ViewModels;
using Microsoft.Practices.Unity;

namespace StatisticsModule.Views
{
    /// <summary>
    /// Interaction logic for RoomCapacityStatisticsView.xaml
    /// </summary>
    public partial class RoomCapacityStatisticsView
    {
        public RoomCapacityStatisticsView()
        {
            InitializeComponent();
        }

        [Dependency]
        public RoomCapacityStatisticsViewModel RoomCapacityStatisticsViewModel
        {
            get { return DataContext as RoomCapacityStatisticsViewModel; }
            set { DataContext = value; }
        }
    }
}